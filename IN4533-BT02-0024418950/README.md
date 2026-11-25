# IN4533 - Bài tập 02: EduHub mini (PostgreSQL + REST API)

- **Họ tên:** Trần Thái Kiệt
- **MSSV:** 0024418950
- **Lớp:** ĐHCNTT24A-CS
- **Học phần:** Lập trình Back End (IN4533)

## Cấu trúc thư mục

```
IN4533-BT02-0024418950/
├── sql/
│   ├── 01-schema.sql     # Tạo 3 bảng + ràng buộc
│   ├── 02-seed.sql       # Dữ liệu mẫu
│   └── 03-queries.sql    # 3 truy vấn a/b/c
├── docs/
│   ├── api.md            # Thiết kế REST API kèm JSON mẫu
│   ├── requests.md       # Các lệnh curl kiểm thử
│   └── images/           # Ảnh chụp màn hình
├── src/                  # Minimal API (ASP.NET Core + Npgsql)
└── README.md
```

## Cách cài PostgreSQL

Cài bằng Docker:

```bash
docker run --name eduhub-pg -e POSTGRES_PASSWORD=matkhau -p 5432:5432 -d postgres:16
```

Hoặc cài trực tiếp PostgreSQL 16 rồi kết nối bằng `psql` / pgAdmin.

## Cách chạy phần cơ sở dữ liệu

```bash
# Tạo db
createdb -h localhost -U postgres eduhub

# Chạy lần lượt 3 file SQL
psql -h localhost -U postgres -d eduhub -f sql/01-schema.sql
psql -h localhost -U postgres -d eduhub -f sql/02-seed.sql
psql -h localhost -U postgres -d eduhub -f sql/03-queries.sql
```

Tệp `01-schema.sql` có `DROP TABLE IF EXISTS` nên chạy lại được từ đầu nhiều lần.

