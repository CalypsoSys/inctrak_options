<template>
  <section class="mx-auto max-w-2xl">
    <article class="card-surface rounded-[2rem] p-8">
      <PageIntro eyebrow="Access" title="Accept terms" description="Complete the Google or invite-driven sign-in flow by accepting the current terms." />
      <div class="mt-8 space-y-5">
        <label class="flex items-center gap-2 rounded-2xl border border-[var(--app-border)] bg-white/60 px-4 py-3 text-sm font-semibold text-[var(--app-muted)]">
          <input v-model="accepted" type="checkbox" />
          I accept the terms and conditions
        </label>
        <Button label="Continue" :loading="isBusy" @click="submitTerms" />
      </div>
    </article>

    <AppDialog
      v-model:visible="dialogVisible"
      title="Terms"
      :message="message"
      :success="isSuccess"
    />
  </section>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Button from 'primevue/button'
import AppDialog from '@/components/AppDialog.vue'
import PageIntro from '@/components/PageIntro.vue'
import { useAsyncState } from '@/composables/useAsyncState'
import { acceptTerms } from '@/services/auth-service'
import { getApiMessage } from '@/services/api'
import { useAuthStore } from '@/stores/auth'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const accepted = ref(false)
const key = String(route.params.key)
const { isBusy, dialogVisible, isSuccess, message, showMessage } = useAsyncState()

async function submitTerms(): Promise<void> {
  if (!accepted.value) {
    showMessage('Please accept the terms and conditions.', false)
    return
  }

  isBusy.value = true
  try {
    const response = await acceptTerms(key, accepted.value)
    showMessage(response.message ?? 'Terms accepted.', response.success !== false)
    if (response.success && response.uuid && response.Role) {
      authStore.setSession(response.uuid, response.Role)
      await router.push(response.Role === 'admin' ? { name: 'stock-classes' } : { name: 'participant-stocks' })
    }
  } catch (error) {
    showMessage(getApiMessage(error, 'Unable to accept the terms.'), false)
  } finally {
    isBusy.value = false
  }
}
</script>
