-- Education management schema (MySQL 8.0+)
-- Rules: a student has 1-3 ACTIVE course enrollments; each enrollment has a scorecard.

CREATE DATABASE IF NOT EXISTS studentdb
  CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci;
USE studentdb;

-- ---------------------------------------------------------------
-- Students
-- ---------------------------------------------------------------
CREATE TABLE students (
    id             INT UNSIGNED  NOT NULL AUTO_INCREMENT,
    name           VARCHAR(100)  NOT NULL,
    email          VARCHAR(150)  NOT NULL,
    date_of_birth  DATE          NULL,          -- store DOB, calculate age when needed
    created_at     DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at     DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    UNIQUE KEY uq_students_email (email),
    CONSTRAINT chk_students_name CHECK (CHAR_LENGTH(TRIM(name)) >= 2)
) ENGINE=InnoDB;

-- ---------------------------------------------------------------
-- Courses
-- ---------------------------------------------------------------
CREATE TABLE courses (
    id          INT UNSIGNED  NOT NULL AUTO_INCREMENT,
    code        VARCHAR(20)   NOT NULL,         -- e.g. CS101
    title       VARCHAR(150)  NOT NULL,
    credits     TINYINT UNSIGNED NOT NULL DEFAULT 3,
    is_active   BOOLEAN       NOT NULL DEFAULT TRUE,
    created_at  DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    UNIQUE KEY uq_courses_code (code),
    CONSTRAINT chk_courses_credits CHECK (credits BETWEEN 1 AND 10)
) ENGINE=InnoDB;

-- ---------------------------------------------------------------
-- Enrollments (student <-> course)
-- ---------------------------------------------------------------
CREATE TABLE enrollments (
    id           INT UNSIGNED NOT NULL AUTO_INCREMENT,
    student_id   INT UNSIGNED NOT NULL,
    course_id    INT UNSIGNED NOT NULL,
    status       ENUM('ACTIVE','COMPLETED','DROPPED') NOT NULL DEFAULT 'ACTIVE',
    enrolled_on  DATE         NOT NULL DEFAULT (CURRENT_DATE),
    PRIMARY KEY (id),
    UNIQUE KEY uq_enrollment_student_course (student_id, course_id),   -- no duplicate enrollment
    UNIQUE KEY uq_enrollment_id_course (id, course_id),                -- target for scores FK
    KEY ix_enrollments_course (course_id),
    CONSTRAINT fk_enrollments_student FOREIGN KEY (student_id) REFERENCES students (id) ON DELETE CASCADE,
    CONSTRAINT fk_enrollments_course  FOREIGN KEY (course_id)  REFERENCES courses (id)  ON DELETE RESTRICT
) ENGINE=InnoDB;

-- ---------------------------------------------------------------
-- Assessments defined per course (quiz, midterm, final ...)
-- weightage = percent of the final result; per course they should add up to 100
-- ---------------------------------------------------------------
CREATE TABLE assessments (
    id          INT UNSIGNED  NOT NULL AUTO_INCREMENT,
    course_id   INT UNSIGNED  NOT NULL,
    name        VARCHAR(80)   NOT NULL,
    max_marks   DECIMAL(6,2)  NOT NULL,
    weightage   DECIMAL(5,2)  NOT NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uq_assessment_course_name (course_id, name),
    UNIQUE KEY uq_assessment_id_course (id, course_id),                -- target for scores FK
    CONSTRAINT fk_assessments_course FOREIGN KEY (course_id) REFERENCES courses (id) ON DELETE CASCADE,
    CONSTRAINT chk_assessments_max   CHECK (max_marks > 0),
    CONSTRAINT chk_assessments_wt    CHECK (weightage > 0 AND weightage <= 100)
) ENGINE=InnoDB;

-- ---------------------------------------------------------------
-- Scores: one row per enrollment per assessment
-- The composite foreign keys force the enrollment and the assessment
-- to belong to the SAME course.
-- ---------------------------------------------------------------
CREATE TABLE scores (
    id               INT UNSIGNED NOT NULL AUTO_INCREMENT,
    enrollment_id    INT UNSIGNED NOT NULL,
    assessment_id    INT UNSIGNED NOT NULL,
    course_id        INT UNSIGNED NOT NULL,
    marks_obtained   DECIMAL(6,2) NOT NULL,
    remarks          VARCHAR(255) NULL,
    recorded_at      DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    UNIQUE KEY uq_scores_enrollment_assessment (enrollment_id, assessment_id),
    CONSTRAINT fk_scores_enrollment FOREIGN KEY (enrollment_id, course_id)
        REFERENCES enrollments (id, course_id) ON DELETE CASCADE,
    CONSTRAINT fk_scores_assessment FOREIGN KEY (assessment_id, course_id)
        REFERENCES assessments (id, course_id) ON DELETE CASCADE,
    CONSTRAINT chk_scores_marks CHECK (marks_obtained >= 0)
) ENGINE=InnoDB;

-- ---------------------------------------------------------------
-- Rule: at most 3 ACTIVE enrollments per student
-- (CHECK constraints cannot look at other rows, so use triggers)
-- ---------------------------------------------------------------
DELIMITER $$

CREATE TRIGGER trg_enrollments_max3_insert
BEFORE INSERT ON enrollments
FOR EACH ROW
BEGIN
    IF NEW.status = 'ACTIVE' AND
       (SELECT COUNT(*) FROM enrollments
         WHERE student_id = NEW.student_id AND status = 'ACTIVE') >= 3 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'A student can have at most 3 active course enrollments';
    END IF;
END$$

CREATE TRIGGER trg_enrollments_max3_update
BEFORE UPDATE ON enrollments
FOR EACH ROW
BEGIN
    -- only when a row becomes ACTIVE (for example a dropped course is re-enrolled)
    IF NEW.status = 'ACTIVE' AND OLD.status <> 'ACTIVE' AND
       (SELECT COUNT(*) FROM enrollments
         WHERE student_id = NEW.student_id AND status = 'ACTIVE' AND id <> NEW.id) >= 3 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'A student can have at most 3 active course enrollments';
    END IF;
END$$

-- Rule: marks cannot exceed the assessment's max_marks
CREATE TRIGGER trg_scores_max_insert
BEFORE INSERT ON scores
FOR EACH ROW
BEGIN
    IF NEW.marks_obtained > (SELECT max_marks FROM assessments WHERE id = NEW.assessment_id) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'marks_obtained cannot exceed the assessment max_marks';
    END IF;
END$$

CREATE TRIGGER trg_scores_max_update
BEFORE UPDATE ON scores
FOR EACH ROW
BEGIN
    IF NEW.marks_obtained > (SELECT max_marks FROM assessments WHERE id = NEW.assessment_id) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'marks_obtained cannot exceed the assessment max_marks';
    END IF;
END$$

DELIMITER ;

-- ---------------------------------------------------------------
-- Scorecard view: one row per enrollment with final % and grade
-- ---------------------------------------------------------------
CREATE OR REPLACE VIEW v_scorecard AS
SELECT
    e.id                                         AS enrollment_id,
    s.id                                         AS student_id,
    s.name                                       AS student_name,
    c.id                                         AS course_id,
    c.code                                       AS course_code,
    c.title                                      AS course_title,
    e.status                                     AS enrollment_status,
    COUNT(a.id)                                  AS total_assessments,
    COUNT(sc.id)                                 AS assessments_scored,
    ROUND(COALESCE(SUM(sc.marks_obtained / a.max_marks * a.weightage), 0), 2)
                                                 AS final_percentage,
    CASE
        WHEN COUNT(sc.id) < COUNT(a.id) THEN 'PENDING'      -- not all assessments scored yet
        WHEN SUM(sc.marks_obtained / a.max_marks * a.weightage) >= 90 THEN 'A+'
        WHEN SUM(sc.marks_obtained / a.max_marks * a.weightage) >= 80 THEN 'A'
        WHEN SUM(sc.marks_obtained / a.max_marks * a.weightage) >= 70 THEN 'B'
        WHEN SUM(sc.marks_obtained / a.max_marks * a.weightage) >= 60 THEN 'C'
        WHEN SUM(sc.marks_obtained / a.max_marks * a.weightage) >= 50 THEN 'D'
        ELSE 'F'
    END                                          AS grade
FROM enrollments e
JOIN students s          ON s.id = e.student_id
JOIN courses  c          ON c.id = e.course_id
LEFT JOIN assessments a  ON a.course_id = c.id
LEFT JOIN scores sc      ON sc.enrollment_id = e.id AND sc.assessment_id = a.id
GROUP BY e.id, s.id, s.name, c.id, c.code, c.title, e.status;

-- Example: scorecard of one student
-- SELECT * FROM v_scorecard WHERE student_id = 1;


ALTER TABLE assessments DROP CHECK chk_assessments_max;
ALTER TABLE assessments
    ADD CONSTRAINT chk_assessments_max CHECK (max_marks > 0 AND max_marks <= 100);

ALTER TABLE courses
    ADD CONSTRAINT chk_courses_code_title CHECK (code <> title);

ALTER TABLE students DROP CHECK chk_students_name;
ALTER TABLE students
    ADD CONSTRAINT chk_students_name CHECK (CHAR_LENGTH(TRIM(name)) >= 3);
