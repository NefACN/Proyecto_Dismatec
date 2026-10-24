import { TokenStore } from '../core/session.js';

export class ApiError extends Error {
    constructor(message, status = 0, payload = null) { super(message); this.name = 'ApiError'; this.status = status; this.payload = payload; }
}
export function extraerMensaje(payload, status) {
    if (typeof payload === 'string' && payload.trim()) return payload;
    if (payload?.mensaje) return payload.mensaje;
    if (Array.isArray(payload?.errores)) return payload.errores.join(' ');
    if (payload?.errors) return Object.values(payload.errors).flat().join(' ');
    return ({ 401: 'Tu sesión ha caducado. Vuelve a iniciar sesión.', 403: 'No tienes permiso para realizar esta acción.',
        423: 'Cuenta bloqueada temporalmente por demasiados intentos fallidos.' })[status]
        ?? 'No se pudo completar la solicitud. Intenta nuevamente.';
}
async function request(path, { method = 'GET', body, signal } = {}, token) {
    const base = globalThis.APP_CONFIG?.apiUrl?.replace(/\/$/, '');
    if (!base) throw new ApiError('No se pudo cargar la configuración de conexión. Recarga la página.');
    const controller = new AbortController();
    const timeout = setTimeout(() => controller.abort(), 30000);
    try {
        const response = await fetch(`${base}${path}`, {
            method, signal: signal ? AbortSignal.any([signal, controller.signal]) : controller.signal,
            headers: { Accept: 'application/json', ...(body !== undefined ? { 'Content-Type': 'application/json' } : {}),
                ...(token ? { Authorization: `Bearer ${token}` } : {}) },
            body: body === undefined ? undefined : JSON.stringify(body),
        });
        const text = await response.text();
        let payload = null;
        if (text) { try { payload = JSON.parse(text); } catch {
            if (response.headers.get('content-type')?.includes('text/plain')) payload = text;
        } }
        if (!response.ok) throw new ApiError(extraerMensaje(payload, response.status), response.status, payload);
        return payload;
    } catch (error) {
        if (error instanceof ApiError || signal?.aborted) throw error;
        throw new ApiError(error.name === 'AbortError' ? 'La solicitud tardó demasiado. Intenta nuevamente.'
            : 'No se pudo contactar al servidor. Comprueba tu conexión e intenta nuevamente.');
    } finally { clearTimeout(timeout); }
}
export const apiCall = (path, options = {}) => request(path, { method: 'POST', ...options });
let refreshInFlight;
let sessionVersion = 0;
export function intentarRenovarSesion() {
    if (refreshInFlight) return refreshInFlight;
    const refreshToken = TokenStore.getRefreshToken();
    if (!refreshToken) return Promise.resolve(false);
    const version = sessionVersion;
    refreshInFlight = (async () => {
        try {
            const result = await apiCall('/auth/refresh-token', { body: { refreshToken } });
            if (!result?.token || sessionVersion !== version) return false;
            TokenStore.setTokens(result); return true;
        } catch (error) {
            if ([400, 401, 403].includes(error.status) && sessionVersion === version) TokenStore.clear();
            return false;
        } finally { refreshInFlight = null; }
    })();
    return refreshInFlight;
}
export async function apiCallAuth(path, options = {}) {
    const passwordCheck = ['/mfa/autenticator/quitar', '/mfa/regenerar-codigos-recuperacion'].includes(path);
    const original = TokenStore.getAccessToken();
    if (!original && !await intentarRenovarSesion()) throw new ApiError('Inicia sesión para continuar.', 401);
    try { return await request(path, options, TokenStore.getAccessToken()); }
    catch (error) {
        if (error.status !== 401) throw error;
        // These endpoints also return 401 for a wrong password. Replaying them
        // would count a failed password twice and can lock the user's account.
        if (passwordCheck && TokenStore.isValid()) throw error;
        if ((TokenStore.getAccessToken() !== original && TokenStore.getAccessToken()) || await intentarRenovarSesion()) {
            try { return await request(path, options, TokenStore.getAccessToken()); }
            catch (retryError) {
                if (retryError.status === 401 && !passwordCheck) {
                    TokenStore.clear(); document.dispatchEvent(new CustomEvent('session:expired'));
                }
                throw retryError;
            }
        }
        if (!TokenStore.getAccessToken()) document.dispatchEvent(new CustomEvent('session:expired'));
        else throw new ApiError('No se pudo renovar la sesión. Intenta nuevamente.');
        throw error;
    }
}
export async function cerrarSesion() {
    sessionVersion++;
    const refreshToken = TokenStore.getRefreshToken(); TokenStore.clear();
    if (refreshToken) { try { await apiCall('/auth/logout', { body: { refreshToken } }); } catch { /* Local logout remains effective. */ } }
}
