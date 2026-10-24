import { apiCallAuth } from '../services/api.js';
import { el, escapeHtml as escapar } from '../utils/ui.js';

(function () {

    const ENDPOINT_CRITICIDADES = '/campanias-phishing/criticidades';

    let areas = [];
    let filtroTexto = '';

    async function cargarAreas() {
        el('tabla-body').innerHTML = '<tr><td colspan="3" class="table-state">Cargando áreas…</td></tr>';
        el('page-error').classList.remove('is-visible');
        try {
            areas = await apiCallAuth('/areas-operativas');
            renderTabla();
        } catch (err) {
            el('tabla-body').innerHTML = '';
            el('page-error').textContent = err.message;
            el('page-error').classList.add('is-visible');
        }
    }

    function badgeNivel(nivel) {
        const n = (nivel || '').toLowerCase();
        if (n.includes('alta') || n.includes('crít')) return 'badge-critico';
        if (n.includes('media')) return 'badge-alto';
        if (n.includes('baja')) return 'badge-info';
        return 'badge-medio';
    }

    function renderTabla() {
        const texto = filtroTexto.trim().toLowerCase();
        const filas = areas.filter((a) => !texto || a.nombre.toLowerCase().includes(texto));

        const cuerpo = el('tabla-body');
        if (!filas.length) {
            cuerpo.innerHTML = '<tr><td colspan="3" class="table-state">No se encontraron áreas operativas.</td></tr>';
            return;
        }

        cuerpo.innerHTML = filas.map((a) => `
      <tr data-id="${a.idArea}">
        <td class="t-primary">${escapar(a.nombre)}</td>
        <td><span class="badge ${badgeNivel(a.nivelCriticidad)}">${escapar(a.nivelCriticidad || '—')}</span></td>
        <td class="t-actions">
          <button type="button" class="icon-action btn-editar" data-id="${a.idArea}" aria-label="Editar">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><path d="M12 20h9"/><path d="M16.5 3.5a2.1 2.1 0 0 1 3 3L7 19l-4 1 1-4z"/></svg>
          </button>
        </td>
      </tr>
    `).join('');

        cuerpo.querySelectorAll('.btn-editar').forEach((btn) => {
            btn.addEventListener('click', () => abrirModal(Number(btn.dataset.id)));
        });
    }

    el('buscador').addEventListener('input', (e) => {
        filtroTexto = e.target.value;
        renderTabla();
    });

    /* ---------------------------------------------------------
       Catálogo de niveles de criticidad — se carga una vez y
       siempre muestra el nombre (nivel), nunca el id.
       --------------------------------------------------------- */
    async function cargarCriticidades() {
        try {
            const criticidades = await apiCallAuth(ENDPOINT_CRITICIDADES);
            const select = el('f-criticidad');
            if (criticidades.length > 0) {
                select.innerHTML = criticidades.map((c) => `<option value="${c.idCriticidad}">${escapar(c.nivel)}</option>`).join('');
            } else {
                select.innerHTML = '<option value="">No hay niveles registrados</option>';
            }
        } catch (err) {
            console.warn('Error cargando criticidades:', err);
            el('f-criticidad').innerHTML = '<option value="">No se pudo cargar el catálogo</option>';
        }
    }

    /* ---------------------------------------------------------
       Modal crear / editar
       --------------------------------------------------------- */
    const modal = el('modal-area');
    const form = el('form-area');

    async function abrirModal(id) {
        el('modal-error').classList.remove('is-visible');
        form.reset();
        el('f-id').value = id ?? '';
        el('modal-titulo').textContent = id ? 'Editar área operativa' : 'Nueva área operativa';

        if (id) {
            const a = areas.find((x) => x.idArea === id);
            if (a) {
                el('f-nombre').value = a.nombre;
                el('f-criticidad').value = a.idCriticidad ?? '';
            }
        }

        modal.showModal();
    }

    function cerrarModal() { modal.close(); }

    el('btn-nueva').addEventListener('click', () => abrirModal(null));
    el('modal-cancelar').addEventListener('click', cerrarModal);
    modal.addEventListener('click', (e) => { if (e.target === modal) cerrarModal(); });

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const id = el('f-id').value ? Number(el('f-id').value) : null;
        const idCriticidad = el('f-criticidad').value ? Number(el('f-criticidad').value) : null;
        const errorEl = el('modal-error');
        errorEl.classList.remove('is-visible');

        if (!idCriticidad) {
            errorEl.textContent = 'Selecciona (o ingresa el ID de) el nivel de criticidad.';
            errorEl.classList.add('is-visible');
            return;
        }

        const cuerpo = { nombre: el('f-nombre').value.trim(), idCriticidad };

        const btn = el('modal-guardar');
        const label = btn.querySelector('.btn-label');
        const original = label.textContent;
        btn.disabled = true;
        label.innerHTML = '<span class="spinner" aria-hidden="true"></span> Guardando…';

        try {
            if (id) {
                await apiCallAuth(`/areas-operativas/${id}`, { method: 'PUT', body: cuerpo });
            } else {
                await apiCallAuth('/areas-operativas', { method: 'POST', body: cuerpo });
            }
            cerrarModal();
            cargarAreas();
        } catch (err) {
            errorEl.textContent = err.message;
            errorEl.classList.add('is-visible');
        } finally {
            btn.disabled = false;
            label.textContent = original;
        }
    });

    cargarAreas();
    cargarCriticidades();
})();