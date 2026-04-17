<script setup lang="ts">
import { onMounted } from 'vue'

onMounted(() => {
  // Simple theme initialization
  const theme = localStorage.getItem('theme')
  if (theme === 'dark' || (!theme && window.matchMedia('(prefers-color-scheme: dark)').matches)) {
    document.documentElement.classList.add('dark')
  } else {
    document.documentElement.classList.remove('dark')
  }
})
</script>

<template>
  <router-view />
</template>

<style>
/* Smooth transition for theme toggle */
:root {
  transition: background-color 0.3s ease, color 0.3s ease;
}

/* Custom scrollbar */
::-webkit-scrollbar {
  width: 8px;
}
::-webkit-scrollbar-track {
  background: transparent;
}
::-webkit-scrollbar-thumb {
  background: color-mix(in srgb, var(--muted-foreground) 20%, transparent);
  border-radius: 9999px;
}
::-webkit-scrollbar-thumb:hover {
  background: color-mix(in srgb, var(--muted-foreground) 30%, transparent);
}
</style>
