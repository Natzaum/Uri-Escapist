<?php

declare(strict_types=1);

require dirname(__DIR__) . '/src/question_seed.php';

function verify(bool $condition, string $message): void
{
    if (!$condition) {
        throw new RuntimeException($message);
    }
}

$questions = load_question_seed(dirname(__DIR__) . '/database/seeds/computer_science_questions.json');
$topics = [];
foreach ($questions as $question) {
    $key = $question['difficulty'] . '/' . $question['discipline'];
    $topics[$key] = ($topics[$key] ?? 0) + 1;
}
verify(count($topics) === 30 && array_unique(array_values($topics)) === [3], 'Expected three questions per topic and difficulty');
$db = new PDO('sqlite::memory:', null, null, [PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION]);
$db->exec("PRAGMA foreign_keys = ON;
    CREATE TABLE teachers (id INTEGER PRIMARY KEY);
    INSERT INTO teachers VALUES (1);
    CREATE TABLE disciplines (id INTEGER PRIMARY KEY, name TEXT, slug TEXT UNIQUE, active INTEGER);
    CREATE TABLE questions (id INTEGER PRIMARY KEY, discipline_id INTEGER REFERENCES disciplines(id),
        teacher_id INTEGER REFERENCES teachers(id), floor_id INTEGER, prompt TEXT,
        option_a TEXT, option_b TEXT, option_c TEXT, option_d TEXT, correct_index INTEGER,
        difficulty TEXT, status TEXT)");
$db->beginTransaction();
$result = import_question_seed($db, 1, $questions);
$db->commit();
verify($result === ['inserted' => 90, 'skipped' => 0], 'Initial import');
verify((int) $db->query('SELECT COUNT(*) FROM disciplines')->fetchColumn() === 10, 'Topic creation');
foreach ($db->query('SELECT difficulty, COUNT(*) AS total FROM questions GROUP BY difficulty') as $row) {
    verify((int) $row['total'] === 30, 'Difficulty distribution');
}
verify((int) $db->query("SELECT COUNT(*) FROM questions WHERE status = 'published' AND floor_id IS NULL")->fetchColumn() === 90, 'Publication and floor');
$stored = $db->query('SELECT * FROM questions ORDER BY id')->fetchAll(PDO::FETCH_ASSOC);
foreach ($stored as $index => $row) {
    $seed = $questions[$index];
    $field = ['option_a', 'option_b', 'option_c', 'option_d'][(int) $row['correct_index']];
    verify($row[$field] === $seed['options'][$seed['correctIndex']], 'Answer mapping');
}
// Repeat imports preserve editorial changes as long as the original prompt remains.
$db->exec("UPDATE questions SET status = 'draft', option_a = 'Alteração do professor' WHERE id = 1");
$db->beginTransaction();
$result = import_question_seed($db, 1, $questions);
$db->commit();
verify($result === ['inserted' => 0, 'skipped' => 90], 'Repeat import must skip all questions');
verify($db->query('SELECT option_a FROM questions WHERE id = 1')->fetchColumn() === 'Alteração do professor', 'Preserve edits');
// A mid-import error must not leave a partially imported bank.
$db->exec("DELETE FROM questions; UPDATE disciplines SET active = 0 WHERE slug = 'redes'");
$db->beginTransaction();
$rejected = false;
try {
    import_question_seed($db, 1, $questions);
} catch (RuntimeException) {
    $rejected = true;
}
$db->rollBack();
verify($rejected && (int) $db->query('SELECT COUNT(*) FROM questions')->fetchColumn() === 0, 'Atomic rollback');
echo "PASS: 90 questions, topic/difficulty counts, import, answer mapping, repeat import, editorial preservation and rollback.\n";
