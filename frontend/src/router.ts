import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/store/autenticacao'

import CoreLayout from '@/layouts/AppLayout.vue'

const HomeView = () => import('@/views/app/HomeView.vue')
const HistoricoView = () => import('@/views/app/HistoricoView.vue')
const AgendarConsultaView = () => import('@/views/app/AgendarConsultaView.vue')
const ListaMedicosView = () => import('@/views/app/ListaMedicosView.vue')

const LoginView = () => import('@/views/auth/LoginView.vue')

const NotFoundView = () => import('@/views/NotFoundView.vue')

const routes = [
	{
		path: '/',
		component: CoreLayout,
		children: [
			{ path: '', name: 'home', component: HomeView, meta: { title: 'Home', requiresAuth: true } },
			{ path: 'historico', name: 'historico', component: HistoricoView, meta: { title: 'Histórico', requiresAuth: true } },
			{ path: 'consultas/medicos', name: 'listaMedicos', component: ListaMedicosView, meta: { title: 'Agendar Consulta', requiresAuth: true } },
			{ path: 'consultas/medicos/:medicoId/agendar', name: 'medico', component: AgendarConsultaView, meta: { title: 'Detalhes do Médico', requiresAuth: true } },
		],
	},

	{
		path: '/autenticacao/login',
		name: 'login',
		component: LoginView,
		meta: { title: 'Login' },
	},
	{
		path: '/:pathMatch(.*)*',
		name: 'notFound',
		component: NotFoundView,
		meta: { title: '404 Not Found' },
	},
]

const router = createRouter({
	history: createWebHistory(import.meta.env.BASE_URL),
	routes,
})

router.beforeEach((to, from, next) => {
	const auth = useAuthStore()
	const token = auth.token || sessionStorage.getItem('token')

	if (to.meta.requiresAuth && !token) {
		next({ name: 'login' })
	} else {
		next()
	}
})

export default router
