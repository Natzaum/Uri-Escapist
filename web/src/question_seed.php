<?php

declare(strict_types=1);

function load_question_seed(string $path): array
{
    $content = file_get_contents($path);
    if ($content === false) {
        throw new RuntimeException('Não foi possível ler o arquivo de perguntas.');
    }
    $questions = json_decode($content, true, 512, JSON_THROW_ON_ERROR);
    if (!is_array($questions) || !array_is_list($questions) || count($questions) !== 90) {
        throw new RuntimeException('O banco inicial deve conter exatamente 90 perguntas.');
    }
    $counts = ['facil' => 0, 'media' => 0, 'dificil' => 0];
    $prompts = [];
    foreach ($questions as $index => $question) {
        if (!is_array($question)) {
            throw new RuntimeException('Pergunta inválida na posição ' . $index);
        }
        foreach (['prompt' => 500, 'discipline' => 120, 'disciplineName' => 120] as $field => $max) {
            if (!is_string($question[$field] ?? null) || trim($question[$field]) === ''
                || mb_strlen($question[$field]) > $max) {
                throw new RuntimeException("Campo {$field} inválido na pergunta " . ($index + 1));
            }
        }
        if (!preg_match('/^[a-z0-9-]+$/', $question['discipline'])
            || !is_string($question['difficulty'] ?? null)
            || !array_key_exists($question['difficulty'], $counts)
            || !is_int($question['correctIndex'] ?? null)
            || $question['correctIndex'] < 0 || $question['correctIndex'] > 3
            || !is_array($question['options'] ?? null)
            || !array_is_list($question['options']) || count($question['options']) !== 4) {
            throw new RuntimeException('Configuração inválida na pergunta ' . ($index + 1));
        }
        $normalizedOptions = [];
        foreach ($question['options'] as $option) {
            if (!is_string($option) || trim($option) === '' || mb_strlen($option) > 255) {
                throw new RuntimeException('Alternativa inválida na pergunta ' . ($index + 1));
            }
            $normalizedOptions[] = mb_strtolower(trim($option), 'UTF-8');
        }
        $promptKey = mb_strtolower(trim($question['prompt']), 'UTF-8');
        if (count(array_unique($normalizedOptions)) !== 4 || isset($prompts[$promptKey])) {
            throw new RuntimeException('Pergunta ou alternativa duplicada na posição ' . ($index + 1));
        }
        $prompts[$promptKey] = true;
        $counts[$question['difficulty']]++;
    }
    if ($counts !== ['facil' => 30, 'media' => 30, 'dificil' => 30]) {
        throw new RuntimeException('São necessárias exatamente 30 perguntas por dificuldade.');
    }
    return $questions;
}

/** Caller owns the transaction and locks the teacher row to serialize repeat imports. */
function import_question_seed(PDO $connection, int $teacherId, array $questions): array
{
    if (!$connection->inTransaction()) {
        throw new LogicException('A importação precisa de uma transação.');
    }
    $disciplineQuery = $connection->prepare('SELECT id, active FROM disciplines WHERE slug = :slug');
    $disciplineInsert = $connection->prepare('INSERT INTO disciplines (name, slug, active) VALUES (:name, :slug, 1)');
    $existing = $connection->prepare('SELECT id FROM questions WHERE teacher_id = :teacher AND prompt = :prompt LIMIT 1');
    $insert = $connection->prepare(
        "INSERT INTO questions (discipline_id, floor_id, teacher_id, prompt,
            option_a, option_b, option_c, option_d, correct_index, difficulty, status)
         VALUES (:discipline, NULL, :teacher, :prompt, :a, :b, :c, :d, :correct, :difficulty, 'published')"
    );
    $disciplineIds = [];
    $result = ['inserted' => 0, 'skipped' => 0];
    foreach ($questions as $question) {
        $existing->execute(['teacher' => $teacherId, 'prompt' => $question['prompt']]);
        if ($existing->fetchColumn() !== false) {
            $result['skipped']++;
            continue;
        }
        $slug = $question['discipline'];
        if (!isset($disciplineIds[$slug])) {
            $disciplineQuery->execute(['slug' => $slug]);
            $discipline = $disciplineQuery->fetch(PDO::FETCH_ASSOC);
            if ($discipline && (int) $discipline['active'] !== 1) {
                throw new RuntimeException("A disciplina {$slug} está inativa. Ative-a no painel antes de importar.");
            }
            if (!$discipline) {
                $disciplineInsert->execute(['name' => $question['disciplineName'], 'slug' => $slug]);
                $disciplineIds[$slug] = (int) $connection->lastInsertId();
            } else {
                $disciplineIds[$slug] = (int) $discipline['id'];
            }
        }
        $insert->execute([
            'discipline' => $disciplineIds[$slug], 'teacher' => $teacherId,
            'prompt' => $question['prompt'], 'a' => $question['options'][0],
            'b' => $question['options'][1], 'c' => $question['options'][2],
            'd' => $question['options'][3], 'correct' => $question['correctIndex'],
            'difficulty' => $question['difficulty'],
        ]);
        $result['inserted']++;
    }
    return $result;
}
