# Bài 1: Hai người giành chỗ cuối - vì sao cần cô lập?

## 1. Tình huống

Khoá "Cơ sở dữ liệu" chỉ còn **1 chỗ**. An và Bình bấm đăng ký gần như cùng lúc:

| Bước | An (phiên A)          | Bình (phiên B)        |
|------|-----------------------|-----------------------|
| 1    | Xem: còn 1 chỗ        |                       |
| 2    |                       | Xem: còn 1 chỗ        |
| 3    | Đăng ký, COMMIT       |                       |
| 4    |                       | Đăng ký, COMMIT       |

Kết quả: cả hai đều đăng ký thành công, khoá bị **41/40 người**.

## 2. Vì sao xảy ra?

Lỗi này gọi là **race condition** (tranh chấp): hai người cùng "chạy đua" giành một thứ, và kết quả phụ thuộc vào ai nhanh hơn.

Cả hai đều làm theo kiểu **"xem trước rồi mới làm"**:

- Bình **xem** số chỗ ở bước 2, lúc đó An **chưa** đăng ký xong, nên Bình thấy còn 1 chỗ.
- Đến bước 4, Bình đăng ký dựa trên thông tin **đã cũ**, vì chỗ đó An đã lấy mất ở bước 3.
- Database không chặn lại, vì An và Bình là hai dòng đăng ký khác nhau, không trùng khoá chính.

Nói ngắn gọn: nếu chạy riêng thì mỗi người đều làm đúng. Sai là do hai người **chen vào giữa nhau**.

## 3. Vì sao cần cô lập (Isolation)?

**Cô lập** là chữ **I** trong **ACID** (4 tính chất của giao dịch). Nó đảm bảo:

> Nhiều giao dịch chạy cùng lúc nhưng kết quả phải giống như chúng **chạy lần lượt từng cái một**.

Nếu An làm xong rồi Bình mới làm, Bình sẽ thấy "hết chỗ" và không đăng ký nữa. Đó là kết quả đúng.

PostgreSQL mặc định dùng mức cô lập **READ COMMITTED**, nghĩa là chỉ thấy dữ liệu **đã COMMIT**. Ở bước 2, An chưa COMMIT nên Bình vẫn thấy còn chỗ. Vì vậy mức mặc định **không đủ** để chặn lỗi này, ta phải tự xử lý thêm.

## 4. Cách xử lý

### Cách 1: Khoá dòng bằng `SELECT ... FOR UPDATE` (cách trên slide)

```sql
BEGIN;
SELECT capacity FROM courses WHERE id = 2 FOR UPDATE;  -- giữ khoá hàng
-- đếm số đăng ký, còn chỗ thì INSERT
COMMIT;
```

Cách này giống như **cầm chìa khoá phòng**:

- An chạy `FOR UPDATE` trước, nên An giữ "chìa khoá" của dòng khoá học id = 2.
- Bình chạy cùng lệnh thì phải **đứng chờ**, vì chìa khoá đang ở chỗ An.
- An đăng ký xong và COMMIT, tức là trả chìa khoá.
- Lúc này Bình mới được chạy tiếp, đếm lại và thấy **hết chỗ**, nên không đăng ký.

### Cách 2: Kiểm tra ngay trong lệnh INSERT

```sql
INSERT INTO enrollments (member_id, course_id)
SELECT 2, 2
WHERE (SELECT COUNT(*) FROM enrollments WHERE course_id = 2)
    < (SELECT capacity FROM courses WHERE id = 2);
```

Gộp "xem" và "ghi" vào chung một lệnh: chỉ INSERT nếu số người đăng ký còn ít hơn sức chứa. Cách này giảm khả năng bị chen ngang, nhưng **chưa chắc chắn 100%**, vì hai người vẫn có thể đếm cùng lúc. Nên dùng kèm với Cách 1 hoặc Cách 3.

### Cách 3: Nâng mức cô lập lên `SERIALIZABLE`

```sql
BEGIN ISOLATION LEVEL SERIALIZABLE;
-- kiểm tra + INSERT
COMMIT;
```

Đây là mức cô lập **cao nhất**. PostgreSQL tự phát hiện khi hai giao dịch giẫm chân nhau, và **huỷ một bên** với lỗi `could not serialize access`. Chương trình cần bắt lỗi này và cho người dùng thử lại.

## 5. Kết luận

- Hai người cùng thấy "còn 1 chỗ" vì bước **xem** và bước **ghi** bị người khác chen vào giữa.
- **Cô lập** giúp các giao dịch chạy cùng lúc mà không làm sai dữ liệu của nhau.
- Cách dễ hiểu và hay dùng nhất là `SELECT ... FOR UPDATE`: khoá dòng lại để mọi người phải **xếp hàng**, ai đến trước làm trước.