<template>
    <div class="p-6 bg-white text-black rounded-3xl shadow-lg border border-gray-200 max-w-md mx-auto">
        <div class="mb-6">
            <h3 class="text-2xl font-bold text-green-600">Agendar Consulta</h3>
            <p class="text-gray-500 text-sm">Revise os detalhes antes de confirmar</p>
        </div>

        <div class="space-y-4">
            <div class="flex items-center gap-3">
                <div class="p-3 bg-green-100 rounded-xl">
                    <i class="fa-solid fa-calendar text-green-600"></i>
                </div>
                <div>
                    <p class="text-gray-500 text-sm">Data</p>
                    <p class="font-semibold text-lg">
                        {{ formatDate(calendarEvent.start) }}
                    </p>
                </div>
            </div>

            <div class="flex items-center gap-3">
                <div class="p-3 bg-blue-100 rounded-xl">
                    <i class="fa-solid fa-clock text-blue-600"></i>
                </div>
                <div>
                    <p class="text-gray-500 text-sm">Horário</p>
                    <p class="font-semibold text-lg">
                        {{ formatTime(calendarEvent.start, calendarEvent.end) }}
                    </p>
                </div>
            </div>
        </div>

        <div class="flex justify-end gap-3 mt-6">
            <button class="btn btn-success" @click="agendarConsulta(calendarEvent.id)">
                Confirmar
            </button>
        </div>
    </div>
</template>

<script setup>
import '@vuepic/vue-datepicker/dist/main.css'

import { consultaService } from '@/services/consultaService'
import { useToast } from '@/services/toastService'
import { useRouter } from 'vue-router'

defineProps({
    calendarEvent: Object
})

const router = useRouter()
const toast = useToast()

const agendarConsulta = async (horarioId) => {
    await consultaService.agendar(horarioId).then(() => {
        toast.success('Consulta agendada com sucesso!')
        router.push({ name: 'home' })
    }).catch((message) => {
        toast.error(message.response.data.erro)
    })
}

function formatDate(date) {
    const d = new Date(date)
    const pad = n => String(n).padStart(2, '0')

    return `${pad(d.getDate())}/${pad(d.getMonth() + 1)}/${d.getFullYear()}`
}

function formatTime(start, end) {
    const s = new Date(start)
    const e = new Date(end)
    const pad = n => String(n).padStart(2, '0')

    return `${pad(s.getHours())}:${pad(s.getMinutes())} às ${pad(e.getHours())}:${pad(e.getMinutes())}`
}

</script>
