<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Card, CardHeader, CardTitle, CardDescription, CardContent, CardFooter } from '@/components/ui/card'
import { Activity, ShieldAlert, Loader2 } from 'lucide-vue-next'

const router = useRouter()
const authStore = useAuthStore()

const email = ref('')
const password = ref('')
const confirmPassword = ref('')
const errorMsg = ref('')

const handleRegister = async () => {
  if (password.value !== confirmPassword.value) {
    errorMsg.value = 'Passwords do not match.'
    return
  }
  
  errorMsg.value = ''
  const success = await authStore.register(email.value, password.value)
  if (success) {
    // If registration is successful, automatically login
    const loginSuccess = await authStore.login(email.value, password.value)
    if (loginSuccess) {
      router.push('/')
    }
  }
}
</script>

<template>
  <div class="min-h-screen flex items-center justify-center bg-muted/30 p-4">
    <Card class="w-full max-w-md shadow-xl border-border/50 bg-card/80 backdrop-blur-sm">
      <CardHeader class="space-y-4 items-center text-center">
        <div class="p-3 bg-primary rounded-2xl text-primary-foreground shadow-lg shadow-primary/20">
          <Activity class="w-8 h-8" />
        </div>
        <div class="space-y-1">
          <CardTitle class="text-2xl font-bold tracking-tight">Create an account</CardTitle>
          <CardDescription>Get started monitoring your URLs today</CardDescription>
        </div>
      </CardHeader>
      <form @submit.prevent="handleRegister">
        <CardContent class="space-y-4">
          <div v-if="authStore.error || errorMsg" class="p-3 rounded-lg bg-red-500/10 border border-red-500/20 text-red-500 text-sm flex items-center gap-2">
            <ShieldAlert class="w-4 h-4 flex-shrink-0" />
            <span>{{ authStore.error || errorMsg }}</span>
          </div>
          <div class="space-y-2">
            <Label for="email">Email address</Label>
            <Input id="email" type="email" placeholder="name@example.com" v-model="email" required class="rounded-xl border-border/50 bg-background/50" />
          </div>
          <div class="space-y-2">
            <Label for="password">Password</Label>
            <Input id="password" type="password" placeholder="••••••••" v-model="password" required class="rounded-xl border-border/50 bg-background/50" />
          </div>
          <div class="space-y-2">
            <Label for="confirmPassword">Confirm Password</Label>
            <Input id="confirmPassword" type="password" placeholder="••••••••" v-model="confirmPassword" required class="rounded-xl border-border/50 bg-background/50" />
          </div>
        </CardContent>
        <CardFooter class="flex flex-col gap-4">
          <Button type="submit" class="w-full rounded-xl py-6 text-base font-semibold shadow-lg shadow-primary/20" :disabled="authStore.loading">
            <Loader2 v-if="authStore.loading" class="mr-2 h-4 w-4 animate-spin" />
            Create Account
          </Button>
          <div class="text-sm text-center text-muted-foreground">
            Already have an account? 
            <router-link to="/login" class="text-primary hover:underline font-semibold">Sign in</router-link>
          </div>
        </CardFooter>
      </form>
    </Card>
  </div>
</template>
