import { TokenStore } from './core/session.js';
import { intentarRenovarSesion } from './services/api.js';
import { installDialogControls, installPasswordControls, toast } from './utils/ui.js';
import { mountShell } from './components/shell.js';
const page = document.body.dataset.page;
const adminPages = new Set(['dashboard', 'usuarios-roles', 'sucursales', 'areas-operativas', 'campanias-phishing', 'preguntas-admin']);
const httpPages = new Set(['http-analisis', 'http-fuentes', 'http-ejecuciones', 'http-patrones', 'http-eventos', 'http-comparacion', 'http-modelo']);
httpPages.forEach(page => adminPages.add(page));
const publicPages = new Set(['login', 'recuperar-password', 'phishing-educacion']);
document.addEventListener('session:expired', () => { if (!publicPages.has(page)) window.location.replace('login.html'); });
async function start() {
    installDialogControls();
    installPasswordControls();
    if (!publicPages.has(page)) {
        if (!TokenStore.isValid() && !await intentarRenovarSesion()) { window.location.replace('login.html'); return; }
        if (adminPages.has(page) && !TokenStore.esAdmin()) { window.location.replace('dashboard-usuario.html'); return; }
        mountShell(page);
    } else if (page === 'login' && TokenStore.isValid()) { window.location.replace(TokenStore.home()); return; }
    document.body.classList.add('app-ready');
    if (httpPages.has(page)) { await import('./features/http/controller.js'); return; }
    const controllers = {
        dashboard: () => import('./controllers/dashboard.js'), 'dashboard-usuario': () => import('./controllers/dashboard-usuario.js'),
        'usuarios-roles': () => import('./controllers/usuarios-roles.js'), sucursales: () => import('./controllers/sucursales.js'),
        'areas-operativas': () => import('./controllers/areas-operativas.js'), 'campanias-phishing': () => import('./controllers/campanias-phishing.js'),
        'preguntas-admin': () => import('./controllers/preguntas-admin.js'), 'seguridad-mfa': () => import('./controllers/seguridad-mfa.js'),
        login: () => import('./controllers/login.js'), 'recuperar-password': () => import('./controllers/recuperar-password.js'),
        'phishing-educacion': () => import('./controllers/phishing-educacion.js'),
    };
    await controllers[page]?.();
}
start().catch(error => {
    document.body.classList.add('app-ready');
    toast(error.message ?? 'No se pudo iniciar esta pantalla. Recarga para volver a intentar.', 'error');
    console.error('No se pudo iniciar la pantalla', error);
});
