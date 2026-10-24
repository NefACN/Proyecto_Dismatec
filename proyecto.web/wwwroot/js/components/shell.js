import { TokenStore } from '../core/session.js';
import { apiCallAuth, cerrarSesion } from '../services/api.js';
import { escapeHtml as esc, toast } from '../utils/ui.js';
import { icon } from './icons.js';
const adminLinks = [
    ['dashboard.html', 'Centro de control', 'grid'], ['campanias-phishing.html', 'Simulaciones de phishing', 'mail'],
    ['preguntas-admin.html', 'Banco de preguntas', 'book'], ['usuarios-roles.html', 'Usuarios y roles', 'users'],
    ['sucursales.html', 'Sucursales', 'building'], ['areas-operativas.html', 'Áreas operativas', 'layers'],
];
const httpLinks = [
    ['http-analisis.html', 'Panel de análisis', 'activity'], ['http-fuentes.html', 'Fuentes y registros', 'building'],
    ['http-ejecuciones.html', 'Ejecuciones', 'refresh'], ['http-patrones.html', 'Patrones', 'layers'],
    ['http-eventos.html', 'Eventos atípicos', 'alert'], ['http-comparacion.html', 'Comparación', 'grid'],
    ['http-modelo.html', 'Configuración del modelo', 'shield'],
];
export function mountShell(page) {
    const admin = TokenStore.esAdmin() && page !== 'dashboard-usuario';
    document.body.dataset.theme = admin ? 'admin' : 'employee';
    const sidebar = document.querySelector('[data-shell-sidebar]');
    const header = document.querySelector('[data-shell-header]');
    const identity = TokenStore.identity();
    const links = admin ? adminLinks : [['dashboard-usuario.html', 'Mi aprendizaje', 'grid'],
        ['dashboard-usuario.html#cursos', 'Retos de seguridad', 'book'], ['dashboard-usuario.html#logros', 'Mis logros', 'trophy']];
    const httpNavigation = admin ? `<details class="http-nav-module" ${page.startsWith('http-') ? 'open' : ''}><summary>${icon('activity')}<span>Análisis HTTP</span><small>PENDIENTE</small></summary><div>${httpLinks.map(([href, label, symbol]) => `<a href="${href}" ${href === `${page}.html` ? 'aria-current="page"' : ''}>${icon(symbol)}<span>${label}</span></a>`).join('')}</div></details>` : '';
    if (sidebar) sidebar.innerHTML = `
        <a class="app-brand" href="${TokenStore.home()}"><span class="brand-symbol">${icon('shield')}</span><span>DISMAC<small>${admin ? 'SECURITY OPERATIONS' : 'APRENDE · PROTEGE · CRECE'}</small></span></a>
        <div class="workspace-label">${admin ? 'ESPACIO ADMINISTRATIVO' : 'TU ESPACIO PERSONAL'}</div>
        <nav class="app-navigation" aria-label="Navegación principal">${links.map(([href, label, symbol]) =>
            `<a href="${href}" ${href === `${page}.html` ? 'aria-current="page"' : ''}>${icon(symbol)}<span>${label}</span></a>`).join('')}${httpNavigation}</nav>
        <div class="sidebar-note">${icon(admin ? 'activity' : 'star')}<strong>${admin ? 'Una cultura más segura' : 'Pequeños pasos. Gran impacto.'}</strong><p>${admin ? 'Personas, aprendizaje y prevención en un solo lugar.' : 'Cada reto te prepara para reconocer las amenazas del día a día.'}</p></div>
        <nav class="app-navigation sidebar-bottom" aria-label="Cuenta"><a href="seguridad-mfa.html" ${page === 'seguridad-mfa' ? 'aria-current="page"' : ''}>${icon('shield')}Seguridad de mi cuenta</a><button type="button" data-logout>${icon('logout')}Cerrar sesión</button></nav>
        <div class="sidebar-version">DISMAC <span>Plataforma de ciberseguridad</span></div>`;
    if (header) header.innerHTML = `
        <div class="topbar-context"><button class="menu-button" id="nav-toggle" aria-label="Abrir navegación" aria-expanded="false" aria-controls="app-sidebar">${icon('menu')}</button><span>${admin ? 'OPERACIONES /' : 'MI ESPACIO /'} <b>${esc(document.body.dataset.title ?? 'Seguridad de la cuenta')}</b></span></div>
        <div class="topbar-account"><span class="workspace-pill">${admin ? 'Administración' : 'Aprendizaje'}</span><a href="seguridad-mfa.html" class="account-link"><span class="avatar">${esc(identity.name.slice(0, 2).toUpperCase())}</span><span>${esc(identity.name)}<small>${admin ? 'Administrador' : 'Colaborador'}</small></span></a></div>`;
    const toggle = document.getElementById('nav-toggle');
    const closeMenu = () => { document.getElementById('dash')?.classList.remove('nav-open'); toggle?.setAttribute('aria-expanded', 'false'); };
    toggle?.addEventListener('click', () => {
        const open = document.getElementById('dash').classList.toggle('nav-open'); toggle.setAttribute('aria-expanded', String(open));
        if (open) sidebar?.querySelector('a')?.focus();
    });
    document.getElementById('dash-scrim')?.addEventListener('click', closeMenu);
    document.addEventListener('keydown', event => {
        if (!document.getElementById('dash')?.classList.contains('nav-open')) return;
        if (event.key === 'Escape') { closeMenu(); toggle?.focus(); }
        if (event.key === 'Tab') {
            const focusable = [...sidebar.querySelectorAll('a[href], button:not(:disabled)')];
            const first = focusable[0], last = focusable.at(-1);
            if (event.shiftKey && document.activeElement === first) { event.preventDefault(); last?.focus(); }
            else if (!event.shiftKey && document.activeElement === last) { event.preventDefault(); first?.focus(); }
        }
    });
    sidebar?.querySelectorAll('a').forEach(link => link.addEventListener('click', closeMenu));
    document.querySelectorAll('[data-logout]').forEach(button => button.addEventListener('click', async () => {
        button.disabled = true; await cerrarSesion(); window.location.replace('login.html');
    }));
    if (page !== 'seguridad-mfa' && !page.startsWith('http-')) apiCallAuth('/mfa/estado').then(state => {
        if (state.authenticatorConfigurado) return;
        const notice = document.createElement('a'); notice.className = 'account-notice'; notice.href = 'seguridad-mfa.html';
        notice.innerHTML = `${icon('shield')}<span>Refuerza tu cuenta: configura tu app autenticadora.</span>${icon('arrow')}`;
        document.querySelector('.dash-content')?.prepend(notice);
    }).catch(() => toast('No se pudo comprobar la protección de tu cuenta.', 'error'));
}
