import api from './axios'
import type { 
  Monitor, 
  MonitorDetail, 
  CreateMonitorRequest, 
  UpdateMonitorRequest 
} from './monitors.types'

export const monitorsService = {
  list: async (page = 1, pageSize = 20): Promise<Monitor[]> => {
    const response = await api.get('/api/monitors', { params: { page, pageSize } })
    return response.data.items ?? response.data
  },
  create: async (request: CreateMonitorRequest): Promise<Monitor> => {
    const response = await api.post('/api/monitors', request)
    return response.data
  },
  get: async (id: string, page = 1, pageSize = 20): Promise<MonitorDetail> => {
    const response = await api.get(`/api/monitors/${id}`, { params: { page, pageSize } })
    return response.data
  },
  update: async (id: string, request: UpdateMonitorRequest): Promise<Monitor> => {
    const response = await api.put(`/api/monitors/${id}`, request)
    return response.data
  },
  delete: async (id: string): Promise<void> => {
    await api.delete(`/api/monitors/${id}`)
  },
}
