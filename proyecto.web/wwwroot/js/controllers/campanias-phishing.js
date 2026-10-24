import { apiCallAuth } from '../services/api.js';
import { el, escapeHtml as esc, errorMessage as mensajeDeError } from '../utils/ui.js';



(function () {

    const dialogCampania = el('dialog-campania');
    const formCampania = el('form-campania');
    const dialogPlantilla = el('dialog-plantilla');
    const formPlantilla = el('form-plantilla');


    const ESTADO_EN_EJECUCION = 'En Ejecución';

    
    function localAIso(valorDatetimeLocal) {
        if (!valorDatetimeLocal) return null;
        const fecha = new Date(valorDatetimeLocal);
        return isNaN(fecha.getTime()) ? null : fecha.toISOString();
    }

    //Inicialización y Gestión de Pestañas (Tabs)
    async function inicializar() {
        await Promise.all([
            cargarCampanias(),
            cargarTablaPlantillas(),
            cargarCatalogos()
        ]);

        document.querySelectorAll('.tab-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                document.querySelectorAll('.tab-btn').forEach(b => b.classList.remove('is-active'));
                document.querySelectorAll('.tab-panel').forEach(p => p.classList.remove('is-active'));

                btn.classList.add('is-active');
                el(`panel-${btn.dataset.tab}`).classList.add('is-active');

                if (btn.dataset.tab === 'plantillas') cargarTablaPlantillas();
                if (btn.dataset.tab === 'campanias') cargarCampanias();
            });
        });

        // Delegación de eventos para las tablas (evita onclick inline y
        // los problemas de escapado que trae).
        el('tbody-campanias').addEventListener('click', (e) => {
            const btnDetalle = e.target.closest('.js-ver-detalle');
            if (btnDetalle) return verDetalle(parseInt(btnDetalle.dataset.id, 10));

            const btnEjecutar = e.target.closest('.js-ejecutar');
            if (btnEjecutar) return ejecutarMotor(parseInt(btnEjecutar.dataset.id, 10));
        });

        el('tbody-plantillas').addEventListener('click', (e) => {
            const btnEditar = e.target.closest('.js-editar-plantilla');
            if (btnEditar) return editarPlantilla(parseInt(btnEditar.dataset.id, 10));

            const btnEliminar = e.target.closest('.js-eliminar-plantilla');
            if (btnEliminar) return eliminarPlantilla(parseInt(btnEliminar.dataset.id, 10));
        });
    }

    /* =========================================================
       MÓDULO: CAMPAÑAS
       ========================================================= */
    async function cargarCampanias() {
        const tbody = el('tbody-campanias');
        try {
            const campanias = await apiCallAuth('/campanias-phishing');
            if (!campanias || campanias.length === 0) {
                tbody.innerHTML = `<tr><td colspan="7" class="empty-state">No hay campañas registradas.</td></tr>`;
                return;
            }

            tbody.innerHTML = campanias.map(c => {
                const enEjecucion = c.estado === ESTADO_EN_EJECUCION;

                return `
                <tr>
                    <td>#${c.idCampania}</td>
                    <td style="font-weight: 500;">${esc(c.titulo)}</td>
                    <td>${esc(c.plantilla)}</td>
                    <td><span class="chip">${c.totalAreasSegmentadas} áreas</span></td>
                    <td>
                        <span class="chip ${enEjecucion ? 'badge-activo' : 'is-muted'}">
                            ${esc(c.estado)}
                        </span>
                    </td>
                    <td>${c.fechaInicio ? new Date(c.fechaInicio).toLocaleDateString() : 'No definida'}</td>
                    <td class="t-acciones">
                        <div class="row-actions">
                            <button type="button" class="btn-icon js-ver-detalle" data-id="${c.idCampania}" title="Ver Reporte">
                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><path d="M12 20V10M18 20V4M6 20v-4"/></svg>
                            </button>
                            ${!enEjecucion ? `
                            <button type="button" class="btn-icon is-danger js-ejecutar" data-id="${c.idCampania}" title="Lanzar Simulación (Enviar correos)">
                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><polygon points="5 3 19 12 5 21 5 3"/></svg>
                            </button>` : ''}
                        </div>
                    </td>
                </tr>
            `;
            }).join('');
        } catch (err) {
            console.error('Error al cargar campañas:', err);
            tbody.innerHTML = `<tr><td colspan="7" class="empty-state" style="color:var(--error);">Error: ${esc(mensajeDeError(err, 'desconocido'))}</td></tr>`;
        }
    }

    async function cargarCatalogos() {
        try {
            const plantillas = await apiCallAuth('/campanias-phishing/plantillas');
            const select = el('c-plantilla');
            if (plantillas.length > 0) {
                select.innerHTML = `<option value="">Selecciona una plantilla…</option>` +
                    plantillas.map(p => `<option value="${p.idPlantilla}">${esc(p.nombre)}</option>`).join('');
            } else {
                select.innerHTML = `<option value="">Debes crear una plantilla primero</option>`;
            }
        } catch (err) {
            console.error('Error cargando plantillas:', err);
        }

        try {
            const areas = await apiCallAuth('/areas-operativas');
            const select = el('c-areas');
            if (areas.length > 0) {
                select.innerHTML = areas.map(a =>
                    `<option value="${a.idArea}">${esc(a.nombre)} (Riesgo: ${esc(a.nivelCriticidad || 'N/A')})</option>`
                ).join('');
            } else {
                select.innerHTML = `<option value="">No hay áreas creadas</option>`;
            }
        } catch (err) {
            console.warn('Endpoint de areas-operativas no encontrado aún.');
        }

        try {
            const criticidades = await apiCallAuth('/campanias-phishing/criticidades');
            const selectCrit = el('p-criticidad');
            if (criticidades.length > 0) {
                selectCrit.innerHTML = `<option value="">Selecciona un nivel…</option>` +
                    criticidades.map(c => `<option value="${c.idCriticidad}">${esc(c.nivel)}</option>`).join('');
            } else {
                selectCrit.innerHTML = `<option value="">No hay niveles registrados</option>`;
            }
        } catch (err) {
            console.warn('Error cargando criticidades:', err);
        }

        try {
            const categorias = await apiCallAuth('/campanias-phishing/categorias-educativas');
            const selectCat = el('p-categoria');
            if (categorias && categorias.length > 0) {
                selectCat.innerHTML = `<option value="">Selecciona una categoría…</option>` +
                    categorias.map(cat => `<option value="${cat.idCategoria}">${esc(cat.nombre)}</option>`).join('');
            } else {
                selectCat.innerHTML = `<option value="">No hay categorías registradas</option>`;
            }
        } catch (err) {
            console.warn('Error cargando categorías educativas:', err);
        }
    }

    el('btn-nueva-campania').addEventListener('click', async () => {
        await cargarCatalogos();
        formCampania.reset();
        el('c-error').style.display = 'none';
        dialogCampania.showModal();
    });

    formCampania.addEventListener('submit', async (e) => {
        e.preventDefault();
        const errorEl = el('c-error');
        errorEl.style.display = 'none';

        const idPlantilla = el('c-plantilla').value;
        if (!idPlantilla) {
            errorEl.textContent = 'Selecciona una plantilla.';
            errorEl.style.display = 'block';
            return;
        }

        const selectAreas = el('c-areas');
        const areasSeleccionadas = Array.from(selectAreas.selectedOptions)
            .map(opt => parseInt(opt.value, 10))
            .filter(v => !isNaN(v));

        if (areasSeleccionadas.length === 0) {
            errorEl.textContent = 'Selecciona al menos un área operativa.';
            errorEl.style.display = 'block';
            return;
        }

        const fechaInicioIso = localAIso(el('c-fecha-inicio').value);
        const fechaFinIso = localAIso(el('c-fecha-fin').value); // opcional

        if (el('c-fecha-fin').value && !fechaFinIso) {
            errorEl.textContent = 'La fecha de finalización no es válida.';
            errorEl.style.display = 'block';
            return;
        }

        const btnSubmit = el('c-submit');
        btnSubmit.disabled = true;

        try {
            await apiCallAuth('/campanias-phishing', {
                method: 'POST',
                body: {
                    titulo: el('c-titulo').value.trim(),
                    idPlantilla: parseInt(idPlantilla, 10),
                    fechaInicioProgramada: fechaInicioIso,
                    fechaFinProgramada: fechaFinIso,
                    areasSeleccionadas: areasSeleccionadas
                }
            });
            dialogCampania.close();
            await cargarCampanias();
        } catch (err) {
            console.error('Error al crear campaña:', err);
            errorEl.textContent = mensajeDeError(err, 'Error al crear la campaña.');
            errorEl.style.display = 'block';
        } finally {
            btnSubmit.disabled = false;
        }
    });

    async function ejecutarMotor(idCampania) {
        if (!confirm('🚨 ALERTA: ¿Estás seguro de lanzar esta simulación?\n\nSe enviarán correos reales a todos los empleados de las áreas seleccionadas usando el sistema SMTP de la empresa.')) return;

        try {
            const res = await apiCallAuth(`/campanias-phishing/${idCampania}/ejecutar`, { method: 'POST' });
            alert(`¡Éxito! ${res.mensaje}\nSe enviaron ${res.victimasSeleccionadas} correos trampa.`);
            await cargarCampanias();
        } catch (err) {
            console.error('Error al ejecutar campaña:', err);
            alert('Error al ejecutar: ' + mensajeDeError(err, 'intenta de nuevo.'));
        }
    }

    function verDetalle(idCampania) {
        alert(`Dashboard de métricas (Aperturas y Clics) en desarrollo para campaña #${idCampania}.`);
    }

    async function cargarTablaPlantillas() {
        const tbody = el('tbody-plantillas');
        try {
            const plantillas = await apiCallAuth('/campanias-phishing/plantillas');
            if (!plantillas || plantillas.length === 0) {
                tbody.innerHTML = `<tr><td colspan="5" class="empty-state">No hay plantillas creadas.</td></tr>`;
                return;
            }

            tbody.innerHTML = plantillas.map(p => `
                <tr>
                    <td>#${p.idPlantilla}</td>
                    <td style="font-weight: 500;">${esc(p.nombre)}</td>
                    <td>${esc(p.asuntoCorreo || p.asunto)}</td>
                    <td><span class="chip">${esc(p.nivelCriticidad || p.criticidad || 'Asignado')}</span></td>
                    <td class="t-acciones">
                        <div class="row-actions">
                            <button type="button" class="btn-icon js-editar-plantilla" data-id="${p.idPlantilla}" title="Editar">
                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                            </button>
                            <button type="button" class="btn-icon is-danger js-eliminar-plantilla" data-id="${p.idPlantilla}" title="Eliminar">
                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/></svg>
                            </button>
                        </div>
                    </td>
                </tr>
            `).join('');
        } catch (err) {
            console.error('Error al cargar plantillas:', err);
            tbody.innerHTML = `<tr><td colspan="5" class="empty-state" style="color:var(--error);">Error: ${esc(mensajeDeError(err, 'desconocido'))}</td></tr>`;
        }
    }

    el('btn-nueva-plantilla').addEventListener('click', () => {
        formPlantilla.reset();
        el('p-id').value = '';
        el('modal-plantilla-titulo').textContent = 'Crear Nueva Plantilla';
        el('p-error').style.display = 'none';
        // Aseguramos que no quede preseleccionado un valor de una edición previa
        el('p-criticidad').value = '';
        el('p-categoria').value = '';
        dialogPlantilla.showModal();
    });

    async function editarPlantilla(id) {
        try {
            const data = await apiCallAuth(`/campanias-phishing/plantillas/${id}`);

            el('p-id').value = data.idPlantilla;
            el('p-nombre').value = data.nombre || '';
            el('p-criticidad').value = data.idCriticidad;
            el('p-asunto').value = data.asuntoCorreo || '';
            el('p-cuerpo').value = data.cuerpoHtml || '';

            const errorEl = el('p-error');
            if (data.idCategoriaPregunta) {
                el('p-categoria').value = data.idCategoriaPregunta;
                errorEl.style.display = 'none';
            } else {
                // No adivinamos una categoría: si falta, se avisa en vez de
                // fijar "1" en silencio y guardar un dato incorrecto.
                el('p-categoria').value = '';
                errorEl.textContent = 'Esta plantilla no tiene categoría educativa asignada. Selecciona una antes de guardar.';
                errorEl.style.display = 'block';
            }

            el('modal-plantilla-titulo').textContent = 'Editar Plantilla';
            dialogPlantilla.showModal();
        } catch (err) {
            console.error('Error al cargar la plantilla:', err);
            alert('Error al cargar la plantilla: ' + mensajeDeError(err, 'intenta de nuevo.'));
        }
    }

    async function eliminarPlantilla(id) {
        if (!confirm('¿Estás seguro de eliminar esta plantilla?')) return;
        try {
            await apiCallAuth(`/campanias-phishing/plantillas/${id}`, { method: 'DELETE' });
            await cargarTablaPlantillas();
            await cargarCatalogos();
        } catch (err) {
            console.error('Error al eliminar plantilla:', err);
            alert('No se pudo eliminar: ' + mensajeDeError(err, 'intenta de nuevo.'));
        }
    }

    // Compatibilidad con onclick inline si quedara alguno en el HTML.

    formPlantilla.addEventListener('submit', async (e) => {
        e.preventDefault();
        const errorEl = el('p-error');
        errorEl.style.display = 'none';

        const idCriticidad = el('p-criticidad').value;
        const idCategoria = el('p-categoria').value;

        if (!idCriticidad || !idCategoria) {
            errorEl.textContent = 'Selecciona un nivel de criticidad y una categoría educativa.';
            errorEl.style.display = 'block';
            return;
        }

        const btnSubmit = el('p-submit');
        btnSubmit.disabled = true;

        const id = el('p-id').value;
        const payload = {
            nombre: el('p-nombre').value.trim(),
            asuntoCorreo: el('p-asunto').value.trim(),
            cuerpoHtml: el('p-cuerpo').value,
            idCriticidad: parseInt(idCriticidad, 10),
            idCategoriaPregunta: parseInt(idCategoria, 10)
        };

        try {
            if (id && id.trim() !== '') {
                await apiCallAuth(`/campanias-phishing/plantillas/${id}`, {
                    method: 'PUT',
                    body: payload
                });
            } else {
                await apiCallAuth('/campanias-phishing/plantillas', {
                    method: 'POST',
                    body: payload
                });
            }

            dialogPlantilla.close();
            await cargarTablaPlantillas();
            await cargarCatalogos();
        } catch (err) {
            console.error('Error al guardar plantilla:', err);
            errorEl.textContent = mensajeDeError(err, 'Error al procesar la solicitud.');
            errorEl.style.display = 'block';
        } finally {
            btnSubmit.disabled = false;
        }
    });

    inicializar();
})();