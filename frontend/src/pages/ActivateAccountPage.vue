<template>
  <section class="mx-auto max-w-2xl">
    <article class="card-surface rounded-[2rem] p-8">
      <PageIntro
        eyebrow="Access"
        title="Activate account"
        description="Finish the account activation flow that was sent by email."
      />
      <div class="mt-8 flex gap-3">
        <Button label="Activate Account" :loading="isBusy" @click="runActivation" />
        <RouterLink :to="{ name: 'login' }">
          <Button label="Back to Login" severity="secondary" variant="outlined" />
        </RouterLink>
      </div>
    </article>

    <AppDialog
      v-model:visible="dialogVisible"
      title="Account Activation"
      :message="message"
      :success="isSuccess"
    />
  </section>
</template>

<script setup lang="ts">
import { useRoute } from 'vue-router'
import Button from 'primevue/button'
import AppDialog from '@/components/AppDialog.vue'
import PageIntro from '@/components/PageIntro.vue'
import { useAsyncState } from '@/composables/useAsyncState'
import { activateAccount } from '@/services/auth-service'
import { getApiMessage } from '@/services/api'

const route = useRoute()
const key = String(route.params.key)
const { isBusy, dialogVisible, isSuccess, message, showMessage } = useAsyncState()

async function runActivation(): Promise<void> {
  isBusy.value = true
  try {
    const response = await activateAccount(key)
    showMessage(response.message ?? 'Activation request completed.', response.success !== false)
  } catch (error) {
    showMessage(getApiMessage(error, 'Unable to activate your account.'), false)
  } finally {
    isBusy.value = false
  }
}
</script>
