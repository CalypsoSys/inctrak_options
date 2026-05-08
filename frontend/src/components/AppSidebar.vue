<template>
  <aside class="hidden w-72 shrink-0 rounded-[2rem] border border-white/40 bg-slate-950/94 p-6 text-slate-100 shadow-2xl xl:block">
    <RouterLink class="flex items-center gap-3" :to="{ name: 'home' }">
      <div class="flex h-11 w-11 items-center justify-center rounded-2xl bg-amber-400 text-lg font-black text-slate-950">I</div>
      <div>
        <p class="text-xs uppercase tracking-[0.3em] text-amber-200/70">IncTrak</p>
        <p class="text-lg font-bold">Operations Hub</p>
      </div>
    </RouterLink>
    <div class="mt-8 space-y-7">
      <section v-if="authStore.isAdmin">
        <p class="mb-3 text-xs font-bold uppercase tracking-[0.28em] text-slate-400">Admin</p>
        <nav class="space-y-1">
          <RouterLink v-for="item in adminItems" :key="item.name" class="sidebar-link" :class="{ 'sidebar-link-active': isActive(item.name) }" :to="{ name: item.name }">
            <i :class="['pi', item.icon]" />
            <span>{{ item.label }}</span>
          </RouterLink>
        </nav>
      </section>
      <section v-if="authStore.isOptionee">
        <p class="mb-3 text-xs font-bold uppercase tracking-[0.28em] text-slate-400">Participant</p>
        <nav class="space-y-1">
          <RouterLink v-for="item in optioneeItems" :key="item.name" class="sidebar-link" :class="{ 'sidebar-link-active': isActive(item.name) }" :to="{ name: item.name }">
            <i :class="['pi', item.icon]" />
            <span>{{ item.label }}</span>
          </RouterLink>
        </nav>
      </section>
      <section>
        <p class="mb-3 text-xs font-bold uppercase tracking-[0.28em] text-slate-400">Public</p>
        <nav class="space-y-1">
          <RouterLink v-for="item in publicItems" :key="item.name" class="sidebar-link" :class="{ 'sidebar-link-active': isActive(item.name) }" :to="{ name: item.name }">
            <i :class="['pi', item.icon]" />
            <span>{{ item.label }}</span>
          </RouterLink>
        </nav>
      </section>
    </div>
  </aside>
</template>

<script setup lang="ts">
import { useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const route = useRoute()
const authStore = useAuthStore()

const adminItems = [
  { name: 'stock-classes', label: 'Stock Classes', icon: 'pi-box' },
  { name: 'stock-holders', label: 'Stock Holders', icon: 'pi-wallet' },
  { name: 'plans', label: 'Plans', icon: 'pi-sitemap' },
  { name: 'vesting-schedules', label: 'Vesting Schedules', icon: 'pi-calendar' },
  { name: 'termination-rules', label: 'Termination Rules', icon: 'pi-stopwatch' },
  { name: 'participants', label: 'Participants', icon: 'pi-users' },
  { name: 'grants', label: 'Grants', icon: 'pi-ticket' }
]

const optioneeItems = [
  { name: 'participant-stocks', label: 'Stock Summary', icon: 'pi-chart-bar' },
  { name: 'participant-options', label: 'Option Summary', icon: 'pi-chart-line' },
  { name: 'participant-grants', label: 'Grant Details', icon: 'pi-briefcase' }
]

const publicItems = [
  { name: 'home', label: 'Dashboard', icon: 'pi-home' },
  { name: 'about', label: 'About', icon: 'pi-info-circle' },
  { name: 'contact', label: 'Contact', icon: 'pi-envelope' },
  { name: 'privacy', label: 'Privacy', icon: 'pi-shield' }
]

function isActive(name: string): boolean {
  return route.name === name
}
</script>

<style scoped>
.sidebar-link {
  display: flex;
  align-items: center;
  gap: 0.85rem;
  border-radius: 1rem;
  padding: 0.85rem 1rem;
  color: rgba(226, 232, 240, 0.88);
  transition: background 160ms ease, color 160ms ease, transform 160ms ease;
}

.sidebar-link:hover {
  background: rgba(148, 163, 184, 0.13);
  color: white;
  transform: translateX(4px);
}

.sidebar-link-active {
  background: linear-gradient(135deg, rgba(217, 119, 6, 0.9), rgba(15, 118, 110, 0.85));
  color: white;
}
</style>
