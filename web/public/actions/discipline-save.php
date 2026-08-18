<?php

declare(strict_types=1);

require dirname(__DIR__, 2) . '/src/bootstrap.php';
require_auth();

if (!is_post()) {
    http_response_code(405);
    exit('Método não permitido.');
}

verify_csrf();

$name = trim((string) ($_POST['name'] ?? ''));
$providedSlug = trim((string) ($_POST['slug'] ?? ''));
$slug = slugify($providedSlug !== '' ? $providedSlug : $name);
remember_input(['name' => $name, 'slug' => $providedSlug]);

if ($name === '' || mb_strlen($name) > 120 || $slug === '' || strlen($slug) > 120) {
    flash('error', 'Informe um nome e uma chave válida para a disciplina.');
    redirect('/disciplines.php');
}

try {
    $statement = db()->prepare('INSERT INTO disciplines (name, slug, active) VALUES (:name, :slug, 1)');
    $statement->execute(['name' => $name, 'slug' => $slug]);
    clear_old_input();
    flash('success', 'Disciplina cadastrada. Use a chave “' . $slug . '” na Unity.');
} catch (PDOException $exception) {
    if ((string) $exception->getCode() === '23000') {
        flash('error', 'Essa chave já está sendo usada por outra disciplina.');
    } else {
        throw $exception;
    }
}

redirect('/disciplines.php');
