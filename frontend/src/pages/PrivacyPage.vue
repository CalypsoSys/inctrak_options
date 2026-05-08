<template>
  <section class="space-y-6">
    <PageIntro
      eyebrow="Legal"
      title="Privacy Policy"
      description="This page preserves the existing privacy commitments while presenting them in a cleaner SPA layout."
    />
    <article class="card-surface rounded-[2rem] p-8 text-sm leading-7 text-[var(--app-muted)]">
      <h3 class="text-xl font-bold text-slate-900">Privacy and data handling</h3>
      <p class="mt-4">IncTrak collects only the information needed to operate account access, participant records, grants, and support workflows.</p>
      <p class="mt-4">Usage data, browser details, and IP addresses may be logged for diagnostics, abuse prevention, and support. Personally identifiable information is handled only where necessary to operate the service.</p>
      <p class="mt-4">Questions about privacy handling should be sent through the contact form or to <a class="font-semibold text-[var(--app-accent)]" href="mailto:contact@inctrak.com">contact@inctrak.com</a>.</p>
    </article>
  </section>
</template>

<script setup lang="ts">
import PageIntro from '@/components/PageIntro.vue'
</script>
