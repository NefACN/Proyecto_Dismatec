import { apiCall } from '../services/api.js';
import { el, escapeHtml as esc } from '../utils/ui.js';
import { questionMarkup, renderEmailPreview } from '../components/quiz.js';

const token = new URLSearchParams(location.search).get('token');
const loading = el('pantalla-carga'), error = el('pantalla-error'), success = el('pantalla-exito'), content = el('modulo-concientizacion');
const questions = el('contenedor-preguntas');
let count = 0;
function showError(message) {
    loading.classList.remove('visible'); content.style.display = 'none';
    el('msg-error-texto').textContent = message; error.classList.add('visible');
}
if (!token || !/^[0-9a-f]{8}-(?:[0-9a-f]{4}-){3}[0-9a-f]{12}$/i.test(token)) {
    showError('El enlace no contiene un identificador válido. Utiliza el enlace original de tu capacitación.');
} else {
    try {
        const data = await apiCall(`/concientizacion/rastrear/${encodeURIComponent(token)}`, { method: 'GET' });
        loading.classList.remove('visible'); content.style.display = 'block';
        renderEmailPreview(el('visualizador-correo'), data.cuerpoOriginal);
        count = data.preguntas?.length ?? 0;
        questions.innerHTML = count ? questionMarkup(data.preguntas) : '<p class="empty-state">Esta capacitación aún no tiene preguntas configuradas.</p>';
        el('btn-evaluar').disabled = !count;
    } catch (failure) { showError(failure.message); }
}
el('form-cuestionario').addEventListener('submit', async event => {
    event.preventDefault();
    const button = el('btn-evaluar'), feedback = el('mensaje-feedback');
    const answers = [...questions.querySelectorAll('input:checked')].map(input => Number(input.value));
    if (!count || answers.length !== count || button.disabled) return;
    button.disabled = true; button.textContent = 'Comprobando…'; feedback.textContent = '';
    try {
        const data = await apiCall(`/concientizacion/completar/${encodeURIComponent(token)}`, { body: { idsOpcionesSeleccionadas: answers } });
        content.style.display = 'none'; success.classList.add('visible');
        el('msg-exito-texto').innerHTML = `${esc(data.mensaje)}<br><strong>+${Number(data.puntosGanados ?? 0)} puntos de CyberScore</strong>${data.insigniasDesbloqueadas?.length ? `<p>Nuevas insignias: ${esc(data.insigniasDesbloqueadas.join(', '))}</p>` : ''}`;
    } catch (failure) {
        feedback.textContent = failure.message; feedback.className = 'feedback error';
        if (failure.status === 400) questions.querySelectorAll('.question-explanation').forEach(node => node.classList.remove('hidden'));
        button.disabled = false; button.textContent = 'Comprobar respuestas';
    }
});
