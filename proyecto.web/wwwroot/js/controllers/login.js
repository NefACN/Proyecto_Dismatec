import { apiCall } from '../services/api.js';
import { TokenStore } from '../core/session.js';
import { escapeHtml as esc, conCarga } from '../utils/ui.js';

/* ============================================================
   login.js — orquesta las vistas de login.html contra
   AuthController: login, cambio de password obligatorio,
   MFA (envío/verificación/recuperación) y "olvidé mi password".
   ============================================================ */

(function () {
    // Nombres de proveedor tal como los expone ASP.NET Identity
    // (TokenOptions.DefaultEmailProvider / DefaultPhoneProvider / DefaultAuthenticatorProvider).
    const METODOS = {
        Email: { label: 'Código por correo', hint: 'Te enviamos un código a tu correo registrado.', icon: iconCorreo },
        Phone: { label: 'Código por SMS', hint: 'Te enviamos un código por mensaje de texto.', icon: iconTelefono },
        Authenticator: { label: 'App autenticadora', hint: 'Abre tu app de autenticación e ingresa el código actual.', icon: iconApp },
    };

    // Estado de la sesión de verificación en curso (vive solo en memoria).
    const estado = {
        cambioPasswordToken: null,
        mfaToken: null,
        metodoSeleccionado: null,
    };

    const vistas = {};
    document.querySelectorAll('.view').forEach((el) => { vistas[el.id] = el; });

    function mostrarVista(id) {
        Object.values(vistas).forEach((v) => v.classList.remove('is-active'));
        vistas[id].classList.add('is-active');
        ocultarError(id);
    }

    function idErrorDeVista(vistaId) {
        return {
            'view-login': 'login-error',
            'view-cambio-obligatorio': 'cambio-error',
            'view-mfa-metodo': 'mfa-metodo-error',
            'view-mfa-recovery': 'recovery-error',
            'view-forgot': 'forgot-error',
        }[vistaId];
    }

    function mostrarError(vistaId, mensaje) {
        const id = idErrorDeVista(vistaId);
        if (!id) return;
        const el = document.getElementById(id);
        el.textContent = mensaje;
        el.classList.add('is-visible');
    }

    function ocultarError(vistaId) {
        const id = idErrorDeVista(vistaId);
        if (!id) return;
        const el = document.getElementById(id);
        el.classList.remove('is-visible');
        el.textContent = '';
    }

    // Mostrar/ocultar contraseña

    /* ---------------------------------------------------------
       Punto único de manejo de la respuesta de login:
       el mismo shape (LoginResponse) puede volver desde
       /login y desde /cambiar-password-obligatorio.
       --------------------------------------------------------- */
    function manejarLoginResponse(data) {
        if (data.requiereCambioPassword) {
            estado.cambioPasswordToken = data.cambioPasswordToken;
            mostrarVista('view-cambio-obligatorio');
            return;
        }
        if (data.requiereMfa) {
            estado.mfaToken = data.mfaToken;
            renderMetodosMfa(data.metodosDisponibles || []);
            mostrarVista('view-mfa-metodo');
            return;
        }
        if (data.token) {
            finalizarSesion(data.token, (data.refreshToken ?? data.refreshtoken));
            return;
        }
        mostrarError('view-login', 'Respuesta inesperada del servidor.');
    }

    function finalizarSesion(token, refreshToken) {
        TokenStore.setTokens({ token, refreshToken });

        window.location.replace(TokenStore.home());
    }

    /* ---------------------------------------------------------
       Vista: login
       --------------------------------------------------------- */
    const formLogin = document.getElementById('form-login');
    formLogin.addEventListener('submit', (e) => {
        e.preventDefault();
        const usuario = document.getElementById('login-usuario').value.trim();
        const password = document.getElementById('login-password').value;

        let valido = true;
        marcarCampo('login-usuario', usuario.length > 0);
        marcarCampo('login-password', password.length > 0);
        if (!usuario || !password) valido = false;
        if (!valido) return;

        conCarga(document.getElementById('login-submit'), async () => {
            try {
                const data = await apiCall('/auth/login', {
                    body: { nombreUsuario: usuario, password },
                });
                manejarLoginResponse(data);
            } catch (err) {
                mostrarError('view-login', err.message);
            }
        })();
    });

    function marcarCampo(inputId, ok) {
        const field = document.getElementById(inputId).closest('.field');
        const errorEl = field.querySelector('.field-error');
        field.classList.toggle('has-error', !ok);
        if (errorEl) errorEl.classList.toggle('is-visible', !ok);
    }

    /* ---------------------------------------------------------
       Vista: cambio de password obligatorio
       --------------------------------------------------------- */
    document.getElementById('form-cambio-obligatorio').addEventListener('submit', (e) => {
        e.preventDefault();
        const nueva = document.getElementById('cambio-password-nueva').value;
        const confirmar = document.getElementById('cambio-password-confirmar').value;

        const coinciden = nueva.length >= 8 && nueva === confirmar;
        marcarCampo('cambio-password-confirmar', coinciden);
        if (!coinciden) return;

        conCarga(document.getElementById('cambio-submit'), async () => {
            try {
                const data = await apiCall('/auth/cambiar-password-obligatorio', {
                    body: { cambioPasswordToken: estado.cambioPasswordToken, passwordNueva: nueva },
                });
                manejarLoginResponse(data);
            } catch (err) {
                mostrarError('view-cambio-obligatorio', err.message);
            }
        })();
    });

    /* ---------------------------------------------------------
       Vista: elegir método MFA + ingresar código
       --------------------------------------------------------- */
    function renderMetodosMfa(metodos) {
        const lista = document.getElementById('mfa-method-list');
        lista.innerHTML = '';
        document.getElementById('mfa-code-block').classList.add('hidden');
        estado.metodoSeleccionado = null;

        metodos.forEach((metodo) => {
            const info = METODOS[metodo] || { label: metodo, hint: '', icon: iconApp };
            const btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'method-option';
            btn.dataset.metodo = metodo;
            btn.innerHTML = `
        <span class="method-option__icon">${info.icon()}</span>
        <span class="method-option__body">
          <strong>${esc(info.label)}</strong>
          <span>${esc(info.hint)}</span>
        </span>`;
            btn.addEventListener('click', () => seleccionarMetodoMfa(metodo, btn));
            lista.appendChild(btn);
        });
    }

    async function seleccionarMetodoMfa(metodo, btnEl) {
        document.querySelectorAll('.method-option').forEach((b) => b.classList.remove('is-selected'));
        btnEl.classList.add('is-selected');
        estado.metodoSeleccionado = metodo;
        ocultarError('view-mfa-metodo');

        const bloque = document.getElementById('mfa-code-block');
        const info = document.getElementById('mfa-code-info');

        if (metodo === 'Authenticator') {
            info.textContent = METODOS.Authenticator.hint;
            bloque.classList.remove('hidden');
            document.getElementById('mfa-codigo').focus();
            return;
        }

        btnEl.disabled = true;
        try {
            const data = await apiCall('/auth/enviar-codigo-mfa', {
                body: { mfaToken: estado.mfaToken, metodo },
            });
            info.textContent = (data && data.mensaje) || 'Código enviado.';
            bloque.classList.remove('hidden');
            document.getElementById('mfa-codigo').focus();
        } catch (err) {
            mostrarError('view-mfa-metodo', err.message);
        } finally {
            btnEl.disabled = false;
        }
    }

    document.getElementById('mfa-reenviar').addEventListener('click', () => {
        if (!estado.metodoSeleccionado || estado.metodoSeleccionado === 'Authenticator') return;
        const btn = document.querySelector(`.method-option[data-metodo="${estado.metodoSeleccionado}"]`);
        if (btn) seleccionarMetodoMfa(estado.metodoSeleccionado, btn);
    });

    document.getElementById('form-mfa-code').addEventListener('submit', (e) => {
        e.preventDefault();
        const codigo = document.getElementById('mfa-codigo').value.trim();
        if (!codigo) return;

        conCarga(document.getElementById('mfa-code-submit'), async () => {
            try {
                const data = await apiCall('/auth/verify-mfa', {
                    body: { mfaToken: estado.mfaToken, metodo: estado.metodoSeleccionado, codigo },
                });
                finalizarSesion(data.token, data.refreshToken);
            } catch (err) {
                mostrarError('view-mfa-metodo', err.message);
            }
        })();
    });

    document.getElementById('go-mfa-recovery').addEventListener('click', () => mostrarVista('view-mfa-recovery'));
    document.getElementById('recovery-volver').addEventListener('click', () => mostrarVista('view-mfa-metodo'));
    document.getElementById('mfa-volver-login').addEventListener('click', () => {
        estado.mfaToken = null;
        mostrarVista('view-login');
    });

    /* ---------------------------------------------------------
       Vista: código de recuperación MFA
       --------------------------------------------------------- */
    document.getElementById('form-mfa-recovery').addEventListener('submit', (e) => {
        e.preventDefault();
        const codigo = document.getElementById('recovery-codigo').value.trim();
        if (!codigo) return;

        conCarga(document.getElementById('recovery-submit'), async () => {
            try {
                const data = await apiCall('/auth/verify-mfa-recovery', {
                    body: { mfaToken: estado.mfaToken, codigoRecuperacion: codigo },
                });
                finalizarSesion(data.token, data.refreshToken);
            } catch (err) {
                mostrarError('view-mfa-recovery', err.message);
            }
        })();
    });

    /* ---------------------------------------------------------
       Vista: olvidé mi contraseña
       --------------------------------------------------------- */
    document.getElementById('go-forgot-password').addEventListener('click', () => mostrarVista('view-forgot'));
    document.getElementById('forgot-volver').addEventListener('click', () => mostrarVista('view-login'));
    document.getElementById('forgot-sent-volver').addEventListener('click', () => mostrarVista('view-login'));

    document.getElementById('form-forgot').addEventListener('submit', (e) => {
        e.preventDefault();
        const email = document.getElementById('forgot-email').value.trim();
        if (!email) return;

        conCarga(document.getElementById('forgot-submit'), async () => {
            try {
                await apiCall('/auth/olvide-password', { body: { email } });
                mostrarVista('view-forgot-sent');
            } catch (err) {
                // El backend no debería revelar si el correo existe; si esto falla
                // es un error real de red/servidor, no "correo no encontrado".
                mostrarError('view-forgot', err.message);
            }
        })();
    });

    function iconCorreo() {
        return '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><rect x="3" y="5" width="18" height="14" rx="2"/><path d="M3 7l9 6 9-6"/></svg>';
    }
    function iconTelefono() {
        return '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><rect x="7" y="2" width="10" height="20" rx="2"/><line x1="11" y1="18" x2="13" y2="18"/></svg>';
    }
    function iconApp() {
        return '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><rect x="4" y="4" width="16" height="16" rx="3"/><path d="M9 12l2 2 4-4"/></svg>';
    }
})();