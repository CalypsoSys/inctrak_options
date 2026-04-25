<template>
  <article class="card-surface rounded-3xl p-5">
    <p class="text-xs font-bold uppercase tracking-[0.24em] text-[var(--app-muted)]">{{ label }}</p>
    <p class="mt-3 text-3xl font-black text-slate-900">{{ value }}</p>
    <p v-if="hint" class="mt-2 text-sm text-[var(--app-muted)]">{{ hint }}</p>
  </article>
</template>

<script setup lang="ts">
defineProps<{
  label: string
  value: string
  hint?: string
}>()
</script>
