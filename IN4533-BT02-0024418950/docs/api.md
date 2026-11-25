# Thiết kế REST API - registrations (EduHub mini)

## 2.1. Bảng thiết kế bốn chức năng

| Chức năng | Method | URI | Mã thành công | Các mã lỗi có thể gặp |
|---|---|---|---|---|
| Xem danh sách đăng ký của một khoá học | GET | `/api/v1/courses/{courseId}/registrations` | 200 OK | 404 Not Found (khoá học không tồn tại) |
| Tạo đăng ký | POST | `/api/v1/courses/{courseId}/registrations` | 201 Created | 400 Bad Request (thiếu/sai dữ liệu), 404 Not Found (khoá học/thành viên không tồn tại), 409 Conflict (trùng đăng ký) |
| Xác nhận đăng ký | PATCH | `/api/v1/registrations/{id}` | 200 OK | 400 Bad Request (trạng thái không hợp lệ), 404 Not Found (phiếu không tồn tại), 409 Conflict (phiếu đã huỷ, không thể xác nhận) |
| Huỷ đăng ký | DELETE | `/api/v1/registrations/{id}` | 204 No Content | 404 Not Found (phiếu không tồn tại) |
