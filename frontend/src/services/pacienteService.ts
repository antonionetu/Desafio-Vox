import api from '@/api';
import { Token } from '@/services/utils';

export const pacienteService = {
    async getPacienteById() {
        const token = new Token();
        const pacienteId = token.asObject()?.id;

        try {
            const response = await api.get(`/pacientes/${pacienteId}`, {
                headers: { Authorization: `Bearer ${token.asString()}` }
            });
            return response.data;
        } 
        
        catch (error) {
            console.error('Erro ao buscar paciente:', error.response?.data || error);
            return null;
        }
    },

    async getConsultas(status = null) {
        const token = new Token();
        const pacienteId = token.asObject()?.id;

        try {
            const response = await api.get(`/pacientes/${pacienteId}/consultas`, { 
                params: { status },
                headers: { Authorization: `Bearer ${token.asString()}` }
            });
            return response.data;
        } 
        
        catch (error) {
            console.error('Erro ao buscar consultas:', error.response?.data || error);
            return [];
        }
    }
};
