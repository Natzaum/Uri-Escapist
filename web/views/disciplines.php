<section class="split-layout">
    <section class="panel">
        <div class="panel-header simple">
            <div>
                <span class="eyebrow">Organização</span>
                <h2>Disciplinas cadastradas</h2>
            </div>
        </div>

        <div class="discipline-list">
            <?php foreach ($disciplines as $discipline): ?>
                <article class="discipline-row <?= (int) $discipline['active'] === 0 ? 'is-inactive' : '' ?>">
                    <div class="discipline-symbol"><?= e(mb_strtoupper(mb_substr($discipline['name'], 0, 2))) ?></div>
                    <div class="discipline-copy">
                        <div class="badges">
                            <h3><?= e($discipline['name']) ?></h3>
                            <span class="badge badge-<?= (int) $discipline['active'] === 1 ? 'published' : 'draft' ?>">
                                <?= (int) $discipline['active'] === 1 ? 'Ativa' : 'Inativa' ?>
                            </span>
                        </div>
                        <code><?= e($discipline['slug']) ?></code>
                        <small>
                            <?= (int) $discipline['question_count'] ?> questões ·
                            <?= (int) ($discipline['published_count'] ?? 0) ?> publicadas
                        </small>
                    </div>
                    <form action="<?= e(url('/actions/discipline-toggle.php')) ?>" method="post">
                        <?= csrf_field() ?>
                        <input type="hidden" name="id" value="<?= (int) $discipline['id'] ?>">
                        <button class="button button-ghost button-small" type="submit">
                            <?= (int) $discipline['active'] === 1 ? 'Desativar' : 'Ativar' ?>
                        </button>
                    </form>
                </article>
            <?php endforeach; ?>
        </div>
    </section>

    <aside class="panel create-discipline-card">
        <span class="eyebrow">Nova categoria</span>
        <h2>Cadastrar disciplina</h2>
        <p class="muted">A disciplina organiza e identifica o conteúdo cadastrado pelo professor.</p>

        <form class="stack-form" action="<?= e(url('/actions/discipline-save.php')) ?>" method="post">
            <?= csrf_field() ?>
            <label>
                <span>Nome</span>
                <input type="text" name="name" value="<?= e(old('name')) ?>" maxlength="120" placeholder="Ex.: Computação Gráfica" required>
            </label>
            <label>
                <span>Identificador interno <small>(opcional)</small></span>
                <input type="text" name="slug" value="<?= e(old('slug')) ?>" maxlength="120" pattern="[a-z0-9-]+" placeholder="computacao-grafica">
            </label>
            <button class="button button-primary button-wide" type="submit">Cadastrar disciplina</button>
        </form>

        <div class="info-box">
            <strong>Integração automática</strong>
            <p>A disciplina organiza o conteúdo no painel. A Unity escolhe as perguntas pelo andar associado ao nome da cena, sem configurar a matéria no BookManager.</p>
        </div>
    </aside>
</section>
