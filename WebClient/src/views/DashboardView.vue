<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { 
  Monitor, Globe, Activity, ShieldAlert, Settings, LogOut, Search, Bell, 
  ChevronDown, CheckCircle2, AlertCircle, XCircle, Clock, Zap, Link,
  Menu, X, ArrowUpRight
} from 'lucide-vue-next'
import { Button } from '@/components/ui/button'
import ThemeToggle from '@/components/ThemeToggle.vue'

const router = useRouter()
const authStore = useAuthStore()
const isSidebarOpen = ref(true)

const navItems = [
  { name: 'Dashboard', icon: Activity, active: true },
  { name: 'Endpoints', icon: Globe, active: false },
  { name: 'Incidents', icon: ShieldAlert, active: false },
  { name: 'Reports', icon: Monitor, active: false },
  { name: 'Settings', icon: Settings, active: false },
]

const stats = [
  { label: 'Overall Uptime', value: '99.98%', icon: CheckCircle2, trend: 'stable', status: 'normal' },
  { label: 'Avg. Response', value: '245ms', icon: Clock, trend: 'down', status: 'normal' },
  { label: 'Total Requests', value: '1.2M', icon: Zap, trend: 'up', status: 'normal' },
  { label: 'Active Incidents', value: '1', icon: ShieldAlert, trend: 'up', status: 'critical' },
]

const monitors = [
  { name: 'Main API Gateway', url: 'https://api.monitor.net/v1', status: 'online', method: 'GET', responseTime: '124ms', lastChecked: '12s ago' },
  { name: 'Customer Portal', url: 'https://portal.monitor.net', status: 'online', method: 'GET', responseTime: '450ms', lastChecked: '45s ago' },
  { name: 'Auth Service', url: 'https://auth.monitor.net', status: 'offline', method: 'POST', responseTime: '0ms', lastChecked: '2m ago' },
  { name: 'Legacy Dashboard', url: 'https://old.monitor.net', status: 'warning', method: 'GET', responseTime: '1200ms', lastChecked: '1m ago' },
  { name: 'Static Assets CDN', url: 'https://cdn.monitor.net', status: 'online', method: 'HEAD', responseTime: '45ms', lastChecked: '10s ago' },
]

const getStatusIcon = (status: string) => {
  switch (status) {
    case 'online': return CheckCircle2
    case 'warning': return AlertCircle
    case 'offline': return XCircle
    default: return CheckCircle2
  }
}

const getStatusColor = (status: string) => {
  switch (status) {
    case 'online': return 'text-green-500 bg-green-500/10'
    case 'warning': return 'text-yellow-500 bg-yellow-500/10'
    case 'offline': return 'text-red-500 bg-red-500/10'
    default: return 'text-muted-foreground bg-muted'
  }
}

const handleLogout = () => {
  authStore.logout()
  router.push('/login')
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
              <span class="font-bold">JD</span>
            </div>
            <div class="flex-1 min-w-0">
              <p class="text-sm font-semibold truncate">{{ authStore.user?.email || 'Justin Doe' }}</p>
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
              JD
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
            <Button variant="outline" class="rounded-xl">Configure Alerts</Button>
            <Button class="rounded-xl shadow-lg shadow-primary/20">Add New Monitor</Button>
          </div>
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
              <div class="flex flex-col items-end">
                <span class="text-[10px] uppercase font-bold tracking-wider text-muted-foreground">Status</span>
                <span :class="['text-xs font-bold', stat.status === 'critical' ? 'text-red-500' : 'text-green-500']">
                  {{ stat.status === 'critical' ? 'Alert' : 'Stable' }}
                </span>
              </div>
            </div>
            <div class="space-y-1 relative z-10">
              <p class="text-sm font-medium text-muted-foreground">{{ stat.label }}</p>
              <div class="flex items-baseline gap-2">
                <span class="text-2xl font-bold tracking-tight">{{ stat.value }}</span>
                <span v-if="stat.trend === 'up'" class="text-xs font-bold text-green-500 flex items-center gap-0.5">
                   +2.4%
                </span>
                <span v-if="stat.trend === 'down'" class="text-xs font-bold text-red-500 flex items-center gap-0.5">
                   -1.2%
                </span>
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
              <p class="text-sm text-muted-foreground">Tracking response times and status codes across 12 endpoints.</p>
            </div>
            <div class="flex items-center gap-2">
              <div class="relative">
                <Search class="absolute left-2.5 top-1/2 -translate-y-1/2 h-3.5 w-3.5 text-muted-foreground" />
                <input 
                  type="text" 
                  placeholder="Filter list..." 
                  class="pl-8 pr-3 py-1.5 bg-background border rounded-lg text-xs focus:outline-none focus:ring-1 focus:ring-primary border-border/50 w-48"
                />
              </div>
              <Button variant="outline" size="sm" class="rounded-lg">Export CSV</Button>
            </div>
          </div>
          <div class="overflow-x-auto">
            <table class="w-full text-left text-sm border-collapse">
              <thead>
                <tr class="bg-muted/30 text-muted-foreground font-semibold border-b">
                  <th class="px-6 py-4">Endpoint</th>
                  <th class="px-6 py-4">Status</th>
                  <th class="px-6 py-4 text-center">Method</th>
                  <th class="px-6 py-4">Resp. Time</th>
                  <th class="px-6 py-4">Last Check</th>
                  <th class="px-6 py-4 text-right">Actions</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-border/50">
                <tr v-for="monitor in monitors" :key="monitor.name" class="hover:bg-muted/20 transition-all group">
                  <td class="px-6 py-4">
                    <div class="flex flex-col">
                      <span class="font-bold text-foreground/90">{{ monitor.name }}</span>
                      <span class="text-xs text-muted-foreground flex items-center gap-1 group-hover:text-primary transition-colors">
                        <Link class="w-3 h-3" />
                        {{ monitor.url }}
                      </span>
                    </div>
                  </td>
                  <td class="px-6 py-4">
                    <span :class="['inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-[11px] font-bold uppercase tracking-wider transition-all', getStatusColor(monitor.status)]">
                      <component :is="getStatusIcon(monitor.status)" class="w-3.5 h-3.5" />
                      {{ monitor.status }}
                    </span>
                  </td>
                  <td class="px-6 py-4 text-center">
                    <span class="px-2 py-0.5 rounded border border-border bg-background text-[10px] font-mono font-bold">
                      {{ monitor.method }}
                    </span>
                  </td>
                  <td class="px-6 py-4">
                    <div class="flex items-center gap-3">
                      <span :class="['font-mono font-medium', parseInt(monitor.responseTime) > 1000 ? 'text-red-500' : parseInt(monitor.responseTime) > 300 ? 'text-yellow-500' : 'text-green-500']">
                        {{ monitor.responseTime }}
                      </span>
                      <div class="h-1.5 w-16 bg-secondary rounded-full overflow-hidden hidden md:block">
                        <div 
                          class="h-full bg-primary transition-all duration-500" 
                          :style="{ width: monitor.status === 'offline' ? '0%' : (Math.min(parseInt(monitor.responseTime) / 10, 100) + '%') }"
                        ></div>
                      </div>
                    </div>
                  </td>
                  <td class="px-6 py-4 text-muted-foreground font-mono text-xs">{{ monitor.lastChecked }}</td>
                  <td class="px-6 py-4 text-right">
                    <div class="flex items-center justify-end gap-1">
                      <Button variant="ghost" size="icon" class="h-8 w-8 rounded-lg hover:text-primary">
                        <Activity class="h-4 w-4" />
                      </Button>
                      <Button variant="ghost" size="icon" class="h-8 w-8 rounded-lg hover:text-primary">
                        <ArrowUpRight class="h-4 w-4" />
                      </Button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
          <div class="p-4 border-t bg-muted/10 text-center">
            <Button variant="link" size="sm" class="text-muted-foreground font-medium hover:text-primary">
              View all 12 endpoints
            </Button>
          </div>
        </div>
      </main>
    </div>
  </div>
</template>
