import { apiCallAuth } from '../services/api.js';
import { el, escapeHtml as esc, errorMessage as mensajeDeError } from '../utils/ui.js';

/* ============================================================
   preguntas-admin.js
   Controlador para el CRUD del Banco de Preguntas
   Versión corregida
   ============================================================ */

(function () {

    const dialogPregunta = el('dialog-pregunta');
    const formPregunta = el('form-pregunta');
    const contenedorOpciones = el('contenedor-opciones');

    /* Escapa texto para innerHTML y para atributos (value="...") */

    async function inicializar() {
        // Los listeners de cierre se registran primero: no dependen de la red.

        await cargarCategorias();
        await cargarTablaPreguntas();
    }

    /* ---------------------------------------------------------
       CARGAS PRINCIPALES
       --------------------------------------------------------- */
    async function cargarCategorias() {
        const select = el('q-categoria');
        try {
            const categorias = await apiCallAuth('/campanias-phishing/categorias-educativas');

            // Opción vacía para que el "required" del select realmente funcione
            select.innerHTML = `<option value="">Selecciona una categoría…</option>` +
                categorias.map(c =>
                    `<option value="${c.idCategoria}">${esc(c.nombre)}</option>`
                ).join('');
        } catch (err) {
            console.error('Error cargando categorías:', err);
            select.innerHTML = `<option value="">Error al cargar categorías</option>`;
        }
    }

    async function cargarTablaPreguntas() {
        const tbody = el('tbody-preguntas');
        try {
            const listaPreguntas = await apiCallAuth('/preguntas-admin');

            if (!listaPreguntas || listaPreguntas.length === 0) {
                tbody.innerHTML = `<tr><td colspan="5" class="empty-state">No hay preguntas registradas.</td></tr>`;
                return;
            }

            tbody.innerHTML = listaPreguntas.map(p => {
                const numOpciones = (p.opciones || []).length;
                const numCorrectas = (p.opciones || []).filter(o => o.esCorrecta).length;

                // Aviso visual si una pregunta quedó sin respuesta correcta
                const avisoCorrectas = numCorrectas === 0
                    ? ` <span class="chip is-danger" title="Sin opción correcta">⚠</span>`
                    : '';

                return `
                <tr>
                    <td>#${p.idPregunta}</td>
                    <td><span class="chip">${esc(p.categoria)}</span></td>
                    <td style="font-weight:500;">${esc(p.textoPregunta)}</td>
                    <td>${numOpciones} opciones${avisoCorrectas}</td>
                    <td class="t-acciones">
                        <div class="row-actions">
                            <button type="button" class="btn-icon js-editar" data-id="${p.idPregunta}" title="Editar">✏️</button>
                            <button type="button" class="btn-icon is-danger js-eliminar" data-id="${p.idPregunta}" title="Eliminar">🗑️</button>
                        </div>
                    </td>
                </tr>
                `;
            }).join('');
        } catch (err) {
            console.error('Error al cargar preguntas:', err);
            tbody.innerHTML = `<tr><td colspan="5" class="empty-state" style="color:var(--error);">Error al cargar: ${esc(mensajeDeError(err, 'desconocido'))}</td></tr>`;
        }
    }

    /* Delegación: sobrevive a que la tabla se regenere */
    el('tbody-preguntas').addEventListener('click', (e) => {
        const btnEditar = e.target.closest('.js-editar');
        if (btnEditar) return editarPregunta(parseInt(btnEditar.dataset.id, 10));

        const btnEliminar = e.target.closest('.js-eliminar');
        if (btnEliminar) return eliminarPregunta(parseInt(btnEliminar.dataset.id, 10));
    });

    /* ---------------------------------------------------------
       MANEJO DINÁMICO DE OPCIONES
       --------------------------------------------------------- */
    function agregarFilaOpcion(idOpcion = '', texto = '', esCorrecta = false) {
        const row = document.createElement('div');
        row.className = `opcion-row ${esCorrecta ? 'correcta' : ''}`;

        row.innerHTML = `
            <input type="hidden" class="opcion-id" value="${esc(idOpcion)}">
            <input type="radio" name="opcion_correcta" class="radio-correcta" title="Marcar como respuesta correcta" ${esCorrecta ? 'checked' : ''} required>
            <input type="text" class="opcion-texto" placeholder="Escribe la opción aquí..." value="${esc(texto)}" required>
            <button type="button" class="btn-icon is-danger btn-quitar-opcion" title="Eliminar opción">×</button>
        `;

        const radio = row.querySelector('.radio-correcta');
        radio.addEventListener('change', () => {
            contenedorOpciones.querySelectorAll('.opcion-row')
                .forEach(r => r.classList.remove('correcta'));
            if (radio.checked) row.classList.add('correcta');
        });

        row.querySelector('.btn-quitar-opcion').addEventListener('click', () => {
            if (contenedorOpciones.querySelectorAll('.opcion-row').length <= 2) {
                alert('Una pregunta debe tener al menos 2 opciones.');
                return;
            }

            const eraCorrecta = radio.checked;
            row.remove();

            // Si se borró la opción marcada como correcta, marcamos la primera
            if (eraCorrecta) {
                const primera = contenedorOpciones.querySelector('.opcion-row');
                if (primera) {
                    primera.querySelector('.radio-correcta').checked = true;
                    contenedorOpciones.querySelectorAll('.opcion-row')
                        .forEach(r => r.classList.remove('correcta'));
                    primera.classList.add('correcta');
                }
            }
        });

        contenedorOpciones.appendChild(row);
    }

    el('btn-agregar-opcion').addEventListener('click', () => agregarFilaOpcion());

    /* ---------------------------------------------------------
       CRUD
       --------------------------------------------------------- */
    el('btn-nueva-pregunta').addEventListener('click', () => {
        formPregunta.reset();
        el('q-id').value = '';
        el('modal-titulo').textContent = 'Crear Nueva Pregunta';
        el('q-error').style.display = 'none';
        el('q-categoria').value = '';

        contenedorOpciones.innerHTML = '';
        agregarFilaOpcion('', '', true);
        agregarFilaOpcion('', '', false);

        dialogPregunta.showModal();
    });

    async function editarPregunta(id) {
        try {
            const preg = await apiCallAuth(`/preguntas-admin/${id}`);

            el('q-id').value = preg.idPregunta;
            el('q-categoria').value = preg.idCategoria;
            el('q-texto').value = preg.textoPregunta || '';
            el('q-explicacion').value = preg.explicacionEducativa || '';

            contenedorOpciones.innerHTML = '';
            const opciones = preg.opciones || [];

            // Un grupo de radios solo admite UNA marcada: si en BD hay varias
            // correctas, avisamos en lugar de perderlas en silencio.
            const correctas = opciones.filter(o => o.esCorrecta);
            opciones.forEach(o => agregarFilaOpcion(o.idOpcion, o.textoOpcion, o.esCorrecta));

            const errorEl = el('q-error');
            if (correctas.length > 1) {
                errorEl.textContent = `Atención: esta pregunta tiene ${correctas.length} opciones correctas en la base de datos, pero este formulario solo permite marcar una. Si guardas, se conservará únicamente la última marcada.`;
                errorEl.style.display = 'block';
            } else {
                errorEl.style.display = 'none';
            }

            el('modal-titulo').textContent = 'Editar Pregunta';
            dialogPregunta.showModal();
        } catch (err) {
            console.error('Error al cargar la pregunta:', err);
            alert('Error al cargar datos: ' + mensajeDeError(err, 'intenta de nuevo.'));
        }
    }

    async function eliminarPregunta(id) {
        if (!confirm('¿Seguro que deseas eliminar esta pregunta?')) return;
        try {
            await apiCallAuth(`/preguntas-admin/${id}`, { method: 'DELETE' });
            await cargarTablaPreguntas();
        } catch (err) {
            console.error('Error al eliminar:', err);
            alert('Atención: ' + mensajeDeError(err, 'no se pudo eliminar la pregunta.'));
        }
    }

    /* ---------------------------------------------------------
       GUARDAR
       --------------------------------------------------------- */
    formPregunta.addEventListener('submit', async (e) => {
        e.preventDefault();

        const id = el('q-id').value;
        const btnSubmit = el('q-submit');
        const errorEl = el('q-error');
        errorEl.style.display = 'none';

        const filas = Array.from(contenedorOpciones.querySelectorAll('.opcion-row'));

        const opcionesData = filas.map(fila => {
            const idOpc = fila.querySelector('.opcion-id').value;
            return {
                idOpcion: idOpc ? parseInt(idOpc, 10) : null,
                textoOpcion: fila.querySelector('.opcion-texto').value.trim(),
                esCorrecta: fila.querySelector('.radio-correcta').checked
            };
        });

        // Validaciones espejo de las del backend, para fallar antes de la red
        if (opcionesData.length < 2) {
            errorEl.textContent = 'Debe proporcionar al menos 2 opciones de respuesta.';
            errorEl.style.display = 'block';
            return;
        }
        if (!opcionesData.some(o => o.esCorrecta)) {
            errorEl.textContent = 'Debe marcar una opción como correcta.';
            errorEl.style.display = 'block';
            return;
        }
        const textos = opcionesData.map(o => o.textoOpcion.toLowerCase());
        if (new Set(textos).size !== textos.length) {
            errorEl.textContent = 'Hay opciones de respuesta repetidas.';
            errorEl.style.display = 'block';
            return;
        }

        const payload = {
            textoPregunta: el('q-texto').value.trim(),
            explicacionEducativa: el('q-explicacion').value.trim(),
            idCategoria: parseInt(el('q-categoria').value, 10),
            opciones: opcionesData
        };

        btnSubmit.disabled = true;
        btnSubmit.textContent = 'Guardando…';

        try {
            if (id) {
                await apiCallAuth(`/preguntas-admin/${id}`, { method: 'PUT', body: payload });
            } else {
                await apiCallAuth('/preguntas-admin', { method: 'POST', body: payload });
            }
            dialogPregunta.close();
            await cargarTablaPreguntas();
        } catch (err) {
            console.error('Error al guardar:', err);
            errorEl.textContent = mensajeDeError(err, 'Error al guardar.');
            errorEl.style.display = 'block';
        } finally {
            btnSubmit.disabled = false;
            btnSubmit.textContent = 'Guardar Pregunta';
        }
    });

    /* ---------------------------------------------------------
       LOGOUT (faltaba en esta página)
       --------------------------------------------------------- */

    inicializar();
})();