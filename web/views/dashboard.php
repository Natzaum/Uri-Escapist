<section class="hero-panel">
    <div>
        <span class="eyebrow">Conteúdo educacional</span>
        <h2>Prepare a próxima partida.</h2>
        <p>Cadastre perguntas, revise as alternativas e publique quando estiver tudo pronto. A Unity carrega o conteúdo publicado ao iniciar a fase.</p>
    </div>
    <a class="button button-primary" href="<?= e(url('/question-form.php')) ?>">+ Nova pergunta</a>
</section>

<section class="stats-grid" aria-label="Resumo das questões">
    <article class="stat-card stat-accent">
        <span class="stat-label">Total de perguntas</span>
        <strong><?= (int) ($summary['total'] ?? 0) ?></strong>
        <small>No seu banco de conteúdo</small>
    </article>
    <article class="stat-card">
        <span class="stat-label">Publicadas</span>
        <strong><?= (int) ($summary['published'] ?? 0) ?></strong>
        <small>Disponíveis para a Unity</small>
    </article>
    <article class="stat-card">
        <span class="stat-label">Rascunhos</span>
        <strong><?= (int) ($summary['drafts'] ?? 0) ?></strong>
        <small>Aguardando sua revisão</small>
    </article>
    <article class="stat-card">
        <span class="stat-label">Disciplinas ativas</span>
        <strong><?= $disciplinesCount ?></strong>
        <small>Organizando as fases</small>
    </article>
</section>

<section class="panel">
    <div class="panel-header">
        <div>
            <span class="eyebrow">Atividade recente</span>
            <h2>Últimas perguntas</h2>
        </div>
        <a class="text-link" href="<?= e(url('/questions.php')) ?>">Ver todas →</a>
    </div>

    <?php if ($recentQuestions === []): ?>
        <div class="empty-state">
            <span class="empty-icon">?</span>
            <h3>Seu banco ainda está vazio</h3>
            <p>Crie a primeira pergunta e associe-a a uma disciplina.</p>
            <a class="button button-secondary" href="<?= e(url('/question-form.php')) ?>">Criar primeira pergunta</a>
        </div>
    <?php else: ?>
        <div class="question-list compact-list">
            <?php foreach ($recentQuestions as $question): ?>
                <article class="question-row">
                    <div class="question-main">
                        <div class="badges">
                            <span class="badge"><?= e($question['discipline']) ?></span>
                            <span class="badge badge-<?= e($question['status']) ?>">
                                <?= $question['status'] === 'published' ? 'Publicada' : 'Rascunho' ?>
                            </span>
                        </div>
                        <h3><?= e($question['prompt']) ?></h3>
                        <small>Atualizada em <?= e(date('d/m/Y H:i', strtotime($question['updated_at']))) ?></small>
                    </div>
                    <a class="icon-button" href="<?= e(url('/question-form.php?id=' . (int) $question['id'])) ?>" aria-label="Editar pergunta">Editar</a>
                </article>
            <?php endforeach; ?>
        </div>
    <?php endif; ?>
</section>
