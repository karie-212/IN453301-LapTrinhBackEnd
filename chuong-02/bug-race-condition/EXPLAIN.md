# Bài 1: Hai người giành chỗ cuối - vì sao cần cô lập?

## 1. Tình huống

Khoá "Cơ sở dữ liệu" chỉ còn **1 chỗ**. An và Bình cùng đăng ký gần như đồng thời:

| Bước | Phiên A (An)            | Phiên B (Bình)          |
|------|-------------------------|-------------------------|
| 1    | SELECT: còn 1 chỗ       |                         |
| 2    |                         | SELECT: còn 1 chỗ       |
| 3    | INSERT đăng ký, COMMIT  |                         |
| 4    |                         | INSERT đăng ký, COMMIT  |

Kết quả: cả hai đều đăng ký thành công, khoá bị **vượt sức chứa** (41/40).

## 2. Vì sao xảy ra?

Đây là **race condition** (tranh chấp): kết quả phụ thuộc vào thứ tự chạy xen kẽ của các phiên.

- Mỗi phiên làm theo kiểu **"kiểm tra rồi mới làm"** (check-then-act): đọc số chỗ trống, thấy còn thì INSERT.
- Giữa lúc **kiểm tra** (bước 2) và lúc **INSERT** (bước 4) của B, phiên A đã lấy mất chỗ cuối.
- B ra quyết định dựa trên dữ liệu **đã cũ**, nên vẫn INSERT.
- Lệnh INSERT của hai người là hai dòng khác nhau, khoá chính không trùng, nên database không có lý do gì để chặn.

Mỗi giao dịch nhìn riêng thì đều đúng. Sai là do chúng **chen vào nhau**.

## 3. Vì sao cần cô lập (Isolation)?

**Isolation** là chữ **I** trong **ACID**: các giao dịch chạy đồng thời phải cho kết quả **như thể chúng chạy lần lượt từng cái một**.

Nếu A và B chạy lần lượt, B sẽ thấy "hết chỗ" và không đăng ký. Thiếu cô lập thì bước kiểm tra và bước ghi bị phiên khác chen vào giữa, dữ liệu mất tính đúng đắn (vi phạm ràng buộc sức chứa).

Mức cô lập mặc định của PostgreSQL là **READ COMMITTED**: mỗi câu lệnh chỉ thấy dữ liệu đã COMMIT. Mức này **không** ngăn được tình huống trên, vì lúc B kiểm tra thì A chưa COMMIT.

## 4. Cách xử lý

### Cách 1: Khoá hàng bằng `SELECT ... FOR UPDATE` (cách trên slide)

```sql
BEGIN;
SELECT capacity FROM courses WHERE id = 2 FOR UPDATE;  -- giữ khoá hàng
-- đếm số đăng ký, còn chỗ thì INSERT
COMMIT;
```

- `FOR UPDATE` khoá dòng khoá học id = 2 cho tới khi giao dịch kết thúc.
- Phiên B chạy cùng lệnh sẽ phải **chờ** A COMMIT hoặc ROLLBACK.
- Khi B được chạy tiếp, nó đếm lại và thấy dữ liệu mới nhất (hết chỗ), nên không INSERT.
- Thao tác "kiểm tra + ghi" lúc này được thực hiện **tuần tự**, đúng tinh thần cô lập.

### Cách 2: Kiểm tra và ghi trong cùng một câu lệnh

```sql
INSERT INTO enrollments (member_id, course_id)
SELECT 2, 2
WHERE (SELECT COUNT(*) FROM enrollments WHERE course_id = 2)
    < (SELECT capacity FROM courses WHERE id = 2);
```

Gộp kiểm tra vào lệnh INSERT giúp thu hẹp khoảng hở. Tuy vậy, ở mức READ COMMITTED hai phiên vẫn có thể cùng đếm trước khi bên kia COMMIT, nên cần kết hợp với khoá (`FOR UPDATE`) hoặc mức cô lập cao hơn.

### Cách 3: Nâng mức cô lập lên `SERIALIZABLE`

```sql
BEGIN ISOLATION LEVEL SERIALIZABLE;
-- kiểm tra + INSERT
COMMIT;
```

PostgreSQL phát hiện hai giao dịch xung đột và **huỷ một bên** với lỗi `could not serialize access`. Ứng dụng cần bắt lỗi đó và cho người dùng thử lại.

## 5. Kết luận

- Hai người cùng thấy "còn 1 chỗ" là vì bước **kiểm tra** và bước **ghi** không được bảo vệ khỏi phiên khác.
- Cô lập đảm bảo các giao dịch đồng thời không làm hỏng dữ liệu của nhau.
- Cách thực tế và dễ hiểu nhất là `SELECT ... FOR UPDATE`: khoá dòng cần kiểm tra để các phiên phải xếp hàng.