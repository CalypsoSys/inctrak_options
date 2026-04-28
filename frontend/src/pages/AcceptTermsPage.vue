<template>
  <section class="mx-auto max-w-2xl">
    <article class="card-surface rounded-[2rem] p-8">
      <PageIntro eyebrow="Access" title="Accept terms" description="This legacy accept-terms flow has been retired in favor of Supabase-backed sign-in." />
      <div class="mt-8 space-y-5">
        <p class="rounded-2xl border border-[var(--app-border)] bg-white/60 px-4 py-4 text-sm leading-6 text-[var(--app-muted)]">
          Sign in through the main login page with your Supabase credentials. If your account still needs tenant access or terms updates, we will handle that in the new auth flow rather than this legacy token page.
        </p>
        <RouterLink :to="{ name: 'login' }">
          <Button label="Back to Login" />
        </RouterLink>
      </div>
    </article>
  </section>
</template>

<script setup lang="ts">
import Button from 'primevue/button'
import PageIntro from '@/components/PageIntro.vue'
</script>
