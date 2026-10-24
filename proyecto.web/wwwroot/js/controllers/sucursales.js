import { apiCallAuth } from '../services/api.js';
import { el, escapeHtml as escapar } from '../utils/ui.js';

(function () {

    const ENDPOINT_CIUDADES = '/ciudades';
    const ENDPOINT_USUARIOS = '/usuarios/catalogo'; 

    let sucursales = [];
    let filtroTexto = '';
    let filtroActivo = '';
    let catalogoUsuarios = null;

    /* ---------------------------------------------------------
       Carga inicial
       --------------------------------------------------------- */
    let branchesRequestId = 0;
    async function cargarSucursales() {
        const requestId = ++branchesRequestId;
        el('tabla-body').innerHTML = '<tr><td colspan="6" class="table-state">Cargando sucursales…</td></tr>';
        el('page-error').classList.remove('is-visible');
        try {
            const qs = filtroActivo !== '' ? `?activo=${filtroActivo}` : '';
            const result = await apiCallAuth(`/sucursales${qs}`);
            if (requestId !== branchesRequestId) return;
            sucursales = result;
            renderTabla();
        } catch (err) {
            if (requestId !== branchesRequestId) return;
            el('tabla-body').innerHTML = '';
            el('page-error').textContent = err.message;
            el('page-error').classList.add('is-visible');
        }
    }

    function renderTabla() {
        const texto = filtroTexto.trim().toLowerCase();
        const filas = sucursales.filter((s) =>
            !texto ||
            s.nombre.toLowerCase().includes(texto) ||
            (s.nombreCiudad || '').toLowerCase().includes(texto) ||
            (s.direccion || '').toLowerCase().includes(texto)
        );

        const cuerpo = el('tabla-body');
        if (!filas.length) {
            cuerpo.innerHTML = '<tr><td colspan="6" class="table-state">No se encontraron sucursales.</td></tr>';
            return;
        }

        cuerpo.innerHTML = filas.map((s) => `
      <tr data-id="${s.idSucursal}">
        <td class="t-primary">${escapar(s.nombre)}</td>
        <td>${escapar(s.nombreCiudad || '—')}</td>
        <td class="t-muted">${escapar(s.direccion || '—')}</td>
        <td class="t-muted">${escapar(s.nombreGerente || 'Sin asignar')}</td>
        <td>
          <label class="switch">
            <input type="checkbox" class="switch-estado" data-id="${s.idSucursal}" ${s.activo ? 'checked' : ''} />
            <span class="switch-track"></span>
          </label>
          <span class="status-label">${s.activo ? 'Activa' : 'Inactiva'}</span>
        </td>
        <td class="t-actions">
          <button type="button" class="icon-action btn-editar" data-id="${s.idSucursal}" aria-label="Editar">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><path d="M12 20h9"/><path d="M16.5 3.5a2.1 2.1 0 0 1 3 3L7 19l-4 1 1-4z"/></svg>
          </button>
        </td>
      </tr>
    `).join('');

        cuerpo.querySelectorAll('.btn-editar').forEach((btn) => {
            btn.addEventListener('click', () => abrirModal(Number(btn.dataset.id)));
        });
        cuerpo.querySelectorAll('.switch-estado').forEach((chk) => {
            chk.addEventListener('change', () => cambiarEstado(Number(chk.dataset.id), chk));
        });
    }

    /* ---------------------------------------------------------
       Filtros
       --------------------------------------------------------- */
    el('buscador').addEventListener('input', (e) => {
        filtroTexto = e.target.value;
        renderTabla();
    });

    el('filtro-estado').addEventListener('click', (e) => {
        const btn = e.target.closest('.chip');
        if (!btn) return;
        el('filtro-estado').querySelectorAll('.chip').forEach((c) => c.classList.remove('is-active'));
        btn.classList.add('is-active');
        filtroActivo = btn.dataset.valor;
        cargarSucursales();
    });

    /* ---------------------------------------------------------
       Activar / desactivar (PATCH estado) con reversión si falla
       --------------------------------------------------------- */
    async function cambiarEstado(id, checkbox) {
        const nuevoValor = checkbox.checked;
        checkbox.disabled = true;
        try {
            await apiCallAuth(`/sucursales/${id}/estado`, { method: 'PATCH', body: { activo: nuevoValor } });
            const s = sucursales.find((x) => x.idSucursal === id);
            if (s) s.activo = nuevoValor;
            checkbox.closest('td').querySelector('.status-label').textContent = nuevoValor ? 'Activa' : 'Inactiva';
        } catch (err) {
            checkbox.checked = !nuevoValor; // revertir visualmente
            el('page-error').textContent = err.message;
            el('page-error').classList.add('is-visible');
        } finally {
            checkbox.disabled = false;
        }
    }

    /* ---------------------------------------------------------
       Catálogos (con fallback a campo numérico si no existen)
       --------------------------------------------------------- */
    /* ---------------------------------------------------------
       Catálogo de ciudades — mismo patrón que usas en el resto
       del proyecto: se carga una vez y se muestra el nombre,
       nunca el id, en el <option>.
       --------------------------------------------------------- */
    async function cargarCiudades() {
        try {
            const ciudades = await apiCallAuth(ENDPOINT_CIUDADES);
            const select = el('f-ciudad');
            if (ciudades.length > 0) {
                select.innerHTML = ciudades.map((c) => `<option value="${c.idCiudad}">${escapar(c.nombre)}</option>`).join('');
            } else {
                select.innerHTML = '<option value="">No hay ciudades registradas</option>';
            }
        } catch (err) {
            console.warn('No se pudo cargar el catálogo de ciudades.', err);
            el('f-ciudad').innerHTML = '<option value="">No se pudo cargar el catálogo de ciudades</option>';
        }
    }

    async function prepararSelectGerente(current) {
        const select = el('f-gerente-select');
        const hint = el('f-gerente-hint');
        try {
            const users = catalogoUsuarios ?? await apiCallAuth(ENDPOINT_USUARIOS);
            catalogoUsuarios = users;
            select.replaceChildren(new Option('Sin asignar', ''));
            for (const user of users) select.add(new Option((user.nombre + ' ' + user.apellido).trim() || user.userName, user.idUsuario));
            if (current?.idGerenteResponsable && !users.some(user => user.idUsuario === current.idGerenteResponsable))
                select.add(new Option((current.nombreGerente || 'Responsable actual') + ' (asignación actual)', current.idGerenteResponsable));
            select.value = current?.idGerenteResponsable ?? '';
            select.disabled = false; hint.classList.add('hidden'); el('modal-guardar').disabled = false;
        } catch {
            select.replaceChildren(new Option('No se pudo cargar la lista', ''));
            select.disabled = true; hint.classList.remove('hidden'); el('modal-guardar').disabled = true;
        }
    }
    function valorGerente() { return el('f-gerente-select').value ? Number(el('f-gerente-select').value) : null; }
    function fijarGerente(value) { el('f-gerente-select').value = value ?? ''; }

    /* ---------------------------------------------------------
       Modal crear / editar
       --------------------------------------------------------- */
    const modal = el('modal-sucursal');
    const form = el('form-sucursal');

    async function abrirModal(id) {
        el('modal-error').classList.remove('is-visible');
        form.reset();
        el('f-id').value = id ?? '';
        el('modal-titulo').textContent = id ? 'Editar sucursal' : 'Nueva sucursal';

        await prepararSelectGerente(sucursales.find(branch => branch.idSucursal === id));

        if (id) {
            const s = sucursales.find((x) => x.idSucursal === id);
            if (s) {
                el('f-nombre').value = s.nombre;
                el('f-direccion').value = s.direccion || '';
                el('f-ciudad').value = s.idCiudad ?? '';
                fijarGerente(s.idGerenteResponsable);
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
        const idCiudad = el('f-ciudad').value ? Number(el('f-ciudad').value) : null;
        const errorEl = el('modal-error');
        errorEl.classList.remove('is-visible');

        if (!idCiudad) {
            errorEl.textContent = 'Selecciona una ciudad.';
            errorEl.classList.add('is-visible');
            return;
        }

        const cuerpo = {
            nombre: el('f-nombre').value.trim(),
            idCiudad,
            direccion: el('f-direccion').value.trim(),
            idGerenteResponsable: valorGerente(),
        };

        const btn = el('modal-guardar');
        const label = btn.querySelector('.btn-label');
        const original = label.textContent;
        btn.disabled = true;
        label.innerHTML = '<span class="spinner" aria-hidden="true"></span> Guardando…';

        try {
            if (id) {
                await apiCallAuth(`/sucursales/${id}`, { method: 'PUT', body: cuerpo });
            } else {
                await apiCallAuth('/sucursales', { method: 'POST', body: cuerpo });
            }
            cerrarModal();
            cargarSucursales();
        } catch (err) {
            errorEl.textContent = err.message;
            errorEl.classList.add('is-visible');
        } finally {
            btn.disabled = false;
            label.textContent = original;
        }
    });

    cargarSucursales();
    cargarCiudades();
})();
