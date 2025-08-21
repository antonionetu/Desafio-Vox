<template>
	<div>
		<h2 class="text-gray-500">
			Histórico
		</h2>

		<!-- Se não tiver consultas -->
		<div v-if="consultas.length === 0" class="mt-8 text-center text-gray-400 italic">
			Você não possui histórico de consultas.
		</div>

		<!-- Se tiver consultas -->
		<ul v-else class="list bg-base-100 rounded-box shadow-md mt-8 max-h-[88vh] overflow-y-auto">
			<li v-for="consulta in consultas" :key="consulta.id" class="list-row">
				<img class="size-10 rounded-box d-flex my-auto"
					src="https://img.daisyui.com/images/profile/demo/1@94.webp" />

				<div class="list-col-grow d-flex my-auto">
					<div>
						{{ auth.tipoUsuario === 'Medico'
							? (consulta?.paciente?.nome || 'Paciente Desconhecido')
							: (consulta.horario?.medico?.nome || 'Médico Desconhecido')
						}}
					</div>

					<div v-if="auth.tipoUsuario === 'Paciente'" class="text-xs uppercase font-semibold opacity-60">
						{{ consulta.especialidade || 'Especialidade' }}
					</div>

					<div class="text-xs opacity-50">
						{{ formatDate(consulta.horario.data) }} das
						{{ consulta.horario.horaInicio }} às
						{{ consulta.horario.horaFim }}
					</div>
				</div>

				<div class="text-xl font-thin text-end opacity-50 d-flex flex-row items-center gap-1 my-auto">
					<span class="flex items-center gap-1">
						<p>{{ consulta.status }}</p>

						<svg v-if="consulta.status === 'Finalizada'" xmlns="http://www.w3.org/2000/svg"
							class="h-5 w-5 text-green-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
							<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
						</svg>

						<svg v-else-if="consulta.status === 'Cancelada'" xmlns="http://www.w3.org/2000/svg"
							class="h-5 w-5 text-red-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
							<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
								d="M6 18L18 6M6 6l12 12" />
						</svg>
					</span>
				</div>
			</li>
		</ul>
	</div>
</template>


<script setup>
import { ref, onMounted, inject } from 'vue';
import { useAuthStore } from '@/store/autenticacao';
import { pacienteService } from '@/services/pacienteService';
import { medicoService } from '@/services/medicoService';

const consultas = ref([]);
const isLoading = inject('isLoading');
const auth = useAuthStore();

const loadConsultas = async () => {
	let todas = [];

	if (auth.tipoUsuario === 'Paciente') {
		const finalizadas = await pacienteService.getConsultas('Finalizada');
		const canceladas = await pacienteService.getConsultas('Cancelada');
		todas = [...finalizadas, ...canceladas];
	} else if (auth.tipoUsuario === 'Medico') {
		const finalizadas = await medicoService.getConsultas('Finalizada');
		const canceladas = await medicoService.getConsultas('Cancelada');
		todas = [...finalizadas, ...canceladas];
	}

	todas.sort((a, b) => new Date(b.horario.data) - new Date(a.horario.data));
	consultas.value = todas;
};

const formatDate = (isoDate) => {
	const date = new Date(isoDate);
	return `${String(date.getDate()).padStart(2, '0')}/${String(
		date.getMonth() + 1
	).padStart(2, '0')}/${date.getFullYear()}`;
};

onMounted(async () => {
	await loadConsultas().then(() => {
		isLoading.value = false;
	});
});
</script>
