import qrcode from '../vendor/qrcode.js';
import { apiCallAuth } from '../services/api.js';
import { TokenStore } from '../core/session.js';
import { el, escapeHtml as esc, conCarga } from '../utils/ui.js';

/* ============================================================
   seguridad-mfa.js — MfaController (estado / activar / confirmar /
   quitar / regenerar códigos) + AuthController.CambiarPassword
   ============================================================ */

(function () {

    /* ---------------------------------------------------------
       Estado MFA
       --------------------------------------------------------- */
    async function cargarEstado() {
        try {
            const data = await apiCallAuth('/mfa/estado');
            renderEstado(data);
        } catch (err) {
            console.error("Error al cargar estado MFA:", err);
            const cargandoEl = document.getElementById('mfa-estado-cargando');
            if (cargandoEl) {
                cargandoEl.textContent = 'Error al cargar: ' + (err.message || 'Sesión inválida');
            }
            if (err.message && err.message.includes("401") || err.status === 401) {
                if (typeof TokenStore !== 'undefined') TokenStore.clear();
                window.location.href = 'login.html';
            }
        }
    }

    function renderEstado(data) {
        el('mfa-estado-cargando').classList.add('hidden');
        el('mfa-estado-bloque').classList.remove('hidden');

        const pillHabilitado = el('pill-mfa-habilitado');
        pillHabilitado.textContent = data.habilitado ? 'Activada' : 'Desactivada';
        pillHabilitado.className = 'pill ' + (data.habilitado ? 'pill-on' : 'pill-off');

        const pillAuth = el('pill-authenticator');
        pillAuth.textContent = data.authenticatorConfigurado ? 'Sí' : 'No';
        pillAuth.className = 'pill ' + (data.authenticatorConfigurado ? 'pill-on' : 'pill-off');

        el('codigos-restantes').textContent = data.codigosRecuperacionRestantes ?? '—';

        const acciones = el('mfa-acciones');
        acciones.innerHTML = '';

        if (!data.authenticatorConfigurado) {
            const aviso = document.createElement('p');
            aviso.className = 'field-hint';
            aviso.style.color = 'var(--error, #c0392b)';
            aviso.textContent = 'La app autenticadora es obligatoria en este sistema. Configúrala para completar tu verificación en dos pasos.';
            acciones.appendChild(aviso);

            const btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'btn btn-accent';
            btn.style.maxWidth = '260px';
            btn.textContent = 'Configurar app autenticadora';
            btn.addEventListener('click', iniciarActivacion);
            acciones.appendChild(btn);
        } else {
            const fila = document.createElement('div');
            fila.className = 'btn-row';
            fila.style.maxWidth = '480px';

            const btnRegenerar = document.createElement('button');
            btnRegenerar.type = 'button';
            btnRegenerar.className = 'btn btn-ghost';
            btnRegenerar.textContent = 'Regenerar códigos de recuperación';
            btnRegenerar.addEventListener('click', () => abrirModalPassword('regenerar'));

            const btnQuitar = document.createElement('button');
            btnQuitar.type = 'button';
            btnQuitar.className = 'btn btn-danger';
            btnQuitar.textContent = 'Restablecer app autenticadora';
            btnQuitar.addEventListener('click', () => abrirModalPassword('quitar'));

            fila.appendChild(btnRegenerar);
            fila.appendChild(btnQuitar);
            acciones.appendChild(fila);
        }
    }

    /* ---------------------------------------------------------
       Activar autenticador
       --------------------------------------------------------- */
    async function iniciarActivacion() {
        el('activar-error').classList.remove('is-visible');
        try {
            const data = await apiCallAuth('/mfa/autenticator/iniciar', { method: 'POST' });
            el('clave-manual').textContent = data.claveManual;
            el('card-activar').classList.remove('hidden');
            el('card-activar').scrollIntoView({ behavior: 'smooth', block: 'center' });

            const qr = qrcode(0, 'M');
            qr.addData(data.uriQr); qr.make();
            const canvas = el('qr-canvas'), context = canvas.getContext('2d');
            const margin = 4, count = qr.getModuleCount(), cell = 5;
            canvas.width = canvas.height = (count + margin * 2) * cell;
            context.fillStyle = '#fff'; context.fillRect(0, 0, canvas.width, canvas.height);
            context.fillStyle = '#000';
            for (let row = 0; row < count; row++) for (let col = 0; col < count; col++) {
                if (qr.isDark(row, col)) context.fillRect((col + margin) * cell, (row + margin) * cell, cell, cell);
            }
        } catch (err) {
            mostrarErrorGlobal(err.message);
        }
    }

    el('cancelar-activar').addEventListener('click', () => {
        el('card-activar').classList.add('hidden');
    });

    el('form-confirmar-mfa').addEventListener('submit', async (e) => {
        e.preventDefault();
        const codigo = el('confirmar-codigo').value.trim();
        if (!codigo) return;

        const errorEl = el('activar-error');
        errorEl.classList.remove('is-visible');
        const btn = el('confirmar-submit');

        conCarga(btn, async () => {
            try {
                const data = await apiCallAuth('/mfa/autenticator/confirmar', {
                    method: 'POST',
                    body: { codigo },
                });
                el('card-activar').classList.add('hidden');
                el('confirmar-codigo').value = '';
                mostrarCodigosRecuperacion(data.codigoRecuperacion || []);
                cargarEstado();
            } catch (err) {
                errorEl.textContent = err.message;
                errorEl.classList.add('is-visible');
            }
        })();
    });

    /* ---------------------------------------------------------
       Códigos de recuperación
       --------------------------------------------------------- */
    function mostrarCodigosRecuperacion(codigos) {
        const grid = el('codigos-grid');
        grid.innerHTML = codigos.map(c => `<span>${esc(c)}</span>`).join('');
        el('card-codigos').classList.remove('hidden');
        el('card-codigos').scrollIntoView({ behavior: 'smooth', block: 'center' });
    }

    el('codigos-listo').addEventListener('click', () => {
        el('card-codigos').classList.add('hidden');

        // Novedad: Si el usuario acababa de regenerar sus códigos, su token murió. 
        // Al darle a "Listo", lo enviamos a iniciar sesión amigablemente.
        if (typeof TokenStore !== 'undefined' && !TokenStore.getAccessToken()) {
            alert('Por seguridad, tu sesión se cerrará ahora. Por favor, inicia sesión nuevamente.');
            window.location.href = 'login.html';
        }
    });

    /* ---------------------------------------------------------
       Modal de confirmación con contraseña
       --------------------------------------------------------- */
    let accionModalPendiente = null;

    function abrirModalPassword(accion) {
        accionModalPendiente = accion;
        el('modal-titulo').textContent = accion === 'quitar'
            ? 'Restablecer app autenticadora'
            : 'Regenerar códigos de recuperación';
        el('modal-sub').textContent = accion === 'quitar'
            ? 'Ingresa tu contraseña actual para desvincular tu app.'
            : 'Los códigos actuales dejarán de funcionar. Ingresa tu contraseña para generar unos nuevos.';
        el('modal-error').classList.remove('is-visible');
        el('modal-password-input').value = '';
        el('modal-password').showModal();
        el('modal-password-input').focus();
    }

    function cerrarModalPassword() {
        el('modal-password').close();
        accionModalPendiente = null;
    }

    el('modal-cancelar').addEventListener('click', cerrarModalPassword);

    el('form-modal-password').addEventListener('submit', async (e) => {
        e.preventDefault();
        const passwordActual = el('modal-password-input').value;
        if (!passwordActual) return;

        const errorEl = el('modal-error');
        errorEl.classList.remove('is-visible');

        conCarga(el('modal-submit'), async () => {
            try {
                if (accionModalPendiente === 'quitar') {
                    await apiCallAuth('/mfa/autenticator/quitar', { method: 'POST', body: { passwordActual } });
                    cerrarModalPassword();

                    // FLUJO DE SEGURIDAD CORRECTO: Limpiamos token y redirigimos
                    alert('App autenticadora restablecida. Por seguridad, debes iniciar sesión nuevamente para configurarla.');
                    if (typeof TokenStore !== 'undefined') TokenStore.clear();
                    window.location.href = 'login.html';

                } else if (accionModalPendiente === 'regenerar') {
                    const data = await apiCallAuth('/mfa/regenerar-codigos-recuperacion', { method: 'POST', body: { passwordActual } });
                    cerrarModalPassword();

                    // FLUJO DE SEGURIDAD CORRECTO: Limpiamos token, pero mostramos los códigos en la interfaz primero.
                    if (typeof TokenStore !== 'undefined') TokenStore.clear();
                    mostrarCodigosRecuperacion(data.codigoRecuperacion || []);
                    // OJO: Ya no llamamos a cargarEstado() aquí porque sabemos que el token ya murió.
                }
            } catch (err) {
                errorEl.textContent = err.message;
                errorEl.classList.add('is-visible');
            }
        })();
    });

    /* ---------------------------------------------------------
       Cambiar contraseña (autenticado)
       --------------------------------------------------------- */
    el('form-cambiar-password').addEventListener('submit', async (e) => {
        e.preventDefault();
        const actual = el('cp-actual').value;
        const nueva = el('cp-nueva').value;
        const confirmar = el('cp-confirmar').value;

        const coinciden = nueva.length >= 8 && nueva === confirmar;
        el('cp-confirmar').closest('.field').classList.toggle('has-error', !coinciden);
        el('cp-confirmar-error').classList.toggle('is-visible', !coinciden);
        if (!coinciden) return;

        const errorEl = el('password-error');
        const exitoEl = el('password-exito');
        errorEl.classList.remove('is-visible');
        exitoEl.classList.remove('is-visible');

        conCarga(el('cp-submit'), async () => {
            try {
                await apiCallAuth('/auth/cambiar-password', {
                    method: 'POST',
                    body: { passwordActual: actual, passwordNueva: nueva },
                });
                exitoEl.classList.add('is-visible');
                el('form-cambiar-password').reset();
            } catch (err) {
                errorEl.textContent = err.message;
                errorEl.classList.add('is-visible');
            }
        })();
    });

    /* ---------------------------------------------------------
       Logout
       --------------------------------------------------------- */

    /* ---------------------------------------------------------
       Utilidades
       --------------------------------------------------------- */

    function mostrarErrorGlobal(mensaje) {
        el('activar-error').textContent = mensaje;
        el('activar-error').classList.add('is-visible');
    }

    // Inicializamos la página
    cargarEstado();
})();