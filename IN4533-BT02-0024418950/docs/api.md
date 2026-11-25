# Thiết kế REST API - registrations (EduHub mini)

## 2.1. Bảng thiết kế bốn chức năng

| Chức năng | Method | URI | Mã thành công | Các mã lỗi có thể gặp |
|---|---|---|---|---|
| Xem danh sách đăng ký của một khoá học | GET | `/api/v1/courses/{courseId}/registrations` | 200 OK | 404 Not Found (khoá học không tồn tại) |
| Tạo đăng ký | POST | `/api/v1/courses/{courseId}/registrations` | 201 Created | 400 Bad Request (thiếu/sai dữ liệu), 404 Not Found (khoá học/thành viên không tồn tại), 409 Conflict (trùng đăng ký) |
| Xác nhận đăng ký | PATCH | `/api/v1/registrations/{id}` | 200 OK | 400 Bad Request (trạng thái không hợp lệ), 404 Not Found (phiếu không tồn tại), 409 Conflict (phiếu đã huỷ, không thể xác nhận) |
| Huỷ đăng ký | DELETE | `/api/v1/registrations/{id}` | 204 No Content | 404 Not Found (phiếu không tồn tại) |

## 2.2. JSON request/response mẫu - Tạo đăng ký

### Request

```
POST /api/v1/courses/2/registrations
Content-Type: application/json

{ "memberId": 4 }
```

### Response thành công - 201 Created

```
201 Created
Content-Type: application/json
Location: /api/v1/registrations/7

{
  "id": 7,
  "memberId": 4,
  "courseId": 2,
  "status": "pending"
}
```

### Response lỗi - trùng đăng ký - 409 Conflict (Problem Details, RFC 9457)

```
409 Conflict
Content-Type: application/problem+json

{
  "type": "https://eduhub.vn/problems/duplicate-registration",
  "title": "Trùng đăng ký",
  "status": 409,
  "detail": "Thành viên 4 đã đăng ký khoá học 2 trước đó.",
  "instance": "/api/v1/courses/2/registrations"
}
```
