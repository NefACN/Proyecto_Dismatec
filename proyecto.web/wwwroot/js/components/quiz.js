import { escapeHtml as esc } from '../utils/ui.js';

export function questionMarkup(questions) {
    return questions.map((question, index) => `<fieldset class="question-card"><legend>${index + 1}. ${esc(question.textoPregunta)}</legend>${question.opciones.map(option => `<label class="quiz-option"><input type="radio" name="question_${Number(question.idPregunta)}" value="${Number(option.idOpcion)}" required><span>${esc(option.textoOpcion)}</span></label>`).join('')}<div class="question-explanation hidden"><strong>Para recordar:</strong> ${esc(question.explicacion)}</div></fieldset>`).join('');
}

// Email templates are untrusted HTML. Keep them isolated, disable scripts,
// forms, navigation and all network resources (including tracking pixels).
export function renderEmailPreview(container, html) {
    const source = new DOMParser().parseFromString(String(html ?? '').replace(/\{\{\s*NombreUsuario\s*\}\}/gi, 'Colaborador'), 'text/html');
    source.querySelectorAll('script,iframe,object,embed,form,input,button,textarea,select,meta,base,link,svg,math').forEach(node => node.remove());
    source.querySelectorAll('*').forEach(node => {
        for (const attr of [...node.attributes]) {
            if (attr.name !== 'style' && attr.name !== 'colspan' && attr.name !== 'rowspan') node.removeAttribute(attr.name);
        }
    });
    const frame = document.createElement('iframe');
    frame.className = 'email-preview'; frame.title = 'Vista protegida del correo de simulación'; frame.setAttribute('sandbox', '');
    frame.srcdoc = `<!doctype html><meta charset="utf-8"><meta http-equiv="Content-Security-Policy" content="default-src 'none'; style-src 'unsafe-inline'; img-src 'none'; form-action 'none'; base-uri 'none'"><style>body{font:14px/1.7 system-ui,sans-serif;color:#26382f;background:#fff;padding:15px;margin:0;overflow-wrap:anywhere}img{display:none}a{color:inherit;text-decoration:underline;pointer-events:none}table{max-width:100%}</style>${source.body.innerHTML}`;
    container.replaceChildren(frame);
}
