<?php
$questionId = (int) $question['id'];
$selectedCorrect = (int) old('correct_index', $question['correct_index']);
?>
<section class="page-actions">
    <div>
        <a class="back-link" href="<?= e(url('/questions.php')) ?>">← Voltar para perguntas</a>
        <p class="muted">Cada questão precisa ter quatro alternativas e apenas uma resposta correta.</p>
    </div>
</section>

<?php if ($disciplines === [] || $floors === []): ?>
    <div class="alert alert-warning">
        <span>É necessário ter pelo menos uma disciplina e um andar ativos antes de criar perguntas.</span>
    </div>
<?php endif; ?>

<form class="editor-layout" action="<?= e(url('/actions/question-save.php')) ?>" method="post">
    <?= csrf_field() ?>
    <input type="hidden" name="id" value="<?= $questionId ?>">

    <section class="panel editor-main">
        <div class="field-group">
            <label for="prompt">Enunciado</label>
            <textarea id="prompt" name="prompt" rows="5" maxlength="500" placeholder="Digite a pergunta que será exibida no livro..." required><?= e(old('prompt', $question['prompt'])) ?></textarea>
            <small>Até 500 caracteres. Escreva de forma direta e sem ambiguidades.</small>
        </div>

        <fieldset class="answers-fieldset">
            <legend>Alternativas</legend>
            <p class="field-hint">Marque o círculo da resposta correta.</p>

            <?php
            $letters = ['A', 'B', 'C', 'D'];
            $fields = ['option_a', 'option_b', 'option_c', 'option_d'];
            foreach ($fields as $index => $field):
            ?>
                <div class="answer-input">
                    <input type="radio" name="correct_index" value="<?= $index ?>" <?= $selectedCorrect === $index ? 'checked' : '' ?> aria-label="Marcar alternativa <?= $letters[$index] ?> como correta" required>
                    <span class="answer-letter"><?= $letters[$index] ?></span>
                    <input type="text" name="<?= $field ?>" value="<?= e(old($field, $question[$field])) ?>" maxlength="255" placeholder="Alternativa <?= $letters[$index] ?>" aria-label="Texto da alternativa <?= $letters[$index] ?>" required>
                </div>
            <?php endforeach; ?>
        </fieldset>
    </section>

    <aside class="panel editor-settings">
        <div class="panel-header simple">
            <div>
                <span class="eyebrow">Configuração</span>
                <h2>Publicação</h2>
            </div>
        </div>

        <div class="field-group">
            <label for="floor_id">Andar do jogo</label>
            <select id="floor_id" name="floor_id" required>
                <option value="">Selecione...</option>
                <?php foreach ($floors as $floor): ?>
                    <?php $selectedFloor = (int) old('floor_id', $question['floor_id']); ?>
                    <option value="<?= (int) $floor['id'] ?>" <?= $selectedFloor === (int) $floor['id'] ? 'selected' : '' ?>>
                        <?= e($floor['name']) ?> — cena <?= e($floor['scene_name']) ?><?= (int) $floor['active'] === 0 ? ' (inativo)' : '' ?>
                    </option>
                <?php endforeach; ?>
            </select>
            <small>A Unity identifica este andar automaticamente pelo nome da cena.</small>
        </div>

        <div class="field-group">
            <label for="discipline_id">Disciplina</label>
            <select id="discipline_id" name="discipline_id" required>
                <option value="">Selecione...</option>
                <?php foreach ($disciplines as $discipline): ?>
                    <?php $selectedDiscipline = (int) old('discipline_id', $question['discipline_id']); ?>
                    <option value="<?= (int) $discipline['id'] ?>" <?= $selectedDiscipline === (int) $discipline['id'] ? 'selected' : '' ?>>
                        <?= e($discipline['name']) ?><?= (int) $discipline['active'] === 0 ? ' (inativa)' : '' ?>
                    </option>
                <?php endforeach; ?>
            </select>
        </div>

        <div class="field-group">
            <label for="difficulty">Dificuldade</label>
            <?php $selectedDifficulty = (string) old('difficulty', $question['difficulty']); ?>
            <select id="difficulty" name="difficulty" required>
                <option value="facil" <?= $selectedDifficulty === 'facil' ? 'selected' : '' ?>>Fácil</option>
                <option value="media" <?= $selectedDifficulty === 'media' ? 'selected' : '' ?>>Média</option>
                <option value="dificil" <?= $selectedDifficulty === 'dificil' ? 'selected' : '' ?>>Difícil</option>
            </select>
        </div>

        <div class="field-group">
            <span class="field-label">Status</span>
            <?php $selectedStatus = (string) old('status', $question['status']); ?>
            <label class="status-option">
                <input type="radio" name="status" value="draft" <?= $selectedStatus === 'draft' ? 'checked' : '' ?>>
                <span><strong>Rascunho</strong><small>Não aparece no jogo.</small></span>
            </label>
            <label class="status-option">
                <input type="radio" name="status" value="published" <?= $selectedStatus === 'published' ? 'checked' : '' ?>>
                <span><strong>Publicada</strong><small>Disponível para a Unity.</small></span>
            </label>
        </div>

        <div class="editor-actions">
            <button class="button button-primary button-wide" type="submit">
                <?= $questionId > 0 ? 'Salvar alterações' : 'Cadastrar pergunta' ?>
            </button>
            <a class="button button-ghost button-wide" href="<?= e(url('/questions.php')) ?>">Cancelar</a>
        </div>
    </aside>
</form>
