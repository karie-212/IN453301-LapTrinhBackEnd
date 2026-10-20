-- Tạo bảng cho EduHub

-- Xoá bảng cũ nếu có (chạy lại nhiều lần không lỗi)
DROP TABLE IF EXISTS enrollments;
DROP TABLE IF EXISTS courses;
DROP TABLE IF EXISTS members;

CREATE TABLE members (
    id        SERIAL PRIMARY KEY,
    full_name VARCHAR(100) NOT NULL,
    email     VARCHAR(100) UNIQUE NOT NULL
);

CREATE TABLE courses (
    id       SERIAL PRIMARY KEY,
    name     VARCHAR(100) NOT NULL,
    capacity INT NOT NULL DEFAULT 40  -- sức chứa tối đa
);

CREATE TABLE enrollments (
    member_id   INT NOT NULL REFERENCES members(id) ON DELETE CASCADE,
    course_id   INT NOT NULL REFERENCES courses(id) ON DELETE CASCADE,
    enrolled_at TIMESTAMP DEFAULT NOW(),
    PRIMARY KEY (member_id, course_id)  -- không cho đăng ký trùng
);
