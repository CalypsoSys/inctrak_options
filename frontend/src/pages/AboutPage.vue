<template>
  <section class="space-y-6">
    <PageIntro
      eyebrow="Product"
      title="A modern equity operations workspace"
      description="IncTrak keeps startup vesting, grants, and participant access in one API-backed dashboard instead of scattered spreadsheets and email threads."
    />
    <article class="card-surface rounded-[2rem] p-8 text-[15px] leading-7 text-[var(--app-muted)]">
      <p>IncTrak is designed for teams that need a dependable record of stock classes, plans, grants, vesting schedules, and participant visibility.</p>
      <p class="mt-4">The administrator experience focuses on configuration and data quality. The participant experience focuses on clarity: what was granted, what is vested, and what remains ahead.</p>
      <p class="mt-4">This frontend modernizes the older static portal into a maintainable Vue application without changing the backend responsibilities or the underlying equity workflows.</p>
    </article>
  </section>
</template>

<script setup lang="ts">
import PageIntro from '@/components/PageIntro.vue'
</script>
