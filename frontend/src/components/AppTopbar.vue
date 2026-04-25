<template>
  <header class="card-surface rounded-[2rem] px-6 py-4">
    <div class="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
      <div>
        <p class="text-xs font-bold uppercase tracking-[0.28em] text-[var(--app-muted)]">{{ route.meta.title }}</p>
        <h2 class="mt-2 text-2xl font-black tracking-tight text-slate-900">{{ headline }}</h2>
      </div>
      <div class="flex flex-wrap items-center gap-3">
        <RouterLink v-for="item in quickLinks" :key="item.name" class="rounded-full border border-[var(--app-border)] px-4 py-2 text-sm font-semibold text-[var(--app-muted)] transition hover:border-[var(--app-accent)] hover:text-[var(--app-accent)]" :to="{ name: item.name }">
          {{ item.label }}
        </RouterLink>
        <Button
          v-if="authStore.isAuthenticated"
          severity="contrast"
          label="Sign out"
          icon="pi pi-sign-out"
          @click="signOut"
        />
        <RouterLink v-else :to="{ name: 'login' }">
          <Button label="Login" icon="pi pi-arrow-right" />
        </RouterLink>
      </div>
    </div>
  </header>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Button from 'primevue/button'
import { useAuthStore } from '@/stores/auth'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

const headline = computed(() => {
  if (authStore.isAdmin) {
    return 'Modern equity operations for administrators'
  }
  if (authStore.isOptionee) {
    return 'Clear participant access to grants and vesting'
  }
  return 'A cleaner static SPA on top of the IncTrak API'
})

const quickLinks = computed(() => {
  if (authStore.isAdmin) {
    return [
      { name: 'stock-classes', label: 'Stock Classes' },
      { name: 'participants', label: 'Participants' },
      { name: 'grants', label: 'Grants' }
    ]
  }

  if (authStore.isOptionee) {
    return [
      { name: 'participant-stocks', label: 'Stock Summary' },
      { name: 'participant-options', label: 'Option Summary' },
      { name: 'participant-grants', label: 'Grant Details' }
    ]
  }

  return [
    { name: 'about', label: 'About' },
    { name: 'contact', label: 'Contact' },
    { name: 'privacy', label: 'Privacy' }
  ]
})

function signOut(): void {
  authStore.clearSession()
  router.push({ name: 'login' })
}
</script>
