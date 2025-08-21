<template>
	<div class="p-6 grid grid-cols-1 md:grid-cols-[2fr_1fr] gap-10">
		<div v-if="medico" class="flex flex-col items-center md:items-start space-y-4">
			<div class="flex items-center space-x-4 mb-6">
				<img class="w-24 h-24 rounded-full shadow" src="https://img.daisyui.com/images/profile/demo/1@94.webp"
					alt="Foto do médico" />
				<div>
					<h1 class="text-2xl font-bold">{{ medico.nome }}</h1>
					<p class="text-gray-600">{{ medico.especialidade }}</p>
				</div>
			</div>

			<div class="text-center md:text-left space-y-2 w-full">
				<p class="text-gray-700">{{ medico.descricao || 'Descrição do médico não disponível.' }}</p>
				<div class="w-full mt-8">
					<video class="w-full rounded-lg shadow" controls autoplay>
						<source src="https://www.w3schools.com/html/mov_bbb.mp4" type="video/mp4" />
						Seu navegador não suporta o elemento de vídeo.
					</video>
				</div>
			</div>
		</div>

		<div :class="isLoadingPagina ? 'hidden' : 'block'">
			<h2 class="text-xl font-semibold mb-4">Horários disponíveis</h2>

			<div v-if="isLoadingHorarios" class="text-center py-6">
				<span class="loading loading-spinner text-success"></span>
			</div>

			<div v-else>
				<ScheduleXCalendar :calendar-app="calendarApp" :custom-components="{ eventModal: CriaConsultaModal }"
					class="max-w-full overflow-hidden" />
			</div>
		</div>
	</div>
</template>

<script setup>
import { ref, onMounted, inject } from 'vue'
import { useRoute } from 'vue-router'
import { medicoService } from '@/services/medicoService'

import { ScheduleXCalendar } from '@schedule-x/vue'
import { createCalendar, createViewDay } from '@schedule-x/calendar'
import { createEventModalPlugin } from '@schedule-x/event-modal'
import '@schedule-x/theme-default/dist/index.css'

import CriaConsultaModal from '@/components/app/CriaConsultaModal.vue'

const medico = ref(null)
const horarios = ref([])

const isLoadingPagina = inject('isLoading')
const isLoadingHorarios = ref(false)

const route = useRoute()
const hoje = new Date().toISOString().split('T')[0]

const eventModalPlugin = createEventModalPlugin()

const calendarApp = createCalendar({
	selectedDate: hoje,
	plugins: [eventModalPlugin],
	views: [createViewDay()],
	locale: 'pt-BR',
	formats: { date: 'dd-MM-yyyy', time: 'HH:mm' },
	events: [],
	onEventClick: (event) => {
		horarioSelecionado.value = event
		consultaModal.value?.openModal()
	},
})

const consultaModal = ref(null)

const carregarPerfil = async (id) => {
	const response = await medicoService.getMedicoById(id)
	medico.value = response
}

const carregarHorarios = async (id) => {
	const response = await medicoService.getHorariosByMedicoId()
	horarios.value = response

	const eventos = horarios.value.map((h) => ({
		id: h.id,
		title: `Disponível`,
		start: `${h.data.split('T')[0]} ${h.horaInicio.slice(0, 5)}`,
		end: `${h.data.split('T')[0]} ${h.horaFim.slice(0, 5)}`,
	}))

	calendarApp.events.set(eventos)
}

onMounted(async () => {
	const medicoId = route.params.medicoId

	await carregarPerfil(medicoId)
	isLoadingPagina.value = false

	isLoadingHorarios.value = true
	await carregarHorarios(medicoId)
	isLoadingHorarios.value = false
})
</script>