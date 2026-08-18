<?php

declare(strict_types=1);

$teacher = current_teacher();
$pageTitle = $pageTitle ?? 'Painel';
$activePage = $activePage ?? '';
$bodyClass = $teacher === null ? 'guest-page' : 'admin-page';
$flashes = consume_flashes();
?>
<!doctype html>
<html lang="pt-BR">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <meta name="color-scheme" content="dark">
    <title><?= e($pageTitle) ?> · <?= e(config('app.name')) ?></title>
    <link rel="stylesheet" href="<?= e(url('/assets/css/app.css')) ?>">
    <script src="<?= e(url('/assets/js/app.js')) ?>" defer></script>
</head>
<body class="<?= e($bodyClass) ?>">
<?php if ($teacher !== null): ?>
    <div class="app-shell">
        <aside class="sidebar" id="sidebar">
            <a class="brand" href="<?= e(url('/index.php')) ?>" aria-label="Página inicial">
                <span class="brand-mark">UE</span>
                <span>
                    <strong>URI Escapist</strong>
                    <small>Central de conteúdo</small>
                </span>
            </a>

            <nav class="main-nav" aria-label="Navegação principal">
                <a class="<?= $activePage === 'dashboard' ? 'active' : '' ?>" href="<?= e(url('/index.php')) ?>">
                    <span class="nav-icon">⌂</span> Visão geral
                </a>
                <a class="<?= $activePage === 'questions' ? 'active' : '' ?>" href="<?= e(url('/questions.php')) ?>">
                    <span class="nav-icon">?</span> Perguntas
                </a>
                <a class="<?= $activePage === 'disciplines' ? 'active' : '' ?>" href="<?= e(url('/disciplines.php')) ?>">
                    <span class="nav-icon">#</span> Disciplinas
                </a>
            </nav>

            <div class="sidebar-footer">
                <div class="connection-state">
                    <span class="status-dot"></span>
                    API disponível para o jogo
                </div>
                <small>Questões publicadas entram na próxima partida.</small>
            </div>
        </aside>

        <div class="workspace">
            <header class="topbar">
                <button class="menu-toggle" type="button" data-menu-toggle aria-label="Abrir menu">☰</button>
                <div>
                    <span class="eyebrow">Painel do professor</span>
                    <h1><?= e($pageTitle) ?></h1>
                </div>
                <div class="teacher-menu">
                    <span class="avatar"><?= e(mb_strtoupper(mb_substr($teacher['name'], 0, 1))) ?></span>
                    <span class="teacher-copy">
                        <strong><?= e($teacher['name']) ?></strong>
                        <small><?= e($teacher['email']) ?></small>
                    </span>
                    <form action="<?= e(url('/actions/logout.php')) ?>" method="post">
                        <?= csrf_field() ?>
                        <button class="button button-ghost button-small" type="submit">Sair</button>
                    </form>
                </div>
            </header>

            <main class="content">
<?php else: ?>
    <main class="guest-shell">
<?php endif; ?>

<?php foreach ($flashes as $flash): ?>
    <div class="alert alert-<?= e($flash['type'] ?? 'info') ?>" role="status" data-alert>
        <span><?= e($flash['message'] ?? '') ?></span>
        <button type="button" aria-label="Fechar aviso" data-alert-close>×</button>
    </div>
<?php endforeach; ?>
