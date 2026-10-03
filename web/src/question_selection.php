<?php

declare(strict_types=1);

/** The third-floor rule is reserved; only registered, active scenes can use it. */
function question_difficulties(string $mode, string $floor): array
{
    $rules = [
        'facil' => ['andar-1' => ['facil'], 'andar-2' => ['facil', 'media'], 'andar-3' => ['media']],
        'normal' => ['andar-1' => ['facil'], 'andar-2' => ['media'], 'andar-3' => ['dificil']],
        'dificil' => ['andar-1' => ['dificil'], 'andar-2' => ['dificil'], 'andar-3' => ['dificil']],
    ];
    if (!isset($rules[$mode][$floor])) {
        throw new InvalidArgumentException('Modo ou andar sem regra de dificuldade.');
    }
    return $rules[$mode][$floor];
}

/** Round-robin pools guarantee a mix when both categories have content. No repeats. */
function select_questions(array $pools, int $limit, bool $random): array
{
    if ($random) {
        foreach ($pools as &$pool) {
            shuffle($pool);
        }
        unset($pool);
        shuffle($pools);
    }
    $selected = [];
    while (count($selected) < $limit) {
        $added = false;
        foreach ($pools as &$pool) {
            if ($pool !== [] && count($selected) < $limit) {
                $selected[] = array_shift($pool);
                $added = true;
            }
        }
        unset($pool);
        if (!$added) {
            break;
        }
    }
    if ($random) {
        shuffle($selected);
    }
    return $selected;
}
