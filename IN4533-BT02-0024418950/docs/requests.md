# Kiểm thử API bằng curl

API chạy tại `http://localhost:5000`. Mỗi lệnh kèm mã trạng thái dự kiến.
Chạy lần lượt từ trên xuống ngay sau khi nạp `sql/01-schema.sql` và `sql/02-seed.sql`
(phiếu mới tạo ở bước 2 sẽ có id = 7).

## 1. Xem danh sách đăng ký của khoá học (200 OK)

```bash
curl -i http://localhost:5000/api/v1/courses/2/registrations
```

## 2. Tạo đăng ký (201 Created)

```bash
curl -i -X POST http://localhost:5000/api/v1/courses/2/registrations -H "Content-Type: application/json" -d "{\"memberId\": 4}"
```

## 3. Xác nhận đăng ký (200 OK)

```bash
curl -i -X PATCH http://localhost:5000/api/v1/registrations/7 -H "Content-Type: application/json" -d "{\"status\": \"confirmed\"}"
```

## 4. Huỷ đăng ký (204 No Content)

```bash
curl -i -X DELETE http://localhost:5000/api/v1/registrations/7
```

## Trường hợp lỗi

```bash
# Trùng đăng ký (409 Conflict) - thành viên 3 đã đăng ký khoá 2 trong dữ liệu mẫu
curl -i -X POST http://localhost:5000/api/v1/courses/2/registrations -H "Content-Type: application/json" -d "{\"memberId\": 3}"

# Xác nhận phiếu đã huỷ (409 Conflict) - phiếu 4 có trạng thái cancelled trong dữ liệu mẫu
curl -i -X PATCH http://localhost:5000/api/v1/registrations/4 -H "Content-Type: application/json" -d "{\"status\": \"confirmed\"}"

# Khoá học không tồn tại (404 Not Found)
curl -i http://localhost:5000/api/v1/courses/999/registrations
```