<?php

declare(strict_types=1);

require dirname(__DIR__) . '/src/question_selection.php';

function check(bool $condition, string $message): void
{
    if (!$condition) {
        throw new RuntimeException($message);
    }
}

$expected = [
    'facil' => [['facil'], ['facil', 'media'], ['media']],
    'normal' => [['facil'], ['media'], ['dificil']],
    'dificil' => [['dificil'], ['dificil'], ['dificil']],
];
foreach ($expected as $mode => $floors) {
    foreach ($floors as $index => $difficulties) {
        check(question_difficulties($mode, 'andar-' . ($index + 1)) === $difficulties, "$mode floor $index");
    }
}
foreach ([['invalid', 'andar-1'], ['normal', 'andar-4']] as [$mode, $floor]) {
    try {
        question_difficulties($mode, $floor);
        throw new RuntimeException('Invalid mode/floor was accepted.');
    } catch (InvalidArgumentException) {
        // Expected.
    }
}
$orders = [];
for ($run = 0; $run < 100; $run++) {
    $selection = select_questions([range(1, 20), range(21, 40)], 10, true);
    check(count($selection) === 10 && count(array_unique($selection)) === 10, 'Incomplete or repeated questions');
    check(count(array_filter($selection, fn ($id) => $id <= 20)) === 5, 'Mixed floor must balance categories');
    $orders[implode(',', $selection)] = true;
}
check(count($orders) > 1, 'Random selection never varied');
$selection = select_questions([[1], range(2, 20)], 10, true);
check(count($selection) === 10 && in_array(1, $selection, true), 'Sparse category must still appear');
check(count(select_questions([[1], [2]], 10, true)) === 2, 'Insufficient bank must not repeat questions');
check(select_questions([[], []], 10, true) === [], 'Empty bank');
check(select_questions([[1, 2], [3, 4]], 3, false) === [1, 3, 2], 'Deterministic selection');
echo "PASS: all nine mode/floor rules, invalid input, mix, randomization, uniqueness and shortages.\n";
