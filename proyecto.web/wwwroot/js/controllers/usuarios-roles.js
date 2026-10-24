import { ApiUsuariosRoles } from '../services/usuarios-roles-api.js';

import { escapeHtml, toast as mostrarToast, openDialog as abrirDialog, closeDialog as cerrarDialog } from '../utils/ui.js';

(function () {


    // ---------------------------------------------------------
    // Estado
    // ---------------------------------------------------------
    const estado = {
        usuarios: [],
        roles: [],
        sucursales: [], // NUEVO
        areas: [],      // NUEVO
        usuarioSeleccionadoId: null,
        buscarDebounce: null,
    };

    // ---------------------------------------------------------
    // Utilidades generales
    // ---------------------------------------------------------

    function iniciales(nombre, apellido) {
        const n = (nombre || '').trim()[0] || '';
        const a = (apellido || '').trim()[0] || '';
        return (n + a).toUpperCase() || '--';
    }

    function formatFecha(iso) {
        if (!iso) return '—';
        const fecha = new Date(iso);
        if (Number.isNaN(fecha.getTime())) return '—';
        return fecha.toLocaleDateString('es-BO', { year: 'numeric', month: 'short', day: 'numeric' });
    }

    // ---------------------------------------------------------
    // Tabs
    // ---------------------------------------------------------
    document.querySelectorAll('.tab-btn').forEach((btn) => {
        btn.addEventListener('click', () => {
            document.querySelectorAll('.tab-btn').forEach((b) => {
                b.classList.remove('is-active');
                b.setAttribute('aria-selected', 'false');
            });
            btn.classList.add('is-active');
            btn.setAttribute('aria-selected', 'true');

            document.querySelectorAll('.tab-panel').forEach((p) => p.classList.remove('is-active'));
            document.getElementById(`panel-${btn.dataset.tab}`).classList.add('is-active');
        });
    });

    // ---------------------------------------------------------
    // Sucursales y Áreas
    // ---------------------------------------------------------
    async function cargarSucursalesYAreas() {
        try {
            // Traemos solo las activas para los formularios
            const [sucs, areas] = await Promise.all([
                ApiUsuariosRoles.listarSucursales({ activo: true }),
                ApiUsuariosRoles.listarAreas()
            ]);
            estado.sucursales = sucs || [];
            estado.areas = areas || [];
        } catch (err) {
            console.error("No se pudieron cargar las sucursales o áreas", err);
        }
    }

    function poblarSelectSucursales(select, idSeleccionado) {
        select.innerHTML = '<option value="">Sin sucursal</option>';
        estado.sucursales.forEach(s => {
            const opt = document.createElement('option');
            opt.value = s.idSucursal;
            opt.textContent = s.nombre;
            if (idSeleccionado && s.idSucursal == idSeleccionado) opt.selected = true;
            select.appendChild(opt);
        });
    }

    function poblarSelectAreas(select, idSeleccionado) {
        select.innerHTML = '<option value="">Sin área</option>';
        estado.areas.forEach(a => {
            const opt = document.createElement('option');
            opt.value = a.idArea;
            opt.textContent = a.nombre;
            if (idSeleccionado && a.idArea == idSeleccionado) opt.selected = true;
            select.appendChild(opt);
        });
    }

    // ===========================================================
    // USUARIOS
    // ===========================================================
    const tbodyUsuarios = document.getElementById('tabla-usuarios-body');
    const usuariosCargando = document.getElementById('usuarios-cargando');
    const usuariosVacio = document.getElementById('usuarios-vacio');
    const inputBuscar = document.getElementById('buscar-usuarios');
    const filtroEstadoUsuarios = document.getElementById('filtro-estado-usuarios');

    let usersRequestId = 0;
    async function cargarUsuarios() {
        const requestId = ++usersRequestId;
        usuariosCargando.hidden = false;
        usuariosVacio.hidden = true;
        tbodyUsuarios.innerHTML = '';

        try {
            const filtros = {
                busqueda: inputBuscar.value.trim() || undefined,
                activo: filtroEstadoUsuarios.value === '' ? undefined : filtroEstadoUsuarios.value === 'true',
            };
            const usuarios = await ApiUsuariosRoles.listarUsuarios(filtros);
            if (requestId !== usersRequestId) return;
            estado.usuarios = usuarios || [];
            renderUsuarios();
        } catch (err) {
            if (requestId !== usersRequestId) return;
            mostrarToast(err.message || 'No se pudieron cargar los usuarios.', 'error');
        } finally {
            if (requestId === usersRequestId) usuariosCargando.hidden = true;
        }
    }

    function renderUsuarios() {
        tbodyUsuarios.innerHTML = '';
        if (!estado.usuarios.length) {
            usuariosVacio.hidden = false;
            return;
        }
        usuariosVacio.hidden = true;

        estado.usuarios.forEach((u) => {
            const idUsuario = u.idUsuario;
            const userName = u.nombreUsuario;
            const rolesHtml = (u.roles && u.roles.length)
                ? u.roles.map((r) => `<span class="chip">${escapeHtml(r)}</span>`).join('')
                : `<span class="chip is-muted">Sin roles</span>`;

            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td>
                    <div class="t-usuario">${escapeHtml(u.nombre)} ${escapeHtml(u.apellido)}</div>
                    <div class="t-meta">@${escapeHtml(userName)} · ${escapeHtml(u.email)}</div>
                </td>
                <td class="t-meta">${escapeHtml(u.sucursal || '—')}${u.area ? ' · ' + escapeHtml(u.area) : ''}</td>
                <td><div class="chip-list">${rolesHtml}</div></td>
                <td><span class="badge ${u.mfaActivo ? 'badge-activo' : 'badge-inactivo'}">${u.mfaActivo ? 'Sí' : 'No'}</span></td>
                <td><span class="badge ${u.activo ? 'badge-activo' : 'badge-inactivo'}">${u.activo ? 'Activo' : 'Inactivo'}</span></td>
                <td class="t-acciones">
                    <div class="row-actions">
                        <button type="button" class="btn-icon" data-accion="ver" title="Ver detalle">
                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7-10-7-10-7z" /><circle cx="12" cy="12" r="3" /></svg>
                        </button>
                    </div>
                </td>
            `;
            tr.querySelector('[data-accion="ver"]').addEventListener('click', () => abrirDetalleUsuario(idUsuario));
            tbodyUsuarios.appendChild(tr);
        });
    }

    inputBuscar.addEventListener('input', () => {
        clearTimeout(estado.buscarDebounce);
        estado.buscarDebounce = setTimeout(cargarUsuarios, 350);
    });
    filtroEstadoUsuarios.addEventListener('change', cargarUsuarios);

    // ---------- Crear usuario ----------
    const dialogUsuarioNuevo = 'dialog-usuario-nuevo';
    const formUsuarioNuevo = document.getElementById('form-usuario-nuevo');
    const nuError = document.getElementById('nu-error');

    document.getElementById('btn-nuevo-usuario').addEventListener('click', () => {
        formUsuarioNuevo.reset();
        nuError.textContent = '';
        poblarSelectRoles(document.getElementById('nu-rol'), { incluirVacio: true });
        poblarSelectSucursales(document.getElementById('nu-sucursal'));
        poblarSelectAreas(document.getElementById('nu-area'));
        abrirDialog(dialogUsuarioNuevo);
    });

    formUsuarioNuevo.addEventListener('submit', async (e) => {
        e.preventDefault();
        nuError.textContent = '';
        const boton = document.getElementById('nu-submit');
        boton.disabled = true;

        const payload = {
            nombre: document.getElementById('nu-nombre').value.trim(),
            apellido: document.getElementById('nu-apellido').value.trim(),
            nombreUsuario: document.getElementById('nu-usuario').value.trim(),
            email: document.getElementById('nu-email').value.trim(),
            telefono: document.getElementById('nu-telefono').value.trim() || null,
            idRolInicial: document.getElementById('nu-rol').value ? Number(document.getElementById('nu-rol').value) : null,
            idSucursal: document.getElementById('nu-sucursal').value ? Number(document.getElementById('nu-sucursal').value) : null,
            idArea: document.getElementById('nu-area').value ? Number(document.getElementById('nu-area').value) : null,
        };

        try {
            const respuesta = await ApiUsuariosRoles.crearUsuario(payload);
            cerrarDialog(dialogUsuarioNuevo);
            mostrarToast('Usuario creado correctamente.', 'success');
            mostrarCredencial(respuesta.passwordTemporal);
            cargarUsuarios();
        } catch (err) {
            nuError.textContent = err.message || 'No se pudo crear el usuario.';
        } finally {
            boton.disabled = false;
        }
    });

    // ---------- Detalle / edición de usuario ----------
    const dialogDetalle = 'dialog-usuario-detalle';
    const formUsuarioEditar = document.getElementById('form-usuario-editar');
    const dtError = document.getElementById('dt-error');
    const dtRolError = document.getElementById('dt-rol-error');

    async function abrirDetalleUsuario(idUsuario) {
        try {
            const u = await ApiUsuariosRoles.obtenerUsuario(idUsuario);

            // Flexibilidad para atrapar datos en camelCase o PascalCase
            estado.usuarioSeleccionadoId = u.idUsuario || u.IdUsuario;

            document.getElementById('dt-id').value = u.idUsuario || u.IdUsuario;
            document.getElementById('dt-avatar').textContent = iniciales(u.nombre || u.Nombre, u.apellido || u.Apellido);
            document.getElementById('dt-nombre-completo').textContent = `${u.nombre || u.Nombre} ${u.apellido || u.Apellido}`;

            const userName = u.nombreUsuario || u.NombreUsuario;
            const fechaCreacion = u.fechaCreacion || u.FechaCreacion;
            document.getElementById('dt-usuario-meta').textContent = `@${userName} · miembro desde ${formatFecha(fechaCreacion)}`;

            document.getElementById('dt-nombre').value = u.nombre || u.Nombre || '';
            document.getElementById('dt-apellido').value = u.apellido || u.Apellido || '';
            document.getElementById('dt-email').value = u.email || u.Email || '';
            document.getElementById('dt-telefono').value = u.telefono || u.Telefono || '';

            poblarSelectSucursales(document.getElementById('dt-sucursal'), u.idSucursal ?? u.IdSucursal);
            poblarSelectAreas(document.getElementById('dt-area'), u.idArea ?? u.IdArea);

            const mfaActivo = u.mfaActivo ?? u.MfaActivo;
            const mfaBadge = document.getElementById('dt-mfa-badge');
            mfaBadge.textContent = mfaActivo ? 'Configurado' : 'No configurado';
            mfaBadge.className = `badge ${mfaActivo ? 'badge-activo' : 'badge-inactivo'}`;

            const activo = u.activo ?? u.Activo;
            const toggleBtn = document.getElementById('dt-toggle-estado');
            toggleBtn.textContent = activo ? 'Desactivar usuario' : 'Activar usuario';
            toggleBtn.dataset.activo = String(activo);

            // AQUÍ ESTÁ EL TRUCO PARA LOS ROLES:
            const rolesData = u.roles || u.Roles || [];
            renderRolesUsuario(rolesData);

            poblarSelectRoles(document.getElementById('dt-nuevo-rol'), {
                incluirVacio: true,
                excluirIds: rolesData.filter((r) => {
                    const rActivo = r.activo ?? r.Activo;
                    const rFechaFin = r.fechaFin ?? r.FechaFin;
                    return rActivo && (!rFechaFin || new Date(rFechaFin) > new Date());
                }).map((r) => r.idRol ?? r.IdRol),
            });

            dtError.textContent = '';
            dtRolError.textContent = '';
            abrirDialog(dialogDetalle);
        } catch (err) {
            mostrarToast(err.message || 'No se pudo cargar el detalle del usuario.', 'error');
        }
    }

    function renderRolesUsuario(roles) {
        const cont = document.getElementById('dt-roles-lista');
        if (!roles || !roles.length) {
            cont.innerHTML = '<p class="field-hint">Este usuario no tiene roles asignados.</p>';
            return;
        }
        cont.innerHTML = '';
        roles.forEach((r) => {
            // Mapeo seguro contra mayúsculas/minúsculas
            const activo = r.activo ?? r.Activo ?? false;
            const fechaFin = r.fechaFin ?? r.FechaFin;
            const fechaInicio = r.fechaInicio ?? r.FechaInicio;
            const nombreRol = r.nombreRol ?? r.NombreRol;
            const idRol = r.idRol ?? r.IdRol;

            const vigente = activo && (!fechaFin || new Date(fechaFin) > new Date());
            const row = document.createElement('div');
            row.className = 'role-row';
            row.innerHTML = `
                <div>
                    <div>${escapeHtml(nombreRol)} <span class="badge ${vigente ? 'badge-activo' : 'badge-inactivo'}">${vigente ? 'Vigente' : 'Finalizado'}</span></div>
                    <div class="role-row__meta">Desde ${formatFecha(fechaInicio)}${fechaFin ? ' · hasta ' + formatFecha(fechaFin) : ''}</div>
                </div>
            `;
            if (vigente) {
                const btnRevocar = document.createElement('button');
                btnRevocar.type = 'button';
                btnRevocar.className = 'btn-icon is-danger';
                btnRevocar.title = 'Revocar rol';
                btnRevocar.innerHTML = `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><line x1="18" y1="6" x2="6" y2="18" /><line x1="6" y1="6" x2="18" y2="18" /></svg>`;
                btnRevocar.addEventListener('click', () => revocarRol(idRol, nombreRol));
                row.appendChild(btnRevocar);
            }
            cont.appendChild(row);
        });
    }

    formUsuarioEditar.addEventListener('submit', async (e) => {
        e.preventDefault();
        dtError.textContent = '';
        const id = Number(document.getElementById('dt-id').value);
        const boton = document.getElementById('dt-guardar');
        boton.disabled = true;

        const payload = {
            nombre: document.getElementById('dt-nombre').value.trim(),
            apellido: document.getElementById('dt-apellido').value.trim(),
            email: document.getElementById('dt-email').value.trim(),
            telefono: document.getElementById('dt-telefono').value.trim() || null,
            idSucursal: document.getElementById('dt-sucursal').value ? Number(document.getElementById('dt-sucursal').value) : null,
            idArea: document.getElementById('dt-area').value ? Number(document.getElementById('dt-area').value) : null,
        };

        try {
            await ApiUsuariosRoles.actualizarUsuario(id, payload);
            mostrarToast('Cambios guardados.', 'success');
            cargarUsuarios();
        } catch (err) {
            dtError.textContent = err.message || 'No se pudieron guardar los cambios.';
        } finally {
            boton.disabled = false;
        }
    });

    document.getElementById('dt-toggle-estado').addEventListener('click', async (e) => {
        const id = Number(document.getElementById('dt-id').value);
        const activoActual = e.target.dataset.activo === 'true';
        const nuevoEstado = !activoActual;
        const accion = nuevoEstado ? 'activar' : 'desactivar';

        if (!confirm(`¿Seguro que deseas ${accion} este usuario?${!nuevoEstado ? ' Se cerrarán sus sesiones activas.' : ''}`)) return;

        try {
            await ApiUsuariosRoles.cambiarEstadoUsuario(id, nuevoEstado);
            mostrarToast(`Usuario ${nuevoEstado ? 'activado' : 'desactivado'} correctamente.`, 'success');
            cerrarDialog(dialogDetalle);
            cargarUsuarios();
        } catch (err) {
            mostrarToast(err.message || `No se pudo ${accion} el usuario.`, 'error');
        }
    });

    document.getElementById('dt-resetear-password').addEventListener('click', async () => {
        const id = Number(document.getElementById('dt-id').value);
        if (!confirm('¿Generar una nueva contraseña temporal para este usuario? La contraseña actual dejará de funcionar.')) return;

        try {
            const respuesta = await ApiUsuariosRoles.resetearPassword(id);
            mostrarCredencial(respuesta.passwordTemporal);
            mostrarToast('Contraseña reestablecida.', 'success');
        } catch (err) {
            mostrarToast(err.message || 'No se pudo resetear la contraseña.', 'error');
        }
    });

    document.getElementById('dt-asignar-rol').addEventListener('click', async () => {
        dtRolError.textContent = '';
        const id = Number(document.getElementById('dt-id').value);
        const idRol = Number(document.getElementById('dt-nuevo-rol').value);
        const fechaFin = document.getElementById('dt-nuevo-rol-fecha-fin').value;

        if (!idRol) {
            dtRolError.textContent = 'Selecciona un rol.';
            return;
        }

        try {
            await ApiUsuariosRoles.asignarRol(id, {
                idRol,
                fechaFin: fechaFin ? new Date(fechaFin).toISOString() : null,
            });
            mostrarToast('Rol asignado.', 'success');
            document.getElementById('dt-nuevo-rol-fecha-fin').value = '';
            abrirDetalleUsuario(id);
            cargarUsuarios();
        } catch (err) {
            dtRolError.textContent = err.message || 'No se pudo asignar el rol.';
        }
    });

    async function revocarRol(idRol, nombreRol) {
        const id = estado.usuarioSeleccionadoId;
        if (!confirm(`¿Revocar el rol "${nombreRol}" a este usuario?`)) return;

        try {
            await ApiUsuariosRoles.revocarRol(id, idRol);
            mostrarToast('Rol revocado.', 'success');
            abrirDetalleUsuario(id);
            cargarUsuarios();
        } catch (err) {
            mostrarToast(err.message || 'No se pudo revocar el rol.', 'error');
        }
    }

    // ---------- Credenciales generadas ----------
    function mostrarCredencial(password) {
        document.getElementById('cred-valor').textContent = password;
        abrirDialog('dialog-credenciales');
    }

    document.getElementById('cred-copiar').addEventListener('click', async () => {
        const valor = document.getElementById('cred-valor').textContent;
        try {
            await navigator.clipboard.writeText(valor);
            mostrarToast('Contraseña copiada al portapapeles.', 'success');
        } catch {
            mostrarToast('No se pudo copiar automáticamente. Selecciónala manualmente.', 'error');
        }
    });

    // ===========================================================
    // ROLES
    // ===========================================================
    const tbodyRoles = document.getElementById('tabla-roles-body');
    const rolesCargando = document.getElementById('roles-cargando');
    const rolesVacio = document.getElementById('roles-vacio');
    const filtroEstadoRoles = document.getElementById('filtro-estado-roles');
    const NOMBRE_ROL_ADMIN = 'Admin'; // debe coincidir con RolesSistemas.RolAdmin en el backend

    async function cargarRoles() {
        rolesCargando.hidden = false;
        rolesVacio.hidden = true;
        tbodyRoles.innerHTML = '';

        try {
            const filtros = { activo: filtroEstadoRoles.value === '' ? undefined : filtroEstadoRoles.value === 'true' };
            const roles = await ApiUsuariosRoles.listarRoles(filtros);
            estado.roles = roles || [];
            renderRoles();
        } catch (err) {
            mostrarToast(err.message || 'No se pudieron cargar los roles.', 'error');
        } finally {
            rolesCargando.hidden = true;
        }
    }

    function renderRoles() {
        tbodyRoles.innerHTML = '';
        if (!estado.roles.length) {
            rolesVacio.hidden = false;
            return;
        }
        rolesVacio.hidden = true;

        estado.roles.forEach((r) => {
            const esAdmin = r.nombre.toLowerCase() === NOMBRE_ROL_ADMIN.toLowerCase();
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td class="t-usuario">${escapeHtml(r.nombre)}</td>
                <td class="t-meta">${escapeHtml(r.descripcion || '—')}</td>
                <td><span class="badge ${r.activo ? 'badge-activo' : 'badge-inactivo'}">${r.activo ? 'Activo' : 'Inactivo'}</span></td>
                <td class="t-acciones">
                    <div class="row-actions">
                        <button type="button" class="btn-icon" data-accion="editar" title="Editar" ${esAdmin ? 'disabled' : ''}>
                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><path d="M12 20h9" /><path d="M16.5 3.5a2.1 2.1 0 0 1 3 3L7 19l-4 1 1-4 12.5-12.5z" /></svg>
                        </button>
                        <button type="button" class="btn-icon ${r.activo ? 'is-danger' : ''}" data-accion="estado" title="${r.activo ? 'Desactivar' : 'Activar'}" ${esAdmin && r.activo ? 'disabled' : ''}>
                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><circle cx="12" cy="12" r="9" /><path d="M12 7v6" /><path d="M9 9l6 6" /></svg>
                        </button>
                    </div>
                </td>
            `;
            const btnEditar = tr.querySelector('[data-accion="editar"]');
            const btnEstado = tr.querySelector('[data-accion="estado"]');
            if (!esAdmin) btnEditar.addEventListener('click', () => abrirEditarRol(r));
            if (!(esAdmin && r.activo)) btnEstado.addEventListener('click', () => cambiarEstadoRol(r));
            tbodyRoles.appendChild(tr);
        });
    }

    filtroEstadoRoles.addEventListener('change', cargarRoles);

    function poblarSelectRoles(select, { incluirVacio = false, excluirIds = [] } = {}) {
        const activos = estado.roles.filter((r) => r.activo && !excluirIds.includes(r.idRol));
        select.innerHTML = '';
        if (incluirVacio) {
            const opt = document.createElement('option');
            opt.value = '';
            opt.textContent = select.id === 'nu-rol' ? 'Sin rol inicial' : 'Seleccionar rol…';
            select.appendChild(opt);
        }
        activos.forEach((r) => {
            const opt = document.createElement('option');
            opt.value = r.idRol;
            opt.textContent = r.nombre;
            select.appendChild(opt);
        });
    }

    // ---------- Crear / editar rol ----------
    const dialogRol = 'dialog-rol';
    const formRol = document.getElementById('form-rol');
    const rolError = document.getElementById('rol-error');

    document.getElementById('btn-nuevo-rol').addEventListener('click', () => {
        formRol.reset();
        document.getElementById('rol-id').value = '';
        document.getElementById('rol-titulo').textContent = 'Nuevo rol';
        rolError.textContent = '';
        abrirDialog(dialogRol);
    });

    function abrirEditarRol(rol) {
        formRol.reset();
        document.getElementById('rol-id').value = rol.idRol;
        document.getElementById('rol-titulo').textContent = `Editar rol · ${rol.nombre}`;
        document.getElementById('rol-nombre').value = rol.nombre;
        document.getElementById('rol-descripcion').value = rol.descripcion || '';
        rolError.textContent = '';
        abrirDialog(dialogRol);
    }

    formRol.addEventListener('submit', async (e) => {
        e.preventDefault();
        rolError.textContent = '';
        const boton = document.getElementById('rol-submit');
        boton.disabled = true;

        const id = document.getElementById('rol-id').value;
        const payload = {
            nombre: document.getElementById('rol-nombre').value.trim(),
            descripcion: document.getElementById('rol-descripcion').value.trim() || null,
        };

        try {
            if (id) {
                await ApiUsuariosRoles.actualizarRol(Number(id), payload);
                mostrarToast('Rol actualizado.', 'success');
            } else {
                await ApiUsuariosRoles.crearRol(payload);
                mostrarToast('Rol creado.', 'success');
            }
            cerrarDialog(dialogRol);
            cargarRoles();
        } catch (err) {
            rolError.textContent = err.message || 'No se pudo guardar el rol.';
        } finally {
            boton.disabled = false;
        }
    });

    async function cambiarEstadoRol(rol) {
        const nuevoEstado = !rol.activo;
        const accion = nuevoEstado ? 'activar' : 'desactivar';
        if (!confirm(`¿Seguro que deseas ${accion} el rol "${rol.nombre}"?`)) return;

        try {
            await ApiUsuariosRoles.cambiarEstadoRol(rol.idRol, nuevoEstado);
            mostrarToast(`Rol ${nuevoEstado ? 'activado' : 'desactivado'}.`, 'success');
            cargarRoles();
            cargarUsuarios();
        } catch (err) {
            mostrarToast(err.message || `No se pudo ${accion} el rol.`, 'error');
        }
    }

    // ---------------------------------------------------------
    // Carga inicial
    // ---------------------------------------------------------
    (async function init() {
        await cargarRoles();
        await cargarSucursalesYAreas(); // <--- Carga sucursales y áreas al inicio
        await cargarUsuarios();
    })();
})();
