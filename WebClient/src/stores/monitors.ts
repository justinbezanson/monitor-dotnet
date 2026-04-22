import { defineStore } from 'pinia'
import { monitorsService } from '@/lib/api/monitors'
import type { Monitor, CreateMonitorRequest, UpdateMonitorRequest } from '@/lib/api/monitors.types'

export const useMonitorStore = defineStore('monitors', {
  state: () => ({
    monitors: [] as Monitor[],
    loading: false,
    error: null as string | null,
  }),
  actions: {
    async fetchMonitors() {
      this.loading = true
      this.error = null
      try {
        this.monitors = await monitorsService.list()
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Failed to fetch monitors.'
      } finally {
        this.loading = false
      }
    },
    async createMonitor(request: CreateMonitorRequest) {
      this.loading = true
      this.error = null
      try {
        const newMonitor = await monitorsService.create(request)
        this.monitors.push(newMonitor)
        return true
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Failed to create monitor.'
        return false
      } finally {
        this.loading = false
      }
    },
    async updateMonitor(id: string, request: UpdateMonitorRequest) {
      this.loading = true
      this.error = null
      try {
        const updatedMonitor = await monitorsService.update(id, request)
        const index = this.monitors.findIndex(m => m.id === id)
        if (index !== -1) {
          this.monitors[index] = updatedMonitor
        }
        return true
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Failed to update monitor.'
        return false
      } finally {
        this.loading = false
      }
    },
    async deleteMonitor(id: string) {
      this.loading = true
      this.error = null
      try {
        await monitorsService.delete(id)
        this.monitors = this.monitors.filter(m => m.id !== id)
        return true
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Failed to delete monitor.'
        return false
      } finally {
        this.loading = false
      }
    },
  },
})
