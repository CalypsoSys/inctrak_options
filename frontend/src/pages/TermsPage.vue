<template>
  <section class="space-y-6">
    <PageIntro
      eyebrow="Legal"
      title="Terms of Service"
      description="These terms describe the expected use of the IncTrak workspace during this rollout phase."
    />
    <article class="card-surface rounded-[2rem] p-8 text-sm leading-7 text-[var(--app-muted)]">
      <h3 class="text-xl font-bold text-slate-900">Use of the service</h3>
      <p class="mt-4">IncTrak is provided for managing equity plans, participants, grants, and related administrative records. You agree to use the workspace only for lawful business purposes and only for companies and records you are authorized to manage.</p>
      <p class="mt-4">Account holders are responsible for protecting their sign-in credentials, reviewing participant and grant data for accuracy, and notifying IncTrak if they believe their account or tenant workspace has been accessed improperly.</p>
      <p class="mt-4">During this transition to the new multi-tenant platform, features may evolve. Continued use of the service means you accept operational updates that improve security, tenant isolation, and authentication handling.</p>
      <p class="mt-4">Questions about these terms should be sent through the contact form or to <a class="font-semibold text-[var(--app-accent)]" href="mailto:contact@inctrak.com">contact@inctrak.com</a>.</p>
    </article>
  </section>
</template>

<script setup lang="ts">
import PageIntro from '@/components/PageIntro.vue'
</script>
