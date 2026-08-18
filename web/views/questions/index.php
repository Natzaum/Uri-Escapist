<section class="page-actions">
    <div>
        <p class="muted">Gerencie as questões que podem aparecer nos livros do jogo.</p>
    </div>
    <a class="button button-primary" href="<?= e(url('/question-form.php')) ?>">+ Nova pergunta</a>
</section>

<section class="panel filter-panel">
    <form class="filter-form" method="get" action="<?= e(url('/questions.php')) ?>">
        <label class="search-field">
            <span class="sr-only">Pesquisar no enunciado</span>
            <input type="search" name="search" value="<?= e($filters['search']) ?>" placeholder="Pesquisar no enunciado...">
        </label>
        <label>
            <span class="sr-only">Disciplina</span>
            <select name="discipline">
                <option value="0">Todas as disciplinas</option>
                <?php foreach ($disciplines as $discipline): ?>
                    <option value="<?= (int) $discipline['id'] ?>" <?= (int) $filters['discipline'] === (int) $discipline['id'] ? 'selected' : '' ?>>
                        <?= e($discipline['name']) ?>
                    </option>
                <?php endforeach; ?>
            </select>
        </label>
        <label>
            <span class="sr-only">Status</span>
            <select name="status">
                <option value="">Todos os status</option>
                <option value="published" <?= $filters['status'] === 'published' ? 'selected' : '' ?>>Publicadas</option>
                <option value="draft" <?= $filters['status'] === 'draft' ? 'selected' : '' ?>>Rascunhos</option>
            </select>
        </label>
        <button class="button button-secondary" type="submit">Filtrar</button>
        <?php if ($filters['search'] !== '' || $filters['discipline'] > 0 || $filters['status'] !== ''): ?>
            <a class="button button-ghost" href="<?= e(url('/questions.php')) ?>">Limpar</a>
        <?php endif; ?>
    </form>
</section>

<section class="panel">
    <div class="panel-header">
        <div>
            <span class="eyebrow">Banco de conteúdo</span>
            <h2><?= count($questions) ?> <?= count($questions) === 1 ? 'resultado' : 'resultados' ?></h2>
        </div>
    </div>

    <?php if ($questions === []): ?>
        <div class="empty-state">
            <span class="empty-icon">⌕</span>
            <h3>Nenhuma pergunta encontrada</h3>
            <p>Ajuste os filtros ou cadastre uma nova questão.</p>
        </div>
    <?php else: ?>
        <div class="question-list">
            <?php foreach ($questions as $question): ?>
                <?php $options = [$question['option_a'], $question['option_b'], $question['option_c'], $question['option_d']]; ?>
                <article class="question-row question-row-detailed">
                    <div class="question-main">
                        <div class="badges">
                            <span class="badge"><?= e($question['discipline']) ?></span>
                            <span class="badge badge-difficulty"><?= e(ucfirst($question['difficulty'])) ?></span>
                            <span class="badge badge-<?= e($question['status']) ?>">
                                <?= $question['status'] === 'published' ? 'Publicada' : 'Rascunho' ?>
                            </span>
                        </div>
                        <h3><?= e($question['prompt']) ?></h3>
                        <p class="correct-answer">Resposta: <strong><?= e($options[(int) $question['correct_index']]) ?></strong></p>
                    </div>
                    <div class="row-actions">
                        <a class="button button-ghost button-small" href="<?= e(url('/question-form.php?id=' . (int) $question['id'])) ?>">Editar</a>
                        <form action="<?= e(url('/actions/question-delete.php')) ?>" method="post" data-confirm="Excluir esta pergunta? Esta ação não pode ser desfeita.">
                            <?= csrf_field() ?>
                            <input type="hidden" name="id" value="<?= (int) $question['id'] ?>">
                            <button class="button button-danger button-small" type="submit">Excluir</button>
                        </form>
                    </div>
                </article>
            <?php endforeach; ?>
        </div>
    <?php endif; ?>
</section>
