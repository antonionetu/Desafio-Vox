<template>
	<div class="min-h-screen flex items-center justify-center bg-base-200">
		<div class="card w-full max-w-sm shadow-xl bg-base-100">
			<div class="card-body">
				<img src="@/assets/images/voxpopiapp_logo.jpeg" alt="logo vox"
					class="rounded-full w-24 h-24 mx-auto mb-4" />
				<h2 class="text-2xl font-bold text-center mb-4">Login</h2>

				<div v-if="erro" class="bg-red-100 text-red-700 p-2 rounded mb-4 text-center">
					{{ erro }}
				</div>

				<form @submit.prevent="handleLogin" class="space-y-4">
					<div class="form-control">
						<label class="label">
							<span class="label-text">Login</span>
						</label>
						<input v-model="login" type="text" placeholder="Digite seu Login" class="input input-bordered"
							required name="login" />
					</div>

					<div class="form-control">
						<label class="label">
							<span class="label-text">Senha</span>
						</label>
						<input v-model="senha" type="password" placeholder="••••••••" class="input input-bordered"
							required name="senha" />
					</div>

					<div class="form-control mt-4">
						<button type="submit" class="btn btn-primary w-full flex justify-center items-center"
							:disabled="loading">
							<span v-if="!loading">Entrar</span>
							<svg v-else class="animate-spin h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg"
								fill="none" viewBox="0 0 24 24">
								<circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor"
									stroke-width="4"></circle>
								<path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v4a4 4 0 00-4 4H4z">
								</path>
							</svg>
						</button>
					</div>
				</form>
			</div>
		</div>
	</div>
</template>

<script setup>
import { autenticacaoService } from '@/services/autenticacaoService'
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/store/autenticacao'

const login = ref('')
const senha = ref('')
const erro = ref('')
const loading = ref(false)

const router = useRouter()
const authStore = useAuthStore()

const handleLogin = async () => {
	erro.value = ''
	loading.value = true

	await autenticacaoService.login(login.value, senha.value).then((response) => {
		loading.value = false

		if (response.status === 200) {
			authStore.setToken(response.data.token)
			authStore.setTipoUsuario(response.data.tipo)

			router.push({ name: 'home' })
		} else {
			erro.value = response.response.data.erro
			loading.value = false
		}

	})
}
</script>