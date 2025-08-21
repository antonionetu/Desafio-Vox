<template>
    <div class="p-6 bg-white text-black rounded-3xl shadow-lg border border-gray-200 max-w-md mx-auto">
        <div class="flex gap-4 mb-6">
            <img v-if="calendarEvent._options.tipo === 'consulta'" class="size-10 rounded-box mt-2"
                src="https://img.daisyui.com/images/profile/demo/1@94.webp" />
            <div>
                <h3 class="text-2xl font-semibold">{{ calendarEvent.title }}</h3>
                <p v-if="calendarEvent._options.tipo === 'consulta'" class="text-gray-500 mb-2">
                    {{ calendarEvent._options.especialidade }}
                </p>

                <p class="mb-4 text-gray-700">
                    {{ formatEventDate(calendarEvent.start, calendarEvent.end) }}
                </p>
            </div>
        </div>

        <div v-if="calendarEvent._options.tipo === 'consulta'" class="flex justify-between gap-2">
            <button class="btn btn-error" @click="openConfirmModal">Cancelar</button>
            <button class="btn btn-success" @click="toast.info('Fora do escopo do desafio :)')">Ir para
                Consulta</button>
        </div>
        <div v-else class="flex justify-end gap-2">
            <button class="btn btn-error" @click="cancelaHorario(calendarEvent.id)">Remover</button>
        </div>
    </div>

    <dialog ref="confirmDialog" class="modal" @click.self="closeConfirmModal">
        <div class="modal-box bg-white rounded-lg shadow-lg border border-gray-200 relative">
            <form method="dialog">
                <button type="button" class="btn btn-sm btn-circle btn-ghost absolute right-2 top-2"
                    @click="closeConfirmModal">✕</button>
            </form>

            <h3 class="text-lg font-bold mb-4">Confirmação</h3>
            <p class="py-4">Tem certeza que deseja cancelar esta consulta?</p>
            <div class="modal-action flex justify-end gap-2">
                <button class="btn btn-error" @click="confirmCancel(calendarEvent.id)">Sim</button>
                <button class="btn btn-outline" @click="closeConfirmModal">Não</button>
            </div>
        </div>
    </dialog>
</template>

<script setup>
import { ref, inject } from 'vue'
import { useToast } from '@/services/toastService';
import { consultaService } from '@/services/consultaService';
import { horarioService } from '@/services/horarioService';
import { useAuthStore } from '@/store/autenticacao'

defineProps({
    calendarEvent: Object,
})

const toast = useToast()
const confirmDialog = ref(null)
const handleCreateEvent = inject('handleCreateEvent')
const handleCancelEvent = inject('handleCancelEvent')
const closeEventModal = inject('closeEventModal')
const auth = useAuthStore()

function openConfirmModal() {
    confirmDialog.value.showModal()
}

function closeConfirmModal() {
    confirmDialog.value.close()
}

function isNumeric(value) {
    return /^-?\d+$/.test(value);
}

async function confirmCancel(consultaId) {
    const id = parseInt(consultaId.replace('c-', ''))

    const response = await consultaService.alterarStatus(id, 'Cancelada')
    if (response.status === 'Cancelada') {
        handleCancelEvent(consultaId)

        if (auth.tipoUsuario === 'Medico') {
            handleCreateEvent({
                id: `h-${response.horario.id}`,
                title: 'Horário disponível',
                start: formatDate(response.horario.data, response.horario.horaInicio),
                end: formatDate(response.horario.data, response.horario.horaFim),
                _options: { tipo: 'horario' },
            })
        }

        closeConfirmModal()
        closeEventModal()
    } else {
        toast.error('Erro ao cancelar a consulta.')
    }
}

async function cancelaHorario(horarioId) {
    const id = parseInt(horarioId.replace('h-', ''))

    try {
        await horarioService.cancela(id).then(() => {
            toast.success('Horário removido com sucesso!')
            handleCancelEvent(horarioId)
            closeEventModal()
        })
    } catch (error) {
        toast.error(error.response?.data?.message)
    }
}

function formatEventDate(start, end) {
    const startDate = new Date(start)
    const endDate = new Date(end)
    const pad = n => String(n).padStart(2, '0')

    const startStr = `${pad(startDate.getDate())}/${pad(startDate.getMonth() + 1)}/${startDate.getFullYear()} das ${pad(startDate.getHours())}:${pad(startDate.getMinutes())}`
    const endStr = `${pad(endDate.getHours())}:${pad(endDate.getMinutes())}`

    return `${startStr} às ${endStr}`
}

function formatDate(dateString, timeString) {
    const date = new Date(dateString);
    date.setDate(date.getDate() + 1);

    const yyyy = date.getFullYear();
    const mm = String(date.getMonth() + 1).padStart(2, '0');
    const dd = String(date.getDate()).padStart(2, '0');

    const [hours, minutes] = timeString.split(':');

    return `${yyyy}-${mm}-${dd} ${hours.padStart(2, '0')}:${minutes.padStart(2, '0')}`;
}

</script>