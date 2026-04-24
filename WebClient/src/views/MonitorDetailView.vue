<script setup lang="ts">
import { onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useMonitorStore } from '@/stores/monitors'
import { 
  ArrowLeft, CheckCircle2, XCircle, Clock, 
  ExternalLink, Link, Calendar, Zap, AlertCircle, RefreshCw,
  BarChart3, Shield, Info
} from 'lucide-vue-next'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'

const route = useRoute()
const router = useRouter()
const monitorStore = useMonitorStore()
const monitorId = route.params.id as string

const monitor = computed(() => monitorStore.currentMonitor)

onMounted(async () => {
  await monitorStore.fetchMonitorDetail(monitorId)
})

const getStatusColor = (status: string | undefined) => {
  if (!status) return 'text-muted-foreground'
  switch (status.toLowerCase()) {
    case 'online': return 'text-green-500'
    case 'warning': return 'text-yellow-500'
    case 'offline': return 'text-red-500'
    case 'pending': return 'text-blue-500'
    default: return 'text-muted-foreground'
  }
}

const getStatusBg = (status: string | undefined) => {
  if (!status) return 'bg-muted'
  switch (status.toLowerCase()) {
    case 'online': return 'bg-green-500/10 border-green-500/20'
    case 'warning': return 'bg-yellow-500/10 border-yellow-500/20'
    case 'offline': return 'bg-red-500/10 border-red-500/20'
    case 'pending': return 'bg-blue-500/10 border-blue-500/20'
    default: return 'bg-muted border-border'
  }
}

const formatDateTime = (date: string) => {
  return new Date(date).toLocaleString()
}

const uptimePercentage = computed(() => {
  if (!monitor.value || monitor.value.recentChecks.length === 0) return '100%'
  const successCount = monitor.value.recentChecks.filter(c => c.isSuccess).length
  return ((successCount / monitor.value.recentChecks.length) * 100).toFixed(1) + '%'
})

const averageResponseTime = computed(() => {
  if (!monitor.value || monitor.value.recentChecks.length === 0) return '0ms'
  const total = monitor.value.recentChecks.reduce((acc, c) => acc + c.responseTimeMs, 0)
  return Math.round(total / monitor.value.recentChecks.length) + 'ms'
})

</script>

<template>
  <div class="min-h-screen bg-muted/5 p-4 sm:p-8 space-y-8 font-sans">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 max-w-7xl mx-auto w-full">
      <div class="flex items-center gap-4">
        <Button variant="ghost" size="icon" class="rounded-xl border shadow-sm bg-card" @click="router.push('/')">
          <ArrowLeft class="w-5 h-5" />
        </Button>
        <div>
          <h1 class="text-3xl font-bold tracking-tight flex items-center gap-3">
            {{ monitor?.name || 'Monitor Detail' }}
            <span v-if="monitor" :class="['text-xs uppercase px-2.5 py-1 rounded-full border font-bold tracking-wider', getStatusBg(monitor.currentStatus), getStatusColor(monitor.currentStatus)]">
              {{ monitor.currentStatus }}
            </span>
          </h1>
          <div class="flex items-center gap-3 mt-1">
            <a v-if="monitor" :href="monitor.url" target="_blank" class="text-sm text-muted-foreground hover:text-primary transition-colors flex items-center gap-1.5 font-mono">
              <Link class="w-3.5 h-3.5" />
              {{ monitor.url }}{{ monitor.port ? `:${monitor.port}` : '' }}
              <ExternalLink class="w-3 h-3" />
            </a>
          </div>
        </div>
      </div>
      <div class="flex gap-2">
        <Button variant="outline" class="rounded-xl" @click="monitorStore.fetchMonitorDetail(monitorId)" :disabled="monitorStore.loading">
          <RefreshCw :class="['w-4 h-4 mr-2', monitorStore.loading ? 'animate-spin' : '']" />
          Refresh Report
        </Button>
      </div>
    </div>

    <div v-if="monitorStore.error" class="max-w-7xl mx-auto w-full p-4 bg-red-500/10 border border-red-500/20 rounded-xl text-red-500 text-sm">
      {{ monitorStore.error }}
    </div>

    <div v-if="monitor" class="max-w-7xl mx-auto w-full space-y-8">
      <!-- Stats Summary -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
        <Card class="rounded-2xl overflow-hidden shadow-sm border-border/50">
          <CardHeader class="pb-2">
            <CardDescription class="flex items-center gap-2">
              <Shield class="w-4 h-4" /> Uptime (Last 20)
            </CardDescription>
            <CardTitle class="text-2xl font-bold text-green-500">{{ uptimePercentage }}</CardTitle>
          </CardHeader>
        </Card>
        
        <Card class="rounded-2xl overflow-hidden shadow-sm border-border/50">
          <CardHeader class="pb-2">
            <CardDescription class="flex items-center gap-2">
              <Zap class="w-4 h-4" /> Avg. Latency
            </CardDescription>
            <CardTitle class="text-2xl font-bold text-primary">{{ averageResponseTime }}</CardTitle>
          </CardHeader>
        </Card>

        <Card class="rounded-2xl overflow-hidden shadow-sm border-border/50">
          <CardHeader class="pb-2">
            <CardDescription class="flex items-center gap-2">
              <Clock class="w-4 h-4" /> Check Interval
            </CardDescription>
            <CardTitle class="text-2xl font-bold text-foreground">{{ monitor.intervalSeconds }}s</CardTitle>
          </CardHeader>
        </Card>

        <Card class="rounded-2xl overflow-hidden shadow-sm border-border/50">
          <CardHeader class="pb-2">
            <CardDescription class="flex items-center gap-2">
              <Calendar class="w-4 h-4" /> Last Checked
            </CardDescription>
            <CardTitle class="text-xl font-bold truncate">
              {{ monitor.lastCheckedAt ? new Date(monitor.lastCheckedAt).toLocaleTimeString() : 'Never' }}
            </CardTitle>
          </CardHeader>
        </Card>
      </div>

      <!-- Main Content Grid -->
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
        <!-- Recent History List -->
        <Card class="lg:col-span-2 rounded-2xl shadow-sm border-border/50 overflow-hidden">
          <CardHeader class="bg-muted/30 border-b">
            <div class="flex items-center justify-between">
              <div>
                <CardTitle class="text-lg">Recent Check History</CardTitle>
                <CardDescription>Detailed logs of the last 20 health checks.</CardDescription>
              </div>
              <BarChart3 class="w-5 h-5 text-muted-foreground opacity-50" />
            </div>
          </CardHeader>
          <CardContent class="p-0">
            <div class="overflow-x-auto">
              <table class="w-full text-left text-sm">
                <thead>
                  <tr class="text-muted-foreground font-medium border-b bg-muted/10">
                    <th class="px-6 py-3">Timestamp</th>
                    <th class="px-6 py-3">Status</th>
                    <th class="px-6 py-3">Response</th>
                    <th class="px-6 py-3">Latency</th>
                    <th class="px-6 py-3">Notes</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-border/50">
                  <tr v-for="check in monitor.recentChecks" :key="check.id" class="hover:bg-muted/20 transition-all">
                    <td class="px-6 py-4 font-mono text-xs text-muted-foreground">
                      {{ formatDateTime(check.timestamp) }}
                    </td>
                    <td class="px-6 py-4">
                      <span v-if="check.isSuccess" class="inline-flex items-center gap-1.5 text-green-500 font-bold">
                        <CheckCircle2 class="w-4 h-4" /> SUCCESS
                      </span>
                      <span v-else class="inline-flex items-center gap-1.5 text-red-500 font-bold">
                        <XCircle class="w-4 h-4" /> FAILED
                      </span>
                    </td>
                    <td class="px-6 py-4">
                      <span :class="['px-2 py-0.5 rounded font-mono text-xs font-bold border', check.isSuccess ? 'bg-green-500/10 text-green-600 border-green-500/20' : 'bg-red-500/10 text-red-600 border-red-500/20']">
                        {{ check.statusCode || '???' }}
                      </span>
                    </td>
                    <td class="px-6 py-4 font-mono text-xs">
                      {{ check.responseTimeMs }}ms
                    </td>
                    <td class="px-6 py-4 text-xs text-muted-foreground max-w-xs truncate">
                      {{ check.errorMessage || 'None' }}
                    </td>
                  </tr>
                  <tr v-if="monitor.recentChecks.length === 0">
                    <td colspan="5" class="px-6 py-12 text-center text-muted-foreground">
                      No check history available yet.
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </CardContent>
        </Card>

        <!-- Sidebar Info -->
        <div class="space-y-6">
          <Card class="rounded-2xl shadow-sm border-border/50 overflow-hidden">
            <CardHeader class="bg-muted/30 border-b">
              <CardTitle class="text-lg flex items-center gap-2">
                <Info class="w-5 h-5" /> Endpoint Config
              </CardTitle>
            </CardHeader>
            <CardContent class="p-6 space-y-4">
              <div>
                <Label class="text-xs text-muted-foreground uppercase tracking-widest">Internal ID</Label>
                <p class="font-mono text-[10px] break-all text-muted-foreground">{{ monitor.id }}</p>
              </div>
              <div>
                <Label class="text-xs text-muted-foreground uppercase tracking-widest">Monitoring</Label>
                <p class="text-sm font-medium">{{ monitor.isEnabled ? 'Active' : 'Paused' }}</p>
              </div>
              <div>
                <Label class="text-xs text-muted-foreground uppercase tracking-widest">Expected Status</Label>
                <p class="text-sm font-medium">200-299 OK</p>
              </div>
            </CardContent>
          </Card>

          <Card class="rounded-2xl shadow-sm border-yellow-500/20 bg-yellow-500/5 overflow-hidden">
            <CardContent class="p-6">
              <div class="flex gap-3">
                <AlertCircle class="w-5 h-5 text-yellow-500 shrink-0" />
                <div class="space-y-1">
                  <h4 class="text-sm font-bold text-yellow-700">Health Note</h4>
                  <p class="text-xs text-yellow-600/80 leading-relaxed">
                    This monitor uses a standard GET request. If your service requires specific headers or authentication, checks may fail with 401 or 403 errors.
                  </p>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  </div>
</template>
