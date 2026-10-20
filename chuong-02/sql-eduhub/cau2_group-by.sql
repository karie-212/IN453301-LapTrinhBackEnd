-- Câu 2: Đếm số thành viên của mỗi khoá, khoá nhiều người nhất đứng đầu 

SELECT c.id, c.name, COUNT(e.member_id) AS soTV
FROM courses c
LEFT JOIN enrollments e ON e.course_id = c.id 
GROUP BY c.id                                 
ORDER BY soTV DESC;
