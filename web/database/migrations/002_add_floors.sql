CREATE TABLE IF NOT EXISTS floors (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(80) NOT NULL,
    slug VARCHAR(80) NOT NULL UNIQUE,
    scene_name VARCHAR(120) NOT NULL UNIQUE,
    active TINYINT(1) NOT NULL DEFAULT 1,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB;

INSERT INTO floors (name, slug, scene_name, active)
VALUES
    ('Andar 1', 'andar-1', 'cenavitor', 1),
    ('Andar 2', 'andar-2', 'cena_ruan', 1)
ON DUPLICATE KEY UPDATE
    name = VALUES(name),
    scene_name = VALUES(scene_name),
    active = VALUES(active);

ALTER TABLE questions
    ADD COLUMN floor_id BIGINT UNSIGNED NULL AFTER discipline_id;

ALTER TABLE questions
    ADD CONSTRAINT fk_questions_floor
    FOREIGN KEY (floor_id) REFERENCES floors(id) ON DELETE SET NULL;
