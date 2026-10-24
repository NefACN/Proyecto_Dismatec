export const el = id => document.getElementById(id);
export const escapeHtml = value => String(value ?? '').replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' })[c]);
export const formatDate = (value, options = {}) => {
    if (!value) return '—';
    const date = new Date(value);
    return Number.isNaN(date.getTime()) ? '—' : date.toLocaleDateString('es-BO', { day: 'numeric', month: 'short', year: 'numeric', ...options });
};
export const errorMessage = (error, fallback = 'No se pudo completar la operación.') => error?.message ?? error?.mensaje ?? fallback;
export function toast(message, type = 'info') {
    let container = el('toast-container');
    if (!container) {
        container = document.createElement('div'); container.id = 'toast-container'; container.className = 'toast-container';
        container.setAttribute('aria-live', 'polite'); document.body.append(container);
    }
    const item = document.createElement('div'); item.className = `toast is-${type}`; item.textContent = message;
    container.append(item); setTimeout(() => item.remove(), 6000);
}
export function openDialog(target) { const dialog = typeof target === 'string' ? el(target) : target; if (dialog && !dialog.open) dialog.showModal(); }
export function closeDialog(target) { const dialog = typeof target === 'string' ? el(target) : target; if (dialog?.open) dialog.close(); }
export function installDialogControls() {
    document.addEventListener('click', event => {
        const close = event.target.closest('[data-close-dialog]'); if (close) closeDialog(close.dataset.closeDialog);
    });
}
export function conCarga(button, action) {
    return async (...args) => {
        if (button.disabled) return;
        const label = button.querySelector('.btn-label') ?? button;
        const original = [...label.childNodes];
        const spinner = document.createElement('span'); spinner.className = 'spinner'; spinner.setAttribute('aria-hidden', 'true');
        label.replaceChildren(spinner, document.createTextNode(' Procesando…'));
        button.disabled = true; button.setAttribute('aria-busy', 'true');
        try { return await action(...args); }
        finally { button.disabled = false; button.removeAttribute('aria-busy'); label.replaceChildren(...original); }
    };
}
export function installPasswordControls() {
    document.addEventListener('click', event => {
        const toggle = event.target.closest('.password-toggle');
        const input = toggle && el(toggle.dataset.toggleFor);
        if (!input) return;
        const show = input.type === 'password';
        input.type = show ? 'text' : 'password'; toggle.textContent = show ? 'Ocultar' : 'Ver';
        toggle.setAttribute('aria-pressed', String(show));
        toggle.setAttribute('aria-label', show ? 'Ocultar contraseña' : 'Mostrar contraseña');
    });
}
