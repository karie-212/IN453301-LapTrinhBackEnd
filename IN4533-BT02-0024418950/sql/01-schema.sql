-- sql/01-schema.sql
-- members, courses, registrations

DROP TABLE IF EXISTS registrations;
DROP TABLE IF EXISTS courses;
DROP TABLE IF EXISTS members;

CREATE TABLE members (
    id        INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    full_name VARCHAR(100) NOT NULL,
    email     VARCHAR(150) NOT NULL UNIQUE
);

CREATE TABLE courses (
    id       INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    title    VARCHAR(150) NOT NULL,
    capacity INT NOT NULL DEFAULT 30
);

CREATE TABLE registrations (
    id         INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    member_id  INT NOT NULL,
    course_id  INT NOT NULL,
    status     VARCHAR(20) NOT NULL DEFAULT 'pending',
    created_at TIMESTAMP NOT NULL DEFAULT now(),

    CONSTRAINT uq_registration UNIQUE (member_id, course_id),

    CONSTRAINT fk_registration_member
        FOREIGN KEY (member_id) REFERENCES members (id) ON DELETE CASCADE,
    CONSTRAINT fk_registration_course
        FOREIGN KEY (course_id) REFERENCES courses (id) ON DELETE CASCADE,

    -- 3 trạng thái hợp lệ
    CONSTRAINT chk_status
        CHECK (status IN ('pending', 'confirmed', 'cancelled'))
);
