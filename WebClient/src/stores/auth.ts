import { defineStore } from 'pinia'
import api from '@/lib/api/axios'

interface User {
  id: string
  email: string
}

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: null as User | null,
    token: localStorage.getItem('token') || null,
    loading: false,
    error: null as string | null,
  }),
  getters: {
    isAuthenticated: (state) => !!state.token,
  },
  actions: {
    async register(email: string, password: string) {
      this.loading = true
      this.error = null
      try {
        await api.post('/register', { email, password })
        // After successful registration, usually the user should login
        return true
      } catch (err: any) {
        this.error = err.response?.data?.errors 
          ? Object.values(err.response.data.errors).flat().join(', ')
          : 'Registration failed. Please try again.'
        return false
      } finally {
        this.loading = false
      }
    },
    async login(email: string, password: string) {
      this.loading = true
      this.error = null
      try {
        const response = await api.post('/login', { email, password })
        // Identity API login endpoint returns a response with accessToken
        const { accessToken } = response.data
        this.token = accessToken
        localStorage.setItem('token', accessToken)
        // Set user if we had a /me endpoint, or just set email for now
        this.user = { id: '', email }
        return true
      } catch (err: any) {
        this.error = err.response?.data?.errors
          ? Object.values(err.response.data.errors).flat().join(', ')
          : 'Login failed. Please check your credentials.'
        return false
      } finally {
        this.loading = false
      }
    },
    logout() {
      this.user = null
      this.token = null
      localStorage.removeItem('token')
    },
  },
})
