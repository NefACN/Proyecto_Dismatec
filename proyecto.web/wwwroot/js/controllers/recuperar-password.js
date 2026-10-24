import { apiCall } from '../services/api.js';

(function () {
    const params = new URLSearchParams(window.location.search);
    const email = params.get('email');
    const token = params.get('token');

    const vistas = {};
    document.querySelectorAll('.view').forEach((el) => { vistas[el.id] = el; });
    function mostrarVista(id) {
        Object.values(vistas).forEach((v) => v.classList.remove('is-active'));
        vistas[id].classList.add('is-active');
    }

    if (!email || !token) {
        mostrarVista('view-reset-invalido');
        return;
    }

    const errorEl = document.getElementById('reset-error');
    function mostrarError(mensaje) {
        errorEl.textContent = mensaje;
        errorEl.classList.add('is-visible');
    }

    document.getElementById('form-reset').addEventListener('submit', async (e) => {
        e.preventDefault();
        const nueva = document.getElementById('reset-password-nueva').value;
        const confirmar = document.getElementById('reset-password-confirmar').value;
        const confirmarField = document.getElementById('reset-password-confirmar').closest('.field');
        const confirmarError = document.getElementById('reset-confirmar-error');

        const coinciden = nueva.length >= 8 && nueva === confirmar;
        confirmarField.classList.toggle('has-error', !coinciden);
        confirmarError.classList.toggle('is-visible', !coinciden);
        if (!coinciden) return;

        const btn = document.getElementById('reset-submit');
        const label = btn.querySelector('.btn-label');
        const textoOriginal = label.textContent;
        btn.disabled = true;
        label.innerHTML = '<span class="spinner" aria-hidden="true"></span> Restableciendo…';

        try {
            await apiCall('/auth/reset-password', {
                body: { email, token, passwordNueva: nueva },
            });
            mostrarVista('view-reset-exito');
        } catch (err) {
            mostrarError(err.message);
            btn.disabled = false;
            label.textContent = textoOriginal;
        }
    });
})();