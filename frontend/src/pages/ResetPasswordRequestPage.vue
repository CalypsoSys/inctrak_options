<template>
  <section class="mx-auto max-w-2xl">
    <article class="card-surface rounded-[2rem] p-8">
      <PageIntro eyebrow="Access" title="Reset password" description="Request a reset email by username or email address." />
      <div class="mt-8 space-y-4">
        <div>
          <label class="field-label">Username or Email</label>
          <input v-model="userNameEmail" class="field-input" type="text" />
        </div>
        <Button label="Send Reset Link" :loading="isBusy" @click="submitRequest" />
      </div>
    </article>

    <AppDialog
      v-model:visible="dialogVisible"
      title="Reset Password"
      :message="message"
      :success="isSuccess"
    />
  </section>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import Button from 'primevue/button'
import AppDialog from '@/components/AppDialog.vue'
import PageIntro from '@/components/PageIntro.vue'
import { useAsyncState } from '@/composables/useAsyncState'
import { requestPasswordReset } from '@/services/auth-service'
import { getApiMessage } from '@/services/api'

const userNameEmail = ref('')
const { isBusy, dialogVisible, isSuccess, message, showMessage } = useAsyncState()

async function submitRequest(): Promise<void> {
  if (!userNameEmail.value.trim()) {
    showMessage('Please enter a username or email address.', false)
    return
  }

  isBusy.value = true
  try {
    const response = await requestPasswordReset(userNameEmail.value)
    showMessage(response.message ?? 'Password reset request completed.', response.success !== false)
    if (response.success !== false) {
      userNameEmail.value = ''
    }
  } catch (error) {
    showMessage(getApiMessage(error, 'Unable to request a password reset.'), false)
  } finally {
    isBusy.value = false
  }
}
</script>
