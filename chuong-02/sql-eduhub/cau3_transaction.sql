-- Câu 3: Chuyển Lê Chi vào khoá "Cơ sở dữ liệu" bằng một transaction

BEGIN;

INSERT INTO enrollments (member_id, course_id)
VALUES (
    (SELECT id FROM members WHERE full_name = 'Lê Chi'),
    (SELECT id FROM courses WHERE name = 'Cơ sở dữ liệu')
);

COMMIT;
-- Nếu có lỗi giữa chừng: chạy ROLLBACK; thay cho COMMIT;

-- Kiểm tra lại
SELECT m.full_name, c.name
FROM members m
JOIN enrollments e ON e.member_id = m.id
JOIN courses c ON c.id = e.course_id
WHERE m.full_name = 'Lê Chi'; 

