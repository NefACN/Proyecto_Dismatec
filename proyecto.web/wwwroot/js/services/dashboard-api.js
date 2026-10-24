import { apiCallAuth } from './api.js';
export const LearningApi = {
    profile: () => apiCallAuth('/dashboard/mi-perfil'), courses: () => apiCallAuth('/dashboard/cursos-disponibles'),
    ranking: () => apiCallAuth('/dashboard/ranking-sucursales'), history: () => apiCallAuth('/dashboard/mi-historial'),
    start: id => apiCallAuth(`/dashboard/cursos/${Number(id)}/iniciar`, { method: 'POST' }),
    complete: (id, answers) => apiCallAuth(`/dashboard/cursos/${Number(id)}/completar`, { method: 'POST', body: { idsOpcionesSeleccionadas: answers } }),
};
export const AuditApi = {
    list(filters = {}, signal) {
        const params = new URLSearchParams(Object.entries(filters).filter(([, value]) => value !== '' && value != null));
        return apiCallAuth(`/auditoria?${params}`, { signal });
    },
};
export const AdminApi = {
    users: () => apiCallAuth('/usuarios'), branches: () => apiCallAuth('/sucursales'),
    campaigns: () => apiCallAuth('/campanias-phishing'), campaign: id => apiCallAuth(`/campanias-phishing/${Number(id)}`),
};
