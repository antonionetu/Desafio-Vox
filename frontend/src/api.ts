import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_URL;

const api = axios.create({
	baseURL: API_BASE_URL,
	headers: {
		'Content-Type': 'application/json',
	},
});

api.interceptors.response.use(
	response => response,
	error => {
		if (error.response && error.response.status === 401) {
			const isInvalidToken = error.response.data?.error === "Token expirado.";
			const isLoginPage = window.location.pathname === '/autenticacao/login';

			if (isInvalidToken && !isLoginPage) {
				sessionStorage.removeItem('token');
				sessionStorage.removeItem('tipoUsuario');
				window.location.href = '/autenticacao/login';
			}
		}
		return Promise.reject(error);
	}
);


export default api;
