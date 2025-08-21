import api from '@/api';
import { Token } from '@/services/utils';

export const medicoService = {
    async getAllMedicos({ limit = 10, nome = '', especialidade = null } = {}) {
        const token = new Token();

        try {
            const response = await api.get('/medicos', {
                headers: { Authorization: `Bearer ${token.asString()}` },
                params: { limit, nome, especialidade }
            });

            return response.data;
        } catch (error) {
            console.error('Erro ao buscar médicos:', error.response?.data || error);
            return [];
        }
    },

    async getMedicoById(id) {
        const token = new Token();

        if (!id) {
            id = token.asObject()?.id;
        }

        try {
            const response = await api.get(`/medicos/${id}`, {
                headers: { Authorization: `Bearer ${token.asString()}` }
            });

            return response.data;
        } catch (error) {
            console.error(`Erro ao buscar médico ${id}:`, error.response?.data || error);
            return null;
        }
    },

    async getHorariosByMedicoId(id) {
        const token = new Token()
        
        if (!id) {
            id = token.asObject()?.id;
        }

        try {
            const response = await api.get(`/medicos/${id}/horarios`, {
                params: { oucupado: false },
                headers: { Authorization: `Bearer ${token.asString()}` }
            })
            return response.data
        } catch (error) {
            console.error('Erro ao buscar horários:', error.response?.data || error)
            return []
        }
    },

    async getConsultas(status = null) {
        const token = new Token();
        const medicoId = token.asObject()?.id;

        try {
            const response = await api.get(`/medicos/${medicoId}/consultas`, {
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
