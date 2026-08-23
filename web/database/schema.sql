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

CREATE TABLE IF NOT EXISTS floors (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(80) NOT NULL,
    slug VARCHAR(80) NOT NULL UNIQUE,
    scene_name VARCHAR(120) NOT NULL UNIQUE,
    active TINYINT(1) NOT NULL DEFAULT 1,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS questions (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    discipline_id BIGINT UNSIGNED NOT NULL,
    floor_id BIGINT UNSIGNED NULL,
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
    CONSTRAINT fk_questions_floor
        FOREIGN KEY (floor_id) REFERENCES floors(id) ON DELETE SET NULL,
    CONSTRAINT fk_questions_teacher
        FOREIGN KEY (teacher_id) REFERENCES teachers(id),
    CONSTRAINT chk_questions_correct_index CHECK (correct_index BETWEEN 0 AND 3),
    INDEX idx_questions_publication (floor_id, discipline_id, status),
    INDEX idx_questions_teacher (teacher_id)
) ENGINE=InnoDB;

INSERT INTO disciplines (name, slug, active)
VALUES ('Geral', 'geral', 1)
ON DUPLICATE KEY UPDATE name = VALUES(name);

INSERT INTO floors (name, slug, scene_name, active)
VALUES
    ('Andar 1', 'andar-1', 'cenavitor', 1),
    ('Andar 2', 'andar-2', 'cena_ruan', 1)
ON DUPLICATE KEY UPDATE
    name = VALUES(name),
    scene_name = VALUES(scene_name),
    active = VALUES(active);

CREATE TABLE IF NOT EXISTS schema_migrations (
    name VARCHAR(190) PRIMARY KEY,
    applied_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

INSERT INTO schema_migrations (name)
VALUES ('002_add_floors.sql')
ON DUPLICATE KEY UPDATE name = VALUES(name);
