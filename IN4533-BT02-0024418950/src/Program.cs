using System.Text.Json;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string connStr = app.Configuration.GetConnectionString("EduHub")
    ?? "Host=localhost;Port=5432;Database=eduhub;Username=postgres;Password=CHANGE_ME";

var dataSource = NpgsqlDataSource.Create(connStr);

// Trả lỗi theo định dạng Problem Details (RFC 9457)
IResult Problem(int status, string type, string title, string detail, string instance)
{
    var body = new
    {
        type = "https://eduhub.vn/problems/" + type,
        title = title,
        status = status,
        detail = detail,
        instance = instance
    };
    return Results.Json(body, statusCode: status, contentType: "application/problem+json");
}

async Task<bool> CourseExists(int courseId)
{
    await using var cmd = dataSource.CreateCommand("SELECT 1 FROM courses WHERE id = @id");
    cmd.Parameters.AddWithValue("@id", courseId);
    return await cmd.ExecuteScalarAsync() != null;
}

app.MapGet("/api/v1/courses/{courseId:int}/registrations", async (int courseId) =>
{
    if (!await CourseExists(courseId))
    {
        return Problem(404, "course-not-found", "Không tìm thấy khoá học",
            "Khoá học " + courseId + " không tồn tại.",
            "/api/v1/courses/" + courseId + "/registrations");
    }

    var danhSach = new List<object>();
    await using var cmd = dataSource.CreateCommand(
        "SELECT r.id, r.member_id, r.course_id, r.status, m.full_name " +
        "FROM registrations r " +
        "JOIN members m ON m.id = r.member_id " +
        "WHERE r.course_id = @courseId " +
        "ORDER BY r.id");
    cmd.Parameters.AddWithValue("@courseId", courseId);

    await using var reader = await cmd.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        danhSach.Add(new
        {
            id = reader.GetInt32(0),
            memberId = reader.GetInt32(1),
            courseId = reader.GetInt32(2),
            status = reader.GetString(3),
            fullName = reader.GetString(4)
        });
    }

    return Results.Ok(danhSach);
});

app.MapPost("/api/v1/courses/{courseId:int}/registrations", async (int courseId, HttpRequest req) =>
{
    string instance = "/api/v1/courses/" + courseId + "/registrations";

    int memberId;
    try
    {
        var doc = await JsonDocument.ParseAsync(req.Body);
        if (!doc.RootElement.TryGetProperty("memberId", out var mProp))
        {
            return Problem(400, "missing-field", "Thiếu dữ liệu",
                "Body phải chứa trường memberId.", instance);
        }
        memberId = mProp.GetInt32();
    }
    catch
    {
        return Problem(400, "invalid-body", "Dữ liệu không hợp lệ",
            "Body không phải JSON hợp lệ.", instance);
    }

    if (!await CourseExists(courseId))
    {
        return Problem(404, "course-not-found", "Không tìm thấy khoá học",
            "Khoá học " + courseId + " không tồn tại.", instance);
    }

    await using (var cmdM = dataSource.CreateCommand("SELECT 1 FROM members WHERE id = @id"))
    {
        cmdM.Parameters.AddWithValue("@id", memberId);
        if (await cmdM.ExecuteScalarAsync() == null)
        {
            return Problem(404, "member-not-found", "Không tìm thấy thành viên",
                "Thành viên " + memberId + " không tồn tại.", instance);
        }
    }

    try
    {
        await using var cmd = dataSource.CreateCommand(
            "INSERT INTO registrations (member_id, course_id) " +
            "VALUES (@memberId, @courseId) " +
            "RETURNING id, status");
        cmd.Parameters.AddWithValue("@memberId", memberId);
        cmd.Parameters.AddWithValue("@courseId", courseId);

        await using var reader = await cmd.ExecuteReaderAsync();
        await reader.ReadAsync();
        int newId = reader.GetInt32(0);
        string status = reader.GetString(1);

        var ketQua = new { id = newId, memberId = memberId, courseId = courseId, status = status };
        return Results.Created("/api/v1/registrations/" + newId, ketQua);
    }
    catch (PostgresException ex) when (ex.SqlState == "23505")
    {
        // 23505 = unique_violation: thành viên đã đăng ký khoá này
        return Problem(409, "duplicate-registration", "Trùng đăng ký",
            "Thành viên " + memberId + " đã đăng ký khoá học " + courseId + " trước đó.",
            instance);
    }
});

app.MapPatch("/api/v1/registrations/{id:int}", async (int id, HttpRequest req) =>
{
    string instance = "/api/v1/registrations/" + id;

    string status;
    try
    {
        var doc = await JsonDocument.ParseAsync(req.Body);
        if (!doc.RootElement.TryGetProperty("status", out var sProp))
        {
            return Problem(400, "missing-field", "Thiếu dữ liệu",
                "Body phải chứa trường status.", instance);
        }
        status = sProp.GetString() ?? "";
    }
    catch
    {
        return Problem(400, "invalid-body", "Dữ liệu không hợp lệ",
            "Body không phải JSON hợp lệ.", instance);
    }

    if (status != "pending" && status != "confirmed" && status != "cancelled")
    {
        return Problem(400, "invalid-status", "Trạng thái không hợp lệ",
            "status chỉ nhận pending, confirmed hoặc cancelled.", instance);
    }

    // Phiếu đã huỷ thì không cho xác nhận lại
    await using (var cmdCheck = dataSource.CreateCommand(
        "SELECT status FROM registrations WHERE id = @id"))
    {
        cmdCheck.Parameters.AddWithValue("@id", id);
        var hienTai = await cmdCheck.ExecuteScalarAsync() as string;
        if (hienTai == null)
        {
            return Problem(404, "registration-not-found", "Không tìm thấy phiếu đăng ký",
                "Phiếu đăng ký " + id + " không tồn tại.", instance);
        }
        if (hienTai == "cancelled" && status == "confirmed")
        {
            return Problem(409, "registration-cancelled", "Phiếu đã huỷ",
                "Phiếu đăng ký " + id + " đã huỷ, không thể xác nhận.", instance);
        }
    }

    await using var cmd = dataSource.CreateCommand(
        "UPDATE registrations SET status = @status WHERE id = @id " +
        "RETURNING id, member_id, course_id, status");
    cmd.Parameters.AddWithValue("@status", status);
    cmd.Parameters.AddWithValue("@id", id);

    await using var reader = await cmd.ExecuteReaderAsync();
    if (!await reader.ReadAsync())
    {
        return Problem(404, "registration-not-found", "Không tìm thấy phiếu đăng ký",
            "Phiếu đăng ký " + id + " không tồn tại.", instance);
    }

    var ketQua = new
    {
        id = reader.GetInt32(0),
        memberId = reader.GetInt32(1),
        courseId = reader.GetInt32(2),
        status = reader.GetString(3)
    };
    return Results.Ok(ketQua);
});

app.MapDelete("/api/v1/registrations/{id:int}", async (int id) =>
{
    await using var cmd = dataSource.CreateCommand(
        "DELETE FROM registrations WHERE id = @id");
    cmd.Parameters.AddWithValue("@id", id);

    int soDong = await cmd.ExecuteNonQueryAsync();
    if (soDong == 0)
    {
        return Problem(404, "registration-not-found", "Không tìm thấy phiếu đăng ký",
            "Phiếu đăng ký " + id + " không tồn tại.",
            "/api/v1/registrations/" + id);
    }
    return Results.NoContent();
});

app.Run();
