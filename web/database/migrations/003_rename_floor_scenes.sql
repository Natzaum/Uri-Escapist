INSERT INTO floors (name, slug, scene_name, active)
VALUES
    ('Andar 1', 'andar-1', 'andar1', 1),
    ('Andar 2', 'andar-2', 'andar2', 1)
ON DUPLICATE KEY UPDATE
    name = VALUES(name),
    scene_name = VALUES(scene_name),
    active = VALUES(active);
