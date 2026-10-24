import { apiCallAuth } from './api.js';
﻿export const ApiUsuariosRoles = (() => {

    return {
        // ---------------- Usuarios (api/usuarios) ----------------
        listarUsuarios(filtros = {}) {
            const queryParams = new URLSearchParams();
            if (filtros.activo !== undefined) queryParams.append('activo', filtros.activo);
            if (filtros.busqueda) queryParams.append('busqueda', filtros.busqueda);

            const qs = queryParams.toString();
            const path = qs ? `/usuarios?${qs}` : '/usuarios';

            return apiCallAuth(path);
        },
        obtenerUsuario(id) {
            return apiCallAuth(`/usuarios/${id}`);
        },
        crearUsuario(payload) {
            return apiCallAuth('/usuarios', { method: 'POST', body: payload });
        },
        actualizarUsuario(id, payload) {
            return apiCallAuth(`/usuarios/${id}`, { method: 'PUT', body: payload });
        },
        cambiarEstadoUsuario(id, activo) {
            return apiCallAuth(`/usuarios/${id}/estado`, { method: 'PATCH', body: { activo } });
        },
        resetearPassword(id) {
            return apiCallAuth(`/usuarios/${id}/resetear-password`, { method: 'POST' });
        },
        asignarRol(idUsuario, payload) {
            return apiCallAuth(`/usuarios/${idUsuario}/roles`, { method: 'POST', body: payload });
        },
        revocarRol(idUsuario, idRol) {
            return apiCallAuth(`/usuarios/${idUsuario}/roles/${idRol}`, { method: 'DELETE' });
        },

        // ---------------- Roles (api/roles) ----------------
        listarRoles(filtros = {}) {
            const queryParams = new URLSearchParams();
            if (filtros.activo !== undefined) queryParams.append('activo', filtros.activo);

            const qs = queryParams.toString();
            const path = qs ? `/roles?${qs}` : '/roles';

            return apiCallAuth(path);
        },
        crearRol(payload) {
            return apiCallAuth('/roles', { method: 'POST', body: payload });
        },
        actualizarRol(id, payload) {
            return apiCallAuth(`/roles/${id}`, { method: 'PUT', body: payload });
        },
        cambiarEstadoRol(id, activo) {
            return apiCallAuth(`/roles/${id}/estado`, { method: 'PATCH', body: { activo } });
        },

        // ---------------- Sucursales y Áreas ----------------
        listarSucursales(filtros = {}) {
            const queryParams = new URLSearchParams();
            if (filtros.activo !== undefined) queryParams.append('activo', filtros.activo);
            const qs = queryParams.toString();
            const path = qs ? `/sucursales?${qs}` : '/sucursales';
            return apiCallAuth(path);
        },
        listarAreas() {
            return apiCallAuth('/areas-operativas');
        }
    };
})();