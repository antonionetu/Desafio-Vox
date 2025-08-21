<template>
    <div class="flex">
        <Sidebar />
        <main class="flex-1 p-6 relative">
            <div v-if="isLoading" class="absolute inset-0 flex items-center justify-center bg-opacity-70 z-10">
                <span class="loading loading-spinner loading-lg text-success"></span>
            </div>
            <router-view />
        </main>
    </div>
</template>

<script setup>
import { ref, provide } from 'vue'
import { useRouter } from 'vue-router'
import Sidebar from '@/components/app/Sidebar.vue'

const isLoading = ref(false)
const router = useRouter()

router.beforeEach((to, from, next) => {
    isLoading.value = true
    next()
})

provide('isLoading', isLoading)
</script>