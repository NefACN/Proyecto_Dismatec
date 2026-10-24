import { questionMarkup } from '../components/quiz.js';
import { LearningApi } from '../services/dashboard-api.js';
import { el, escapeHtml as esc, formatDate, toast } from '../utils/ui.js';
import { icon } from '../components/icons.js';

document.querySelectorAll('[data-icon]').forEach(node => { node.innerHTML = icon(node.dataset.icon); });
let courses = [], attempt = null, starting = false, evaluating = false, loadVersion = 0;
const dialog = el('dialog-curso');
async function loadLearning() {
    const version = ++loadVersion;
    const results = await Promise.allSettled([LearningApi.profile(), LearningApi.courses(), LearningApi.ranking(), LearningApi.history()]);
    if (version !== loadVersion) return;
    const [profile, available, ranking, history] = results.map(result => result.status === 'fulfilled' ? result.value : null);
    if (profile) {
        el('welcome-name').textContent = profile.nombre || 'colaborador';
        el('learning-branch').textContent = profile.sucursal || 'Mi aprendizaje';
        el('learning-score').textContent = Number(profile.cyberScore).toLocaleString('es-BO');
        const badges = profile.insigniasObtenidas ?? [];
        el('learning-badges-count').textContent = badges.length;
        el('learning-badges').innerHTML = badges.length ? badges.map(badge => `<article class="achievement"><span class="medal">${icon('trophy')}</span><h3>${esc(badge.nombre)}</h3><small>${esc(formatDate(badge.fecha))}</small></article>`).join('') : '<p class="empty-state">Tu colección empieza aquí.<br>Completa retos y acumula CyberScore para desbloquear insignias.</p>';
    } else {
        el('learning-badges').innerHTML = '<p class="empty-state">No se pudieron cargar tus logros.</p>';
    }
    if (available) {
        courses = available;
        const practiced = courses.filter(course => course.ultimoExito).length;
        el('learning-practiced').textContent = practiced;
        el('learning-progress-caption').textContent = `De ${courses.length} temas disponibles`;
        el('courses-count').textContent = `${courses.length} temas`;
        el('learning-courses').innerHTML = courses.length ? courses.map((course, index) => `<article class="course-card"><span class="course-number">${String(index + 1).padStart(2, '0')}</span><div><h3>${esc(course.nombre)}</h3><span class="course-tag">${course.otorgaPuntos ? 'Puede otorgar puntos' : 'Modo repaso'}</span><p>${esc(course.mensajeCooldown)}</p></div><button class="btn ${course.otorgaPuntos ? 'btn-primary' : 'btn-ghost'}" data-course="${Number(course.idCategoria)}">${course.otorgaPuntos ? 'Empezar reto' : 'Repasar'} ${icon('arrow')}</button></article>`).join('') : '<p class="empty-state">Aún no hay cursos publicados. Vuelve pronto para empezar.</p>';
    } else el('learning-courses').innerHTML = '<p class="empty-state">No se pudieron cargar los retos. <button class="text-action" data-retry>Volver a intentar</button></p>';
    el('learning-ranking').innerHTML = ranking?.length ? ranking.map((row, index) => `<li><span class="rank-position">${index + 1}</span><span class="rank-name">${esc(row.sucursal)}<small>${Number(row.totalEmpleados)} empleados activos</small></span><span class="rank-score">${Math.round(row.promedioCyberScore)} <small>pts</small></span></li>`).join('') : `<li class="empty-state">${ranking ? 'Aún no hay sucursales en la clasificación.' : 'No se pudo cargar la clasificación.'}</li>`;
    el('learning-history').innerHTML = history?.length ? history.map(row => `<div class="history-item"><div>${esc(row.nombreCampania)}<small>${esc(row.temaAprendido)} · ${esc(formatDate(row.fechaCompletado))}</small></div><span>+${Number(row.puntosObtenidos)} pts</span></div>`).join('') : `<p class="empty-state">${history ? 'Las capacitaciones completadas de tus simulaciones aparecerán aquí.' : 'No se pudo consultar tu historial.'}</p>`;
    const failed = results.find(result => result.status === 'rejected');
    el('learning-error').classList.toggle('hidden', !failed);
    if (failed) {
        el('learning-error').replaceChildren(document.createTextNode(failed.reason.message + ' '));
        const retry = document.createElement('button'); retry.className = 'text-action'; retry.textContent = 'Volver a intentar';
        retry.addEventListener('click', loadLearning); el('learning-error').append(retry);
    }
}
async function startCourse(id, button) {
    if (starting || evaluating) return;
    starting = true; button.disabled = true;
    attempt = null; el('course-submit').disabled = true; el('course-submit').classList.remove('hidden');
    el('course-feedback').className = 'quiz-feedback hidden';
    el('course-questions').innerHTML = '<p class="empty-state">Preparando tus preguntas…</p>';
    el('course-title').textContent = courses.find(course => course.idCategoria === id)?.nombre ?? 'Reto de seguridad';
    el('course-instructions').textContent = 'Selecciona una respuesta por pregunta.';
    dialog.showModal();
    try {
        const result = await LearningApi.start(id);
        if (!dialog.open) return;
        if (!result.preguntas?.length) throw new Error('Este tema aún no tiene preguntas disponibles. Elige otro reto.');
        attempt = result;
        el('course-questions').innerHTML = questionMarkup(result.preguntas);
        el('course-submit').disabled = false;
    } catch (error) {
        el('course-questions').replaceChildren(); showFeedback(error.message, true);
    } finally { starting = false; button.disabled = false; }
}
function showFeedback(message, error = false) {
    el('course-feedback').textContent = message;
    el('course-feedback').className = `quiz-feedback${error ? ' is-error' : ''}`;
}
el('learning-courses').addEventListener('click', event => {
    const button = event.target.closest('[data-course]');
    if (button) startCourse(Number(button.dataset.course), button);
    if (event.target.closest('[data-retry]')) loadLearning();
});
dialog.addEventListener('cancel', event => { if (evaluating) event.preventDefault(); });
el('course-form').addEventListener('submit', async event => {
    event.preventDefault();
    if (!attempt || evaluating || !event.target.reportValidity()) return;
    const answers = [...el('course-questions').querySelectorAll('input:checked')].map(input => Number(input.value));
    if (answers.length !== attempt.preguntas.length) return;
    evaluating = true; el('course-submit').disabled = true; el('course-submit').textContent = 'Comprobando…';
    dialog.querySelectorAll('[data-close-dialog]').forEach(button => { button.disabled = true; });
    try {
        const result = await LearningApi.complete(attempt.idIntento, answers);
        showFeedback([result.mensaje, result.puntosGanados ? `+${result.puntosGanados} puntos de CyberScore.` : '',
            result.insigniasDesbloqueadas?.length ? `Nuevas insignias: ${result.insigniasDesbloqueadas.join(', ')}.` : ''].filter(Boolean).join(' '));
        el('course-submit').classList.add('hidden');
        el('course-questions').querySelectorAll('input').forEach(input => { input.disabled = true; });
        attempt = null; await loadLearning();
    } catch (error) {
        showFeedback(error.message, true);
        if (error.status === 400) {
            el('course-questions').querySelectorAll('.question-explanation').forEach(node => node.classList.remove('hidden'));
            el('course-questions').querySelectorAll('input').forEach(input => { input.disabled = true; });
            el('course-submit').classList.add('hidden'); attempt = null;
            showFeedback(`${error.message} Revisa las explicaciones y vuelve a abrir el reto para iniciar otro intento.`, true);
        } else {
            // The outcome is unknown after a connection failure. Reopen to reconcile with the server, don't blindly resubmit.
            el('course-submit').classList.add('hidden'); attempt = null;
            showFeedback(`${error.message} Cierra y vuelve a abrir el reto para comprobar su estado.`, true);
        }
    } finally {
        evaluating = false; el('course-submit').textContent = 'Comprobar respuestas';
        dialog.querySelectorAll('[data-close-dialog]').forEach(button => { button.disabled = false; });
    }
});
await loadLearning();
