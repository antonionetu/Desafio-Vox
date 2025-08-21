import api from '@/api'
import { Token } from '@/services/utils'

export const horarioService = {
    async cria(horario) {
        const token = new Token()

        try {
            const response = await api.post(
                '/horarios',
                horario,
                { headers: { Authorization: `Bearer ${token.asString()}` } }
            )

            return response
        }
        catch (error) {
            return error.response
        }
    },

    async cancela(horarioId: number) {
        const token = new Token()

        try {
            const response = await api.delete(
                `/horarios/${horarioId}`,
                { headers: { Authorization: `Bearer ${token.asString()}` } }
            )

            return response.data
        }
        catch (error) {
            console.error('Erro ao cancelar horário:', error.response?.data || error)
            throw error
        }
    },

    async atualiza(horarioId: number, horario) {
        const token = new Token()

        try {
            const response = await api.put(
                `/horarios/${horarioId}`,
                horario,
                { headers: { Authorization: `Bearer ${token.asString()}` } }
            )

            return response
        }
        catch (error) {
            return error.response
        }
    }
}
