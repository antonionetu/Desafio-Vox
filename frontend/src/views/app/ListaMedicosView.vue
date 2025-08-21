<template>
    <div class="p-6 max-w-5xl mx-auto">
        <div class="flex items-center mb-6 space-x-4">
            <input v-model="searchQuery" type="text" placeholder="Buscar médicos..."
                class="bg-white p-4 rounded-xl flex-1 border-0 outline-none ring-0 focus:ring-0 focus:border-0" />

            <div class="dropdown">
                <label tabindex="0" class="btn btn-outline btn-square border-none">
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24"
                        stroke="currentColor" stroke-width="2">
                        <path stroke-linecap="round" stroke-linejoin="round"
                            d="M3 4a1 1 0 011-1h16a1 1 0 011 1v2a1 1 0 01-.293.707l-6 6V19a1 1 0 01-1.447.894L9 17.618V12.707l-6-6A1 1 0 013 6V4z" />
                    </svg>
                </label>
                <ul tabindex="0" class="dropdown-content menu p-2 shadow bg-base-100 rounded-box w-52">
                    <li v-for="esp in especialidades" :key="esp">
                        <a @click="toggleEspecialidade(esp)">
                            <input type="checkbox" :checked="selectedEspecialidades.includes(esp)" class="mr-2" />
                            {{ esp }}
                        </a>
                    </li>
                </ul>
            </div>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            <div v-for="medico in medicos" :key="medico.id" class="card shadow-lg bg-base-100">
                <div class="card-body">
                    <img class="size-10 rounded-box mt-2" src="https://img.daisyui.com/images/profile/demo/1@94.webp" />
                    <h2 class="card-title">{{ medico.nome }}</h2>
                    <p class="text-sm text-gray-500">{{ medico.especialidade }}</p>
                    <div class="card-actions justify-end">
                        <button class="btn btn-primary" @click="agendar(medico.id)">
                            Agendar
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup>
import { ref, inject, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { medicoService } from '@/services/medicoService'

const router = useRouter()
const isLoading = inject('isLoading')

const medicos = ref([])
const especialidades = ref([])
const selectedEspecialidades = ref([])
const searchQuery = ref('')

const toggleEspecialidade = (esp) => {
    if (selectedEspecialidades.value.includes(esp)) {
        selectedEspecialidades.value = selectedEspecialidades.value.filter(e => e !== esp)
    } else {
        selectedEspecialidades.value.push(esp)
    }
    carregarMedicos(searchQuery.value, selectedEspecialidades.value)
}

const agendar = (medicoId) => {
    router.push(`/consultas/medicos/${medicoId}/agendar`)
}

async function carregarMedicos(nome = '', especialidadeList = []) {
    try {
        const especialidadeParam = especialidadeList.join(',') || null
        const response = await medicoService.getAllMedicos({
            limit: 10,
            nome,
            especialidade: especialidadeParam
        })

        if (response) {
            medicos.value = response.medicos || []
            especialidades.value = response.especialidades || []
        }
    } catch (error) {
        console.error('Erro ao carregar médicos:', error)
    } finally {
        isLoading.value = false
    }
}

onMounted(() => {
    carregarMedicos()
})

watch(searchQuery, (newQuery) => {
    carregarMedicos(newQuery, selectedEspecialidades.value)
})
</script>