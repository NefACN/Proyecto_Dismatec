import { AdminApi, AuditApi } from '../services/dashboard-api.js';
import { el, escapeHtml as esc } from '../utils/ui.js';
import { icon } from '../components/icons.js';

const labels = { Login: 'Inicio de sesión', LoginFallido: 'Inicio fallido', CambioPassword: 'Cambio de contraseña',
    CrearUsuario: 'Usuario creado', ActualizarUsuario: 'Usuario actualizado', AsignarRol: 'Rol asignado', RevocarRol: 'Rol revocado' };
const state = { page: 1, pageSize: 10, auditVersion: 0, loading: false };
document.querySelectorAll('[data-icon]').forEach(node => { node.innerHTML = icon(node.dataset.icon); });

async function loadOverview() {
    if (state.loading) return;
    state.loading = true; el('refresh-dashboard').disabled = true;
    el('dashboard-error').classList.add('hidden');
    const since = new Date(); since.setHours(0, 0, 0, 0); since.setDate(since.getDate() - 6);
    const results = await Promise.allSettled([AdminApi.users(), AdminApi.branches(), AdminApi.campaigns(),
        AuditApi.list({ desde: since.toISOString(), tamanioPagina: 200, pagina: 1 })]);
    const [users, branches, campaigns, events] = results.map(result => result.status === 'fulfilled' ? result.value : null);
    const failures = results.filter(result => result.status === 'rejected');
    el('stat-users').textContent = users ? users.filter(user => user.activo).length : '—';
    el('stat-users-caption').textContent = users ? `De ${users.length} usuarios registrados` : 'No disponible';
    el('stat-branches').textContent = branches ? branches.filter(branch => branch.activo).length : '—';
    el('stat-campaigns').textContent = campaigns?.length ?? '—';
    el('stat-clicks').textContent = '—';
    if (campaigns) {
        const details = await Promise.allSettled(campaigns.map(campaign => AdminApi.campaign(campaign.idCampania)));
        const complete = details.every(result => result.status === 'fulfilled');
        if (complete) {
            const clicks = details.reduce((sum, result) => sum + result.value.totalClicsVulnerados, 0);
            const sends = details.reduce((sum, result) => sum + result.value.totalEnviosGenerados, 0);
            el('stat-clicks').textContent = clicks;
            el('stat-clicks-caption').textContent = `${sends} envíos generados en total`;
        } else el('stat-clicks-caption').textContent = 'No se pudieron consultar todos los resultados';
    } else el('stat-clicks-caption').textContent = 'No disponible';
    renderChart(events, since);
    renderPriorities(users, events);
    if (failures.length) {
        el('dashboard-error').textContent = `Algunos datos no se pudieron cargar. ${failures[0].reason.message}`;
        el('dashboard-error').classList.remove('hidden');
    }
    el('snapshot-time').textContent = `Consulta ${failures.length ? 'parcial' : 'actualizada'} a las ${new Date().toLocaleTimeString('es-BO', { hour: '2-digit', minute: '2-digit' })}`;
    state.loading = false; el('refresh-dashboard').disabled = false;
}
function renderChart(events, since) {
    const chart = el('activity-chart');
    if (!events) { chart.innerHTML = '<p class="empty-state">La actividad no está disponible.</p>'; chart.setAttribute('aria-label', 'Actividad no disponible'); return; }
    const days = Array.from({ length: 7 }, (_, index) => {
        const date = new Date(since); date.setDate(date.getDate() + index);
        const rows = events.filter(event => new Date(event.fechaHora).toDateString() === date.toDateString());
        return { date, ok: rows.filter(row => row.exitoso).length, failed: rows.filter(row => !row.exitoso).length };
    });
    const maximum = Math.max(1, ...days.map(day => day.ok + day.failed));
    const descriptions = days.map(day => `${day.date.toLocaleDateString('es-BO', { weekday: 'short' })}: ${day.ok} exitosos, ${day.failed} no completados`);
    chart.setAttribute('aria-label', `Eventos en la muestra. ${descriptions.join('. ')}`);
    chart.innerHTML = days.map((day, index) => `<div class="chart-column" title="${esc(descriptions[index])}"><span class="chart-value">${day.ok + day.failed}</span><div class="chart-stack"><div class="chart-failure" style="height:${day.failed / maximum * 125}px"></div><div class="chart-success" style="height:${day.ok / maximum * 125}px"></div></div><span class="chart-label">${esc(day.date.toLocaleDateString('es-BO', { weekday: 'short' }))}</span></div>`).join('');
}
function renderPriorities(users, events) {
    const rows = [];
    if (users) {
        const unprotected = users.filter(user => user.activo && !user.mfaActivo).length;
        rows.push([unprotected ? 'alert' : 'check', unprotected ? `${unprotected} cuentas activas sin MFA configurado` : 'Cuentas activas con MFA configurado',
            unprotected ? 'Revisa la configuración de protección de estas cuentas.' : 'Mantén los métodos de recuperación al día.']);
        const pending = users.filter(user => user.activo && user.debeCambiarPassword).length;
        if (pending) rows.push(['shield', `${pending} cambios de contraseña pendientes`, 'Usuarios que deben actualizar su contraseña al ingresar.']);
    }
    if (events) {
        const failed = events.filter(event => !event.exitoso).length;
        rows.push(['activity', `${failed} eventos no completados`, 'En la muestra reciente de auditoría. Revisa el contexto de cada evento.']);
    }
    el('security-priorities').innerHTML = rows.length ? rows.map(([symbol, title, text]) => `<div class="priority"><span class="priority-symbol">${icon(symbol)}</span><div><strong>${esc(title)}</strong><p>${esc(text)}</p></div></div>`).join('') : '<p class="empty-state">No se pudieron consultar las prioridades.</p>';
}
async function loadAudit() {
    const version = ++state.auditVersion;
    el('audit-prev').disabled = true; el('audit-next').disabled = true;
    el('audit-body').innerHTML = '<tr><td colspan="5" class="table-state">Cargando eventos…</td></tr>';
    const from = el('audit-from').value, to = el('audit-to').value;
    if (from && to && from > to) { el('audit-body').innerHTML = '<tr><td colspan="5" class="table-state">La fecha inicial debe ser anterior a la final.</td></tr>'; return; }
    try {
        const rows = await AuditApi.list({ accion: el('audit-action').value, desde: from ? new Date(`${from}T00:00:00`).toISOString() : '',
            hasta: to ? new Date(`${to}T23:59:59.999`).toISOString() : '', pagina: state.page, tamanioPagina: state.pageSize });
        if (version !== state.auditVersion) return;
        el('audit-body').innerHTML = rows.length ? rows.map(row => `<tr><td>${esc(new Date(row.fechaHora).toLocaleString('es-BO', { day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit' }))}</td><td>${esc(row.nombreUsuario || 'Sistema')}</td><td>${esc(labels[row.accion] ?? row.accion)}</td><td class="mono">${esc(row.ipOrigen || '—')}</td><td><span class="result-badge ${row.exitoso ? '' : 'is-failed'}">${row.exitoso ? 'Completado' : 'No completado'}</span></td></tr>`).join('') : '<tr><td colspan="5" class="table-state">No hay eventos para estos filtros.</td></tr>';
        el('audit-count').textContent = `${rows.length} eventos en esta página`;
        el('audit-prev').disabled = state.page === 1; el('audit-next').disabled = rows.length < state.pageSize;
    } catch (error) {
        if (version !== state.auditVersion) return;
        el('audit-body').innerHTML = `<tr><td colspan="5" class="table-state">${esc(error.message)}</td></tr>`;
        el('audit-count').textContent = 'No disponible'; el('audit-prev').disabled = state.page === 1;
    }
    el('audit-page').textContent = `Página ${state.page}`;
}
el('refresh-dashboard').addEventListener('click', () => Promise.all([loadOverview(), loadAudit()]));
el('audit-filters').addEventListener('submit', event => { event.preventDefault(); state.page = 1; loadAudit(); });
el('audit-clear').addEventListener('click', () => { el('audit-filters').reset(); state.page = 1; loadAudit(); });
el('audit-prev').addEventListener('click', () => { state.page = Math.max(1, state.page - 1); loadAudit(); });
el('audit-next').addEventListener('click', () => { state.page++; loadAudit(); });
await Promise.all([loadOverview(), loadAudit()]);
