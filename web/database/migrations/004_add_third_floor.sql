INSERT INTO floors (name, slug, scene_name, active)
VALUES ('Andar 3', 'andar-3', 'andar3', 1)
ON DUPLICATE KEY UPDATE
    name = VALUES(name),
    scene_name = VALUES(scene_name),
    active = VALUES(active);
