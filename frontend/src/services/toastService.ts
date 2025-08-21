import { createApp } from 'vue'
import Toast from '@/components/Toast.vue'

let toastInstance

export function useToast() {
    if (!toastInstance) {
        const div = document.createElement('div')
        document.body.appendChild(div)

        const app = createApp(Toast)
        toastInstance = app.mount(div)
    }

    return {
        success: (msg, duration) => toastInstance.showToast(msg, 'success', duration),
        error: (msg, duration) => toastInstance.showToast(msg, 'error', duration),
        warning: (msg, duration) => toastInstance.showToast(msg, 'warning', duration),
        info: (msg, duration) => toastInstance.showToast(msg, 'info', duration)
    }
}
