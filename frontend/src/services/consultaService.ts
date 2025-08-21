import api from '@/api'
import { Token } from '@/services/utils'

export const consultaService = {
    async agendar(horarioId) {
        const token = new Token()

        try {
            const response = await api.post(
                '/consultas',
                { horarioId },
                { headers: { Authorization: `Bearer ${token.asString()}` } }
            )

            return response.data
        }

        catch (error) {
            console.error('Erro ao agendar consulta:', error.response?.data || error)
            throw error
        }
    },

    async alterarStatus(consultaId, status) {
        const token = new Token()
        try {
            const response = await api.patch(
                `/consultas/${consultaId}/status`,
                { status },
                { headers: { Authorization: `Bearer ${token.asString()}` } }
            )

            return response.data
        }
        catch (error) {
            console.error('Erro ao alterar status da consulta:', error.response?.data || error)
            throw error
        }
    }
}
