import { defineStore } from 'pinia';

export const useAuthStore = defineStore('auth', {
    state: () => ({
        token: sessionStorage.getItem('token') || null,
        tipoUsuario: sessionStorage.getItem('tipoUsuario') || null
    }),
    actions: {
        setToken(newToken) {
            if (this.token) {
                sessionStorage.removeItem('token');
            }

            this.token = newToken;
            sessionStorage.setItem('token', newToken);
        },
        clearToken() {
            this.token = null;
            this.tipoUsuario = null;
            sessionStorage.removeItem('token');
            sessionStorage.removeItem('tipoUsuario');
        },
        setTipoUsuario(tipo) {
            this.tipoUsuario = tipo;
            sessionStorage.setItem('tipoUsuario', tipo);
        }
    }
});
