CREATE DATABASE IF NOT EXISTS uri_escapist
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE uri_escapist;

CREATE TABLE IF NOT EXISTS teachers (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(120) NOT NULL,
    email VARCHAR(190) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    active TINYINT(1) NOT NULL DEFAULT 1,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS disciplines (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(120) NOT NULL,
    slug VARCHAR(120) NOT NULL UNIQUE,
    active TINYINT(1) NOT NULL DEFAULT 1,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS questions (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    discipline_id BIGINT UNSIGNED NOT NULL,
    teacher_id BIGINT UNSIGNED NOT NULL,
    prompt VARCHAR(500) NOT NULL,
    option_a VARCHAR(255) NOT NULL,
    option_b VARCHAR(255) NOT NULL,
    option_c VARCHAR(255) NOT NULL,
    option_d VARCHAR(255) NOT NULL,
    correct_index TINYINT UNSIGNED NOT NULL,
    difficulty ENUM('facil', 'media', 'dificil') NOT NULL DEFAULT 'media',
    status ENUM('draft', 'published') NOT NULL DEFAULT 'draft',
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_questions_discipline
        FOREIGN KEY (discipline_id) REFERENCES disciplines(id),
    CONSTRAINT fk_questions_teacher
        FOREIGN KEY (teacher_id) REFERENCES teachers(id),
    CONSTRAINT chk_questions_correct_index CHECK (correct_index BETWEEN 0 AND 3),
    INDEX idx_questions_publication (discipline_id, status),
    INDEX idx_questions_teacher (teacher_id)
) ENGINE=InnoDB;

INSERT INTO disciplines (name, slug, active)
VALUES ('Geral', 'geral', 1)
ON DUPLICATE KEY UPDATE name = VALUES(name);
