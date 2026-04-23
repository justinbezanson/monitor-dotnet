  <script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useMonitorStore } from '@/stores/monitors'
import { 
  Monitor, Globe, Activity, ShieldAlert, Settings, LogOut, Search, Bell, 
  ChevronDown, CheckCircle2, AlertCircle, XCircle, Clock, Zap, Link,
  Menu, X, Plus, Trash2, Loader2, ExternalLink, Pencil
} from 'lucide-vue-next'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import ThemeToggle from '@/components/ThemeToggle.vue'

const router = useRouter()
const authStore = useAuthStore()
const monitorStore = useMonitorStore()
const isSidebarOpen = ref(true)
const isAddModalOpen = ref(false)
const isEditModalOpen = ref(false)

// Add Monitor Form
const newMonitor = ref({
  name: '', 
  url: '',
  port: undefined as number | undefined,
  intervalSeconds: 60
})

// Edit Monitor Form
const editingMonitor = ref({
  id: '',
  name: '',
  url: '',
  port: undefined as number | undefined,
  intervalSeconds: 60,
  isEnabled: true
})

const navItems = [
  { name: 'Dashboard', icon: Activity, active: true },
  { name: 'Endpoints', icon: Globe, active: false },
  { name: 'Incidents', icon: ShieldAlert, active: false },
  { name: 'Reports', icon: Monitor, active: false },
  { name: 'Settings', icon: Settings, active: false },
]

onMounted(async () => {
  await monitorStore.fetchMonitors()
})

const stats = computed(() => {
  const monitors = monitorStore.monitors
  const total = monitors.length
  const online = monitors.filter(m => m.currentStatus.toLowerCase() === 'online').length
  const offline = monitors.filter(m => m.currentStatus.toLowerCase() === 'offline').length
  const warning = monitors.filter(m => m.currentStatus.toLowerCase() === 'warning').length
  
  const uptime = total > 0 ? ((online / total) * 100).toFixed(2) + '%' : '100%'
  
  return [
    { label: 'Overall Uptime', value: uptime, icon: CheckCircle2, trend: 'stable', status: 'normal' },
    { label: 'Online', value: online.toString(), icon: Zap, trend: 'up', status: 'normal' },
    { label: 'Offline', value: offline.toString(), icon: XCircle, trend: 'none', status: offline > 0 ? 'critical' : 'normal' },
    { label: 'Warning', value: warning.toString(), icon: AlertCircle, trend: 'none', status: warning > 0 ? 'warning' : 'normal' },
  ]
})

const getStatusIcon = (status: string) => {
  switch (status.toLowerCase()) {
    case 'online': return CheckCircle2
    case 'warning': return AlertCircle
    case 'offline': return XCircle
    case 'pending': return Clock
    default: return CheckCircle2
  }
}

const getStatusColor = (status: string) => {
  switch (status.toLowerCase()) {
    case 'online': return 'text-green-500 bg-green-500/10'
    case 'warning': return 'text-yellow-500 bg-yellow-500/10'
    case 'offline': return 'text-red-500 bg-red-500/10'
    case 'pending': return 'text-blue-500 bg-blue-500/10'
    default: return 'text-muted-foreground bg-muted'
  }
}

const handleLogout = () => {
  authStore.logout()
  router.push('/login')
}

const handleAddMonitor = async () => {
  monitorStore.error = null
  
  // Ensure port is a number or null
  const portValue = newMonitor.value.port === '' || newMonitor.value.port === undefined ? null : Number(newMonitor.value.port)

  const success = await monitorStore.createMonitor({
    name: newMonitor.value.name,
    url: newMonitor.value.url,
    port: portValue,
    intervalSeconds: newMonitor.value.intervalSeconds
  })
  
  if (success) {
    isAddModalOpen.value = false
    newMonitor.value = { name: '', url: '', port: undefined, intervalSeconds: 60 }
  }
}

const handleDeleteMonitor = async (id: string) => {
  monitorStore.error = null
  if (confirm('Are you sure you want to delete this monitor?')) {
    await monitorStore.deleteMonitor(id)
  }
}

const handleEditMonitor = (monitor: any) => {
  monitorStore.error = null
  editingMonitor.value = {
    id: monitor.id,
    name: monitor.name,
    url: monitor.url,
    port: monitor.port ?? undefined,
    intervalSeconds: monitor.intervalSeconds,
    isEnabled: monitor.isEnabled
  }
  isEditModalOpen.value = true
}

const handleUpdateMonitor = async () => {
  monitorStore.error = null
  
  // Ensure port is a number or null
  const portValue = editingMonitor.value.port === '' || editingMonitor.value.port === undefined || editingMonitor.value.port === null ? null : Number(editingMonitor.value.port)

  const success = await monitorStore.updateMonitor(editingMonitor.value.id, {
    name: editingMonitor.value.name,
    url: editingMonitor.value.url,
    port: portValue,
    intervalSeconds: editingMonitor.value.intervalSeconds,
    isEnabled: editingMonitor.value.isEnabled
  })
  
  if (success) {
    isEditModalOpen.value = false
  }
}

const closeAddModal = () => {
  isAddModalOpen.value = false
  monitorStore.error = null
}

const closeEditModal = () => {
  isEditModalOpen.value = false
  monitorStore.error = null
}

const formatLastChecked = (date: string | null) => {
  if (!date) return 'Never'
  const now = new Date()
  const checked = new Date(date)
  const diff = Math.floor((now.getTime() - checked.getTime()) / 1000)
  
  if (diff < 60) return `${diff}s ago`
  if (diff < 3600) return `${Math.floor(diff / 60)}m ago`
  return checked.toLocaleTimeString()
}
</script>

<template>
  <div class="flex min-h-screen bg-background text-foreground transition-colors duration-300 font-sans">
    <!-- Sidebar -->
    <aside 
      :class="[
        'fixed inset-y-0 left-0 z-50 w-64 bg-card border-r transition-transform duration-300 lg:static lg:translate-x-0',
        isSidebarOpen ? 'translate-x-0' : '-translate-x-full'
      ]"
    >
      <div class="h-full flex flex-col p-4">
        <div class="flex items-center gap-3 px-2 mb-8">
          <div class="p-2 bg-primary rounded-xl text-primary-foreground shadow-lg shadow-primary/20">
            <Activity class="w-6 h-6" />
          </div>
          <span class="text-xl font-bold tracking-tight">MonitorDotNet</span>
        </div>

        <nav class="flex-1 space-y-1">
          <a 
            v-for="item in navItems" 
            :key="item.name"
            href="#" 
            :class="[
              'flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium transition-all group',
              item.active ? 'bg-primary text-primary-foreground shadow-md shadow-primary/10' : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground'
            ]"
          >
            <component :is="item.icon" class="w-5 h-5 opacity-80 group-hover:opacity-100" />
            {{ item.name }}
          </a>
        </nav>

        <div class="pt-4 border-t space-y-1">
          <div class="flex items-center gap-3 px-3 py-4 mb-2">
            <div class="w-10 h-10 rounded-xl bg-gradient-to-br from-primary/20 to-primary/5 flex items-center justify-center text-primary border border-primary/10">
              <span class="font-bold">{{ authStore.user?.email?.[0].toUpperCase() || 'U' }}</span>
            </div>
            <div class="flex-1 min-w-0">
              <p class="text-sm font-semibold truncate">{{ authStore.user?.email || 'User' }}</p>
              <p class="text-xs text-muted-foreground truncate">System Administrator</p>
            </div>
          </div>
          <button @click="handleLogout" class="w-full flex items-center gap-3 px-3 py-2 rounded-xl text-sm font-medium text-red-500 hover:bg-red-500/10 transition-colors">
            <LogOut class="w-5 h-5" />
            Sign Out
          </button>
        </div>
      </div>
    </aside>

    <!-- Main Content -->
    <div class="flex-1 flex flex-col min-w-0 overflow-hidden">
      <!-- Header -->
      <header class="h-16 border-b bg-card/50 backdrop-blur-xl sticky top-0 z-40 flex items-center justify-between px-4 sm:px-8">
        <div class="flex items-center gap-4 flex-1">
          <Button variant="ghost" size="icon" class="lg:hidden rounded-xl" @click="isSidebarOpen = !isSidebarOpen">
            <Menu v-if="!isSidebarOpen" class="h-5 w-5" />
            <X v-else class="h-5 w-5" />
          </Button>
          <div class="max-w-md w-full relative hidden sm:block">
            <Search class="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
            <input 
              type="text" 
              placeholder="Search monitors, logs, or endpoints..." 
              class="w-full pl-10 pr-4 py-2 bg-background/50 border rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-primary/20 transition-all border-border/50"
            />
          </div>
        </div>

        <div class="flex items-center gap-2 sm:gap-4">
          <Button variant="ghost" size="icon" class="rounded-xl relative">
            <Bell class="h-5 w-5" />
            <span class="absolute top-2 right-2 w-2 h-2 bg-red-500 rounded-full border-2 border-card"></span>
          </Button>
          <ThemeToggle />
          <div class="h-8 w-px bg-border/50 mx-1"></div>
          <Button variant="ghost" class="hidden sm:flex items-center gap-2 font-normal rounded-xl px-2">
            <div class="w-8 h-8 rounded-lg bg-primary/10 text-primary flex items-center justify-center font-bold text-xs border border-primary/20">
              {{ authStore.user?.email?.[0].toUpperCase() || 'U' }}
            </div>
            <ChevronDown class="h-4 w-4 text-muted-foreground" />
          </Button>
        </div>
      </header>

      <!-- Main Section -->
      <main class="flex-1 overflow-y-auto p-4 sm:p-8 space-y-8 bg-muted/5">
        <div class="flex flex-col sm:flex-row sm:items-end justify-between gap-4">
          <div>
            <h2 class="text-3xl font-bold tracking-tight text-foreground/90">Monitor Overview</h2>
            <p class="text-muted-foreground mt-1">Real-time status for your global URL endpoints.</p>
          </div>
          <div class="flex gap-2">
            <Button variant="outline" class="rounded-xl" @click="monitorStore.fetchMonitors()" :disabled="monitorStore.loading">
              <Loader2 v-if="monitorStore.loading" class="w-4 h-4 mr-2 animate-spin" />
              Refresh
            </Button>
            <Button class="rounded-xl shadow-lg shadow-primary/20" @click="isAddModalOpen = true">
              <Plus class="w-4 h-4 mr-2" />
              Add New Monitor
            </Button>
          </div>
        </div>

        <div v-if="monitorStore.error" class="p-4 bg-red-500/10 border border-red-500/20 rounded-xl text-red-500 text-sm">
          {{ monitorStore.error }}
        </div>

        <!-- Summary Stats -->
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 sm:gap-6">
          <div 
            v-for="stat in stats" 
            :key="stat.label"
            class="p-6 rounded-2xl border bg-card shadow-sm hover:shadow-md transition-all group relative overflow-hidden"
          >
            <div class="flex items-center justify-between mb-4 relative z-10">
              <div :class="[
                'p-2.5 rounded-xl transition-colors',
                stat.status === 'critical' ? 'bg-red-500/10 text-red-500' : 
                stat.status === 'warning' ? 'bg-yellow-500/10 text-yellow-500' : 'bg-primary/10 text-primary'
              ]">
                <component :is="stat.icon" class="w-6 h-6" />
              </div>
            </div>
            <div class="space-y-1 relative z-10">
              <p class="text-sm font-medium text-muted-foreground">{{ stat.label }}</p>
              <div class="flex items-baseline gap-2">
                <span class="text-2xl font-bold tracking-tight">{{ stat.value }}</span>
              </div>
            </div>
            <!-- Decorative Background Element -->
            <div class="absolute -right-4 -bottom-4 opacity-[0.03] group-hover:opacity-[0.05] transition-opacity">
              <component :is="stat.icon" class="w-24 h-24" />
            </div>
          </div>
        </div>

        <!-- Detailed View (Table) -->
        <div class="bg-card rounded-2xl border shadow-sm overflow-hidden">
          <div class="p-6 border-b flex items-center justify-between gap-4 flex-wrap bg-card/50">
            <div class="space-y-1">
              <h3 class="font-bold text-lg">Active Monitors</h3>
              <p class="text-sm text-muted-foreground">Tracking {{ monitorStore.monitors.length }} endpoints.</p>
            </div>
          </div>
          <div class="overflow-x-auto">
            <table class="w-full text-left text-sm border-collapse">
              <thead>
                <tr class="bg-muted/30 text-muted-foreground font-semibold border-b">
                  <th class="px-6 py-4">Endpoint</th>
                  <th class="px-6 py-4">Status</th>
                  <th class="px-6 py-4 text-center">Interval</th>
                  <th class="px-6 py-4">Last Check</th>
                  <th class="px-6 py-4 text-right">Actions</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-border/50">
                <tr v-if="monitorStore.monitors.length === 0">
                  <td colspan="5" class="px-6 py-12 text-center text-muted-foreground">
                    No monitors found. Click "Add New Monitor" to get started.
                  </td>
                </tr>
                <tr v-for="monitor in monitorStore.monitors" :key="monitor.id" class="hover:bg-muted/20 transition-all group">
                  <td class="px-6 py-4">
                    <div class="flex flex-col">
                      <span class="font-bold text-foreground/90">{{ monitor.name }}</span>
                      <div class="flex items-center gap-2 mt-0.5">
                        <a :href="monitor.url" target="_blank" class="text-xs text-muted-foreground flex items-center gap-1 hover:text-primary transition-colors text-nowrap">
                          <Link class="w-3 h-3" />
                          {{ monitor.url }}
                          <ExternalLink class="w-2 h-2" />
                        </a>
                        <span v-if="monitor.port" class="px-1.5 py-0.5 rounded bg-primary/5 text-[10px] font-mono font-bold text-primary border border-primary/10">
                          PORT: {{ monitor.port }}
                        </span>
                      </div>
                    </div>
                  </td>
                  <td class="px-6 py-4">
                    <span :class="['inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-[11px] font-bold uppercase tracking-wider transition-all', getStatusColor(monitor.currentStatus)]">
                      <component :is="getStatusIcon(monitor.currentStatus)" class="w-3.5 h-3.5" />
                      {{ monitor.currentStatus }}
                    </span>
                  </td>
                  <td class="px-6 py-4 text-center">
                    <span class="px-2 py-0.5 rounded border border-border bg-background text-[10px] font-mono font-bold">
                      {{ monitor.intervalSeconds }}s
                    </span>
                  </td>
                  <td class="px-6 py-4 text-muted-foreground font-mono text-xs">{{ formatLastChecked(monitor.lastCheckedAt) }}</td>
                  <td class="px-6 py-4 text-right">
                    <div class="flex items-center justify-end gap-1">
                      <Button variant="ghost" size="icon" class="h-8 w-8 rounded-lg hover:text-primary" @click="handleEditMonitor(monitor)">
                        <Pencil class="h-4 w-4" />
                      </Button>
                      <Button variant="ghost" size="icon" class="h-8 w-8 rounded-lg hover:text-red-500" @click="handleDeleteMonitor(monitor.id)">
                        <Trash2 class="h-4 w-4" />
                      </Button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </main>
    </div>

    <!-- Add Monitor Modal -->
    <div v-if="isAddModalOpen" class="fixed inset-0 z-[100] flex items-center justify-center p-4">
      <div class="absolute inset-0 bg-background/80 backdrop-blur-sm" @click="closeAddModal"></div>
      <div class="relative w-full max-w-md bg-card border rounded-2xl shadow-2xl p-6 space-y-6">
        <div class="flex items-center justify-between">
          <h3 class="text-xl font-bold">Add New Monitor</h3>
          <Button variant="ghost" size="icon" class="rounded-xl" @click="closeAddModal">
            <X class="h-5 w-5" />
          </Button>
        </div>

        <div v-if="monitorStore.error" class="p-3 bg-red-500/10 border border-red-500/20 rounded-xl text-red-500 text-xs">
          {{ monitorStore.error }}
        </div>

        <div class="space-y-4">
          <div class="space-y-2">
            <Label for="name">Friendly Name</Label>
            <Input id="name" v-model="newMonitor.name" placeholder="My Awesome API" />
          </div>
          <div class="space-y-2">
            <Label for="url">URL</Label>
            <Input id="url" v-model="newMonitor.url" placeholder="https://api.example.com/health" />
          </div>
          <div class="space-y-2">
            <Label for="port">Port (optional)</Label>
            <Input id="port" type="number" v-model="newMonitor.port" placeholder="e.g. 8080" />
          </div>
          <div class="space-y-2">
            <Label for="interval">Check Interval (seconds)</Label>
            <Input id="interval" type="number" v-model="newMonitor.intervalSeconds" />
          </div>
        </div>

        <div class="flex gap-3 pt-2">
          <Button variant="outline" class="flex-1 rounded-xl" @click="closeAddModal">Cancel</Button>
          <Button class="flex-1 rounded-xl shadow-lg shadow-primary/20" @click="handleAddMonitor" :disabled="monitorStore.loading">
            <Loader2 v-if="monitorStore.loading" class="w-4 h-4 mr-2 animate-spin" />
            Create Monitor
          </Button>
        </div>
      </div>
    </div>
    <!-- Edit Monitor Modal -->
    <div v-if="isEditModalOpen" class="fixed inset-0 z-[100] flex items-center justify-center p-4">
      <div class="absolute inset-0 bg-background/80 backdrop-blur-sm" @click="closeEditModal"></div>
      <div class="relative w-full max-w-md bg-card border rounded-2xl shadow-2xl p-6 space-y-6">
        <div class="flex items-center justify-between">
          <h3 class="text-xl font-bold">Edit Monitor</h3>
          <Button variant="ghost" size="icon" class="rounded-xl" @click="closeEditModal">
            <X class="h-5 w-5" />
          </Button>
        </div>

        <div v-if="monitorStore.error" class="p-3 bg-red-500/10 border border-red-500/20 rounded-xl text-red-500 text-xs">
          {{ monitorStore.error }}
        </div>

        <div class="space-y-4">
          <div class="space-y-2">
            <Label for="edit-name">Friendly Name</Label>
            <Input id="edit-name" v-model="editingMonitor.name" />
          </div>
          <div class="space-y-2">
            <Label for="edit-url">URL</Label>
            <Input id="edit-url" v-model="editingMonitor.url" />
          </div>
          <div class="space-y-2">
            <Label for="edit-port">Port (optional)</Label>
            <Input id="edit-port" type="number" v-model="editingMonitor.port" />
          </div>
          <div class="space-y-2">
            <Label for="edit-interval">Check Interval (seconds)</Label>
            <Input id="edit-interval" type="number" v-model="editingMonitor.intervalSeconds" />
          </div>
          <div class="flex items-center gap-2 pt-2">
            <input type="checkbox" id="edit-enabled" v-model="editingMonitor.isEnabled" class="w-4 h-4 rounded border-gray-300 text-primary focus:ring-primary" />
            <Label for="edit-enabled" class="cursor-pointer">Monitoring Enabled</Label>
          </div>
        </div>

        <div class="flex gap-3 pt-2">
          <Button variant="outline" class="flex-1 rounded-xl" @click="closeEditModal">Cancel</Button>
          <Button class="flex-1 rounded-xl shadow-lg shadow-primary/20" @click="handleUpdateMonitor" :disabled="monitorStore.loading">
            <Loader2 v-if="monitorStore.loading" class="w-4 h-4 mr-2 animate-spin" />
            Save Changes
          </Button>
        </div>
      </div>
    </div>
  </div>
</template>
