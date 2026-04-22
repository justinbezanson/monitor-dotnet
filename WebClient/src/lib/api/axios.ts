import axios from 'axios'

const api = axios.create({
  baseURL: 'http://localhost:5144',
  headers: {
    'Content-Type': 'application/json',
  },
})

// Add a request interceptor to add the auth token to every request
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// Add a response interceptor to handle 401 Unauthorized errors
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token')
      // Redirect to login page
      window.location.href = '/login'
    }
    return Promise.reject(error)
  }
)

export default api
