<section class="login-card">
    <div class="login-visual" aria-hidden="true">
        <div class="visual-content">
            <span class="brand-mark brand-mark-large">UE</span>
            <p class="eyebrow">URI Escapist</p>
            <h2>Ideias boas merecem uma boa pergunta.</h2>
            <p>Um espaço para preparar e organizar as questões que dão vida às partidas.</p>
        </div>
    </div>

    <div class="login-form-wrap">
        <div class="mobile-brand">
            <span class="brand-mark">UE</span>
            <strong>URI Escapist</strong>
        </div>
        <span class="eyebrow">Área restrita</span>
        <h1>Acesse o painel</h1>
        <p class="muted">Entre com a conta de professor cadastrada.</p>

        <form class="stack-form" action="<?= e(url('/login.php')) ?>" method="post">
            <?= csrf_field() ?>
            <label>
                <span>E-mail</span>
                <input type="email" name="email" value="<?= e(old('email')) ?>" autocomplete="username" required autofocus>
            </label>
            <label>
                <span>Senha</span>
                <span class="password-field">
                    <input id="password" type="password" name="password" autocomplete="current-password" required>
                    <button type="button" data-password-toggle="password" aria-label="Mostrar senha">Mostrar</button>
                </span>
            </label>
            <button class="button button-primary button-wide" type="submit">Entrar no painel</button>
        </form>

        <p class="login-footnote">A conta inicial é criada pelo administrador durante a instalação.</p>
    </div>
</section>
