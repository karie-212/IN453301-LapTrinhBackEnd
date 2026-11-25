-- sql/02-seed.sql
-- Dữ liệu mẫu: 4 thành viên, 3 khoá học, 6 phiếu đăng ký (đủ 3 trạng thái)
-- Riêng thành viên có id = 4 thì chưa đăng ký khoá nào

INSERT INTO members (full_name, email) VALUES
    ('Nguyễn Văn An',  'an@eduhub.vn'),
    ('Trần Thị Bình',  'binh@eduhub.vn'),
    ('Lê Hoàng Cường', 'cuong@eduhub.vn'),
    ('Phạm Minh Dũng', 'dung@eduhub.vn');

INSERT INTO courses (title, capacity) VALUES
    ('Lập trình Back End',   40),
    ('Cơ sở dữ liệu',        35),
    ('Lập trình Front-End',  30);

INSERT INTO registrations (member_id, course_id, status) VALUES
    (1, 1, 'confirmed'),
    (1, 2, 'pending'),
    (2, 1, 'confirmed'),
    (2, 3, 'cancelled'),
    (3, 2, 'confirmed'),
    (3, 3, 'pending');
