<?php

declare(strict_types=1);

if (PHP_SAPI !== 'cli') {
    http_response_code(404);
    exit;
}
require dirname(__DIR__) . '/src/bootstrap.php';
require dirname(__DIR__) . '/src/question_seed.php';

$arguments = getopt('', ['email:', 'validate-only']);
$connection = null;
try {
    $questions = load_question_seed(dirname(__DIR__) . '/database/seeds/computer_science_questions.json');
    if (array_key_exists('validate-only', $arguments)) {
        fwrite(STDOUT, "Banco validado: 90 perguntas, sendo 30 fáceis, 30 médias e 30 difíceis. Nenhuma gravação realizada.\n");
        exit(0);
    }
    $connection = db();
    $connection->beginTransaction();
    $email = trim((string) ($arguments['email'] ?? getenv('INITIAL_TEACHER_EMAIL') ?: ''));
    if ($email !== '') {
        $teacherQuery = $connection->prepare('SELECT id, email FROM teachers WHERE email = :email AND active = 1 FOR UPDATE');
        $teacherQuery->execute(['email' => $email]);
        $teachers = $teacherQuery->fetchAll();
    } else {
        $teachers = $connection->query('SELECT id, email FROM teachers WHERE active = 1 ORDER BY id FOR UPDATE')->fetchAll();
    }
    if (count($teachers) !== 1) {
        throw new RuntimeException('Informe uma conta ativa existente com --email=professor@exemplo.com. Nenhum dado foi importado.');
    }
    $teacher = $teachers[0];
    $result = import_question_seed($connection, (int) $teacher['id'], $questions);
    $connection->commit();
    fwrite(STDOUT, "Professor: {$teacher['email']}\n");
    fwrite(STDOUT, "Perguntas publicadas inseridas: {$result['inserted']}. Já existentes, preservadas: {$result['skipped']}.\n");
} catch (Throwable $exception) {
    if ($connection instanceof PDO && $connection->inTransaction()) {
        $connection->rollBack();
    }
    fwrite(STDERR, 'Importação cancelada: ' . $exception->getMessage() . "\n");
    exit(1);
}
