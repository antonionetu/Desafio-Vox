<template>
    <div>
        <button class="md:hidden p-2 m-2 rounded-lg bg-base-200" @click="isOpen = !isOpen">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24"
                stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
            </svg>
        </button>

        <aside :class="[
            'w-64 h-screen bg-white p-4 flex flex-col justify-between fixed top-0 left-0 z-50 transform transition-transform duration-300',
            isOpen ? 'translate-x-0' : '-translate-x-full',
            'md:translate-x-0 md:static md:flex'
        ]">
            <div class="mb-8">
                <p class="text-lg text-center font-semibold">Bem-vindo de volta, {{ user.name }}</p>
            </div>

            <nav class="flex flex-col gap-1">
                <router-link v-for="item in mainMenuItems" :key="item.name" :to="item.path"
                    class="flex items-center gap-3 px-3 py-2 rounded-lg transition-colors"
                    :class="isActive(item.path) ? 'bg-green-100 text-green-800 font-medium' : 'text-gray-900 hover:bg-gray-100'"
                    @click="isOpen = false">
                    <component :is="item.icon" class="w-5 h-5 flex-shrink-0" />
                    <span>{{ item.label }}</span>
                </router-link>
            </nav>

            <nav class="flex flex-col gap-1 mt-6">
                <button @click="logout"
                    class="flex items-center gap-3 px-3 py-2 rounded-lg text-red-600 hover:bg-red-100 transition-colors cursor-pointer">
                    <UserCircleIcon class="w-5 h-5 flex-shrink-0" />
                    <span>Sair</span>
                </button>
            </nav>
        </aside>

        <div v-if="isOpen" class="fixed inset-0 bg-black bg-opacity-30 md:hidden" @click="isOpen = false"></div>
    </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { CalendarIcon, ClockIcon, ClipboardDocumentListIcon, UserCircleIcon } from '@heroicons/vue/24/outline'
import { pacienteService } from '@/services/pacienteService'
import { medicoService } from '@/services/medicoService'

const route = useRoute()

const isOpen = ref(false)

const user = ref({ name: '', role: '' })

onMounted(async () => {
    user.value = { name: 'Carregando...', role: sessionStorage.getItem('tipoUsuario') }

    if (user.value.role === 'Paciente') {
        const paciente = await pacienteService.getPacienteById()
        user.value.name = paciente.nome.split(' ')[0]

    } else if (user.value.role === 'Medico') {
        const medico = await medicoService.getMedicoById()
        user.value.name = medico.nome.split(' ')[0]
    }
})

const mainMenuItems = computed(() => {
    if (user.value.role === 'Paciente') {
        return [
            { path: '/', name: 'agenda', label: 'Meus Agendamentos', icon: CalendarIcon },
            { path: '/consultas/medicos', name: 'listaMedicos', label: 'Agendar Consulta', icon: ClockIcon },
            { path: '/historico', name: 'historico', label: 'Histórico', icon: ClipboardDocumentListIcon }
        ]
    } else if (user.value.role === 'Medico') {
        return [
            { path: '/', name: 'agenda', label: 'Horários Marcados', icon: CalendarIcon },
            { path: '/historico', name: 'historico', label: 'Histórico', icon: ClipboardDocumentListIcon }
        ]
    }
    return []
})

function logout() {
    sessionStorage.removeItem('token')
    sessionStorage.removeItem('tipoUsuario')
    window.location.href = '/autenticacao/login'
}

function isActive(path) {
    return route.path === path
}
</script>
