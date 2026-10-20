-- Dữ liệu mẫu

INSERT INTO members (full_name, email) VALUES
    ('Nguyễn An',  'an@eduhub.vn'),   
    ('Trần Bình',  'binh@eduhub.vn'),  
    ('Lê Chi',     'chi@eduhub.vn'),   
    ('Phạm Dũng',  'dung@eduhub.vn'); 

INSERT INTO courses (name, capacity) VALUES
    ('Lập trình C#',       40),  
    ('Cơ sở dữ liệu',      2),   
    ('Lập trình Web',      40),  
    ('Kiến trúc phần mềm', 40);  

INSERT INTO enrollments (member_id, course_id) VALUES
    (1, 1), (1, 3),  
    (2, 1), (2, 3),  
    (3, 3),          
    (4, 1), (4, 2);  
