<template>
    <dialog ref="dialogRef" class="modal" @click.self="closeModal">
        <div class="modal-box w-8/12 max-w-5xl relative">
            <button class="btn btn-sm btn-circle btn-ghost absolute right-2 top-2" @click="closeModal">✕</button>

            <h3 class="text-lg font-bold mb-4">
                {{ isCreation ? 'Criar Novo Horário' : 'Detalhes do Horário' }}
            </h3>

            <div class="py-4 space-y-4">
                <div class="space-y-8">
                    <div class="flex flex-col">
                        <label class="label"><span class="label-text font-medium">Dia</span></label>
                        <VueDatePicker v-model="date" placeholder="dd/mm/yyyy" :locale="'pt-BR'" :format="'dd/MM/yyyy'"
                            :clearable="false" :disabled="false" />
                    </div>

                    <div class="flex flex-col">
                        <label class="label"><span class="label-text font-medium">Horário de Início</span></label>
                        <input type="time" v-model="startTime" class="input input-bordered w-full" />
                    </div>

                    <div class="flex flex-col">
                        <label class="label"><span class="label-text font-medium">Horário de Fim</span></label>
                        <input type="time" v-model="endTime" class="input input-bordered w-full" />
                    </div>
                </div>
            </div>

            <div class="modal-action flex gap-2 mt-40">
                <button class="btn btn-primary" @click="save">Salvar</button>
                <button class="btn" @click="closeModal">Fechar</button>
            </div>
        </div>
    </dialog>
</template>

<script setup>
import { ref, inject } from 'vue'
import { useToast } from '@/services/toastService'
import VueDatePicker from '@vuepic/vue-datepicker'
import { horarioService } from '@/services/horarioService'
import '@vuepic/vue-datepicker/dist/main.css'

const props = defineProps({
    calendarEvent: {
        type: Object,
        default: () => ({})
    }
})

const toast = useToast()
const handleCreateEvent = inject('handleCreateEvent')

const dialogRef = ref(null)
const date = ref(null)
const startTime = ref('')
const endTime = ref('')

const isCreation = !props.calendarEvent?.id

const openModal = () => {
    dialogRef.value?.showModal()
}

const closeModal = () => dialogRef.value?.close()

function formatDate(date) {
    const year = date.getFullYear()
    const month = String(date.getMonth() + 1).padStart(2, '0')
    const day = String(date.getDate()).padStart(2, '0')

    return `${year}-${month}-${day}`
}

const save = async () => {
    if (!date.value || !startTime.value || !endTime.value) {
        toast.error('Por favor, preencha todos os campos.')
        return
    }

    const novoEvento = {
        data: formatDate(date.value),
        horaInicio: startTime.value,
        horaFim: endTime.value
    }

    await horarioService.cria(novoEvento).then((response) => {
        if (response.status === 200) {
            toast.success('Horário criado com sucesso!')
            handleCreateEvent({
                id: `h-${response.data.id}`,
                title: 'Horário disponível',
                start: `${novoEvento.data} ${novoEvento.horaInicio}`,
                end: `${novoEvento.data} ${novoEvento.horaFim}`,
                _options: { tipo: 'horario' }
            })
        } else {
            toast.error(response.data.erro)
        }
        closeModal()
    })
}

defineExpose({ openModal })
</script>