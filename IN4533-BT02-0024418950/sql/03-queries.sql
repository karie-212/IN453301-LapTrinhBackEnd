-- sql/03-queries.sql

-- (a) Họ tên thành viên và tên khoá học của các phiếu confirmed, sắp xếp theo họ tên
SELECT m.full_name, c.title
FROM registrations r
JOIN members m ON m.id = r.member_id
JOIN courses c ON c.id = r.course_id
WHERE r.status = 'confirmed'
ORDER BY m.full_name;

-- (b) Số phiếu đăng ký của từng khoá học, kể cả khoá chưa có phiếu nào 
SELECT c.title, COUNT(r.id) AS so_dang_ky
FROM courses c
LEFT JOIN registrations r ON r.course_id = c.id
GROUP BY c.id, c.title
ORDER BY so_dang_ky DESC, c.title;

-- (c) Các thành viên chưa đăng ký khoá nào
SELECT m.full_name, m.email
FROM members m
LEFT JOIN registrations r ON r.member_id = m.id
WHERE r.id IS NULL
ORDER BY m.full_name;
