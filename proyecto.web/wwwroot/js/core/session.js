const KEYS = { access: 'auth.accessToken', refresh: 'auth.refreshToken' };
export function decodeToken(token) {
    try {
        const encoded = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
        return JSON.parse(new TextDecoder().decode(Uint8Array.from(atob(encoded.padEnd(Math.ceil(encoded.length / 4) * 4, '=')), c => c.charCodeAt(0))));
    } catch { return null; }
}
export const TokenStore = {
    getAccessToken: () => localStorage.getItem(KEYS.access),
    getRefreshToken: () => localStorage.getItem(KEYS.refresh),
    setTokens({ token, refreshToken } = {}) {
        if (token) localStorage.setItem(KEYS.access, token);
        if (refreshToken) localStorage.setItem(KEYS.refresh, refreshToken);
    },
    clear() { Object.values(KEYS).forEach(key => localStorage.removeItem(key)); },
    getDecodedToken() { return decodeToken(this.getAccessToken()); },
    esAdmin() {
        const claims = this.getDecodedToken();
        const roles = claims?.role ?? claims?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ?? [];
        return (Array.isArray(roles) ? roles : [roles]).includes('Admin');
    },
    isValid() { return Number(this.getDecodedToken()?.exp) * 1000 > Date.now() + 15000; },
    home() { return this.esAdmin() ? 'dashboard.html' : 'dashboard-usuario.html'; },
    identity() {
        const claims = this.getDecodedToken() ?? {};
        return {
            name: claims.unique_name ?? claims.name ?? claims['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] ?? 'Mi cuenta',
            email: claims.email ?? claims['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] ?? '',
        };
    },
};
