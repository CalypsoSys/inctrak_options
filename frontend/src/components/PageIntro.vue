<template>
  <div class="flex flex-col gap-4 md:flex-row md:items-end md:justify-between">
    <div class="max-w-3xl">
      <p class="text-xs font-bold uppercase tracking-[0.3em] text-[var(--app-accent)]">{{ eyebrow }}</p>
      <h1 class="mt-2 text-3xl font-black tracking-tight text-slate-900 md:text-4xl">{{ title }}</h1>
      <p v-if="description" class="mt-3 text-sm leading-6 text-[var(--app-muted)] md:text-base">{{ description }}</p>
    </div>
    <div v-if="$slots.actions" class="flex flex-wrap gap-3">
      <slot name="actions" />
    </div>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  eyebrow: string
  title: string
  description?: string
}>()
</script>
