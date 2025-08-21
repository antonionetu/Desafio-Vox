import api from '@/api';

export const autenticacaoService = {
    async login(login: string, senha: string) {
        try {
            const response = await api.post('/autenticacao/login', { login, senha });
            return response;
        } catch (error) {
            return error;
        }
    },
};
