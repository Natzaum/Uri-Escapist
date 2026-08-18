document.querySelectorAll('[data-alert-close]').forEach((button) => {
    button.addEventListener('click', () => button.closest('[data-alert]')?.remove());
});

document.querySelectorAll('[data-password-toggle]').forEach((button) => {
    button.addEventListener('click', () => {
        const input = document.getElementById(button.dataset.passwordToggle);

        if (!input) return;

        const showing = input.type === 'text';
        input.type = showing ? 'password' : 'text';
        button.textContent = showing ? 'Mostrar' : 'Ocultar';
        button.setAttribute('aria-label', showing ? 'Mostrar senha' : 'Ocultar senha');
    });
});

document.querySelectorAll('form[data-confirm]').forEach((form) => {
    form.addEventListener('submit', (event) => {
        if (!window.confirm(form.dataset.confirm)) {
            event.preventDefault();
        }
    });
});

const menuToggle = document.querySelector('[data-menu-toggle]');
const sidebar = document.getElementById('sidebar');

menuToggle?.addEventListener('click', () => sidebar?.classList.toggle('is-open'));

document.addEventListener('click', (event) => {
    if (
        sidebar?.classList.contains('is-open') &&
        !sidebar.contains(event.target) &&
        !menuToggle?.contains(event.target)
    ) {
        sidebar.classList.remove('is-open');
    }
});
