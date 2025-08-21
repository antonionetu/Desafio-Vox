<script setup>
import { ref, onMounted, inject, provide } from 'vue'
import { ScheduleXCalendar } from '@schedule-x/vue'
import {
    createCalendar,
    createViewDay,
    createViewMonthAgenda,
    createViewMonthGrid,
    createViewWeek,
} from '@schedule-x/calendar'
import { createDragAndDropPlugin } from '@schedule-x/drag-and-drop'
import { createResizePlugin } from '@schedule-x/resize'
import { createEventModalPlugin } from '@schedule-x/event-modal'
import { createEventsServicePlugin } from '@schedule-x/events-service'
import '@schedule-x/theme-default/dist/index.css'

import * as signalR from '@microsoft/signalr'

import HorarioModal from '@/components/app/HorarioModal.vue'
import CriaHorarioModal from '@/components/app/CriaHorarioModal.vue'
import { pacienteService } from '@/services/pacienteService'
import { medicoService } from '@/services/medicoService'
import { useAuthStore } from '@/store/autenticacao'
import { horarioService } from '@/services/horarioService'
import { Token } from '@/services/utils'
import { useToast } from '@/services/toastService'

const toast = useToast()
const token = new Token()
const auth = useAuthStore()

const isLoading = inject('isLoading')
const hoje = new Date().toISOString().split('T')[0]
const eventModalPlugin = createEventModalPlugin()
const criaHorario = ref(null)

async function loadConsultas() {
    let consultas = []

    if (auth.tipoUsuario === 'Paciente') {
        consultas = await pacienteService.getConsultas('Agendada')

        return consultas.map(({ id, horario }) => ({
            id: `c-${id}`,
            title: `Consulta com Dr. ${horario.medico.nome}`,
            start: `${horario.data.split("T")[0]} ${horario.horaInicio.slice(0, 5)}`,
            end: `${horario.data.split("T")[0]} ${horario.horaFim.slice(0, 5)}`,
            _options: { tipo: 'consulta', especialidade: horario.medico.especialidade, horarioId: horario.id },
        }))
    }

    if (auth.tipoUsuario === 'Medico') {
        const horarios = await medicoService.getHorariosByMedicoId()
        const consultas = await medicoService.getConsultas('Agendada')

        const horariosEventos = horarios.map((horario) => ({
            id: `h-${horario.id}`,
            title: 'Horário disponível',
            start: `${horario.data.split("T")[0]} ${horario.horaInicio.slice(0, 5)}`,
            end: `${horario.data.split("T")[0]} ${horario.horaFim.slice(0, 5)}`,
            _options: { tipo: 'horario' },
        }))

        const consultasEventos = consultas.map((consulta) => ({
            id: `c-${consulta.id}`,
            title: `Consulta com ${consulta.paciente?.nome}`,
            start: `${consulta.horario.data.split("T")[0]} ${consulta.horario.horaInicio.slice(0, 5)}`,
            end: `${consulta.horario.data.split("T")[0]} ${consulta.horario.horaFim.slice(0, 5)}`,
            _options: { tipo: 'consulta', horarioId: consulta.horario.id, consulta }
        }))

        return [...horariosEventos, ...consultasEventos]
    }

    return consultas
}

async function updateConsultas(updatedEvent) {
    const { tipo } = updatedEvent._options || {}
    const data = updatedEvent.start.split(' ')[0]
    const horaInicio = updatedEvent.start.split(' ')[1]
    const horaFim = updatedEvent.end.split(' ')[1]

    const horarioId = tipo === 'horario'
        ? updatedEvent.id.split('-')[1]
        : updatedEvent._options?.horarioId

    const horario = await horarioService.atualiza(horarioId, { data, horaInicio, horaFim })
    return horario
}

const calendarApp = createCalendar({
    selectedDate: hoje,
    plugins: [
        ...(auth.tipoUsuario === 'Medico' ? [createDragAndDropPlugin(), createResizePlugin()] : []),
        eventModalPlugin,
        createEventsServicePlugin()
    ],
    views: [
        createViewDay(),
        createViewWeek(),
        createViewMonthGrid(),
        createViewMonthAgenda(),
    ],
    locale: 'pt-BR',
    formats: { date: 'dd-MM-yyyy', time: 'HH:mm' },
    events: [],
    callbacks: {
        async onBeforeEventUpdate(oldEvent, newEvent, $app) {
            try {
                const response = await updateConsultas(newEvent);
                if (response.status === 200) {
                    return true;
                } else {
                    toast.error(response.data.erro);
                    return false;
                }
            } catch (error) {
                toast.error('Falha ao atualizar o horário. Tente novamente.');
                return false;
            }
        }
    }
})

const connection = new signalR.HubConnectionBuilder()
    .withUrl(`http://localhost:5054/hub/horarios`, {
        accessTokenFactory: () => token.asString()
    })
    .withAutomaticReconnect()
    .build();

connection.on("HorarioAtualizado", (horarioResponse) => {
    const allEvents = calendarApp.events.getAll();
    const eventToUpdate = allEvents.find(e => e._options?.horarioId === horario.id);

    if (eventToUpdate) {
        eventToUpdate.start = `${horario.data.split("T")[0]} ${horario.horaInicio.slice(0, 5)}`;
        eventToUpdate.end = `${horario.data.split("T")[0]} ${horario.horaFim.slice(0, 5)}`;
        calendarApp.events.update(eventToUpdate);
    }
});

connection.on("ConsultaCancelada", (consulta) => {
    toast.warning(`Você teve uma consulta cancelada.`, 3000)

    calendarApp.events.remove(`c-${consulta.id}`);

    if (auth.tipoUsuario === 'Paciente') {
        return;
    }

    if (auth.tipoUsuario === 'Medico') {
        const horario = consulta.horario

        calendarApp.events.add({
            id: `h-${horario.id}`,
            title: 'Horário disponível',
            start: `${horario.data.split("T")[0]} ${horario.horaInicio.slice(0, 5)}`,
            end: `${horario.data.split("T")[0]} ${horario.horaFim.slice(0, 5)}`,
            _options: { tipo: 'horario' },
        });
    }
});

connection.on("ConsultaCriada", (consulta) => {
    if (auth.tipoUsuario === "Medico") {
        toast.warning(`Você tem uma nova consulta.`, 3000)
    }

    const allEvents = calendarApp.events.getAll();
    const eventToUpdate = allEvents.find(e => parseInt(e.id.replace("h-", "")) === consulta.horarioId);

    if (eventToUpdate) {
        eventToUpdate.id = `c-${consulta.id}`;
        eventToUpdate.title = `Consulta - ${consulta.paciente.nome}`;
        eventToUpdate._options = {
            tipo: "consulta"
        };

        calendarApp.events.remove(`h-${consulta.horarioId}`)
        calendarApp.events.add(eventToUpdate);
    }
});

connection.start()
    .then(() => console.log("Conectado ao HorarioHub"))
    .catch(err => console.error("Erro ao conectar SignalR:", err))

onMounted(async () => {
    const medicoEvents = await loadConsultas()
    calendarApp.events.set(medicoEvents)
    isLoading.value = false
})

function openCriaHorarioModal() {
    criaHorario.value?.openModal()
}

provide('handleCreateEvent', (event) => calendarApp.events.add(event))
provide('handleCancelEvent', (consultaId) => calendarApp.events.remove(consultaId))
provide('closeEventModal', () => eventModalPlugin.close())
</script>


<template>
    <div class="p-4">
        <div class="flex justify-between items-center mb-4">
            <h2 class="text-gray-500">
                {{ auth.tipoUsuario === 'Medico' ? 'Meus Horários' : 'Meus Agendamentos' }}
            </h2>

            <CriaHorarioModal ref="criaHorario" />
            <button v-if="auth.tipoUsuario === 'Medico'" @click="openCriaHorarioModal" class="btn btn-primary">
                Criar Novo Horário
            </button>
        </div>

        <ScheduleXCalendar :calendar-app="calendarApp" :custom-components="{ eventModal: HorarioModal }"
            class="max-w-full overflow-hidden" />
    </div>
</template>