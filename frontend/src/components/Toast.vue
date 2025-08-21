<template>
    <div class="fixed top-4 right-4 space-y-2 z-50">
        <transition-group name="fade" tag="div">
            <div v-for="toast in toasts" :key="toast.id" class="alert" :class="{
                'alert-success': toast.type === 'success',
                'alert-error': toast.type === 'error',
                'alert-warning': toast.type === 'warning',
                'alert-info': toast.type === 'info'
            }">
                <span>{{ toast.message }}</span>
            </div>
        </transition-group>
    </div>
</template>

<script setup>
import { ref } from 'vue'

const toasts = ref([])

let id = 0

function showToast(message, type = 'success', duration = 3000) {
    const newToast = { id: id++, message, type }
    toasts.value.push(newToast)

    setTimeout(() => {
        toasts.value = toasts.value.filter(t => t.id !== newToast.id)
    }, duration)
}

defineExpose({ showToast })
</script>

<style>
.fade-enter-active,
.fade-leave-active {
    transition: opacity 0.3s;
}

.fade-enter-from,
.fade-leave-to {
    opacity: 0;
}
</style>