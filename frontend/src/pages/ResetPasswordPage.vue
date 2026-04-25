<template>
  <section class="mx-auto max-w-2xl">
    <article class="card-surface rounded-[2rem] p-8">
      <PageIntro eyebrow="Access" title="Set a new password" description="Complete the reset flow and accept the current terms." />
      <div class="mt-8 space-y-5">
        <div>
          <label class="field-label">New Password</label>
          <Password v-model="password1" fluid toggle-mask :feedback="false" />
        </div>
        <div>
          <label class="field-label">Confirm Password</label>
          <Password v-model="password2" fluid toggle-mask :feedback="false" />
        </div>
        <label class="flex items-center gap-2 rounded-2xl border border-[var(--app-border)] bg-white/60 px-4 py-3 text-sm font-semibold text-[var(--app-muted)]">
          <input v-model="acceptTermsValue" type="checkbox" />
          I accept the terms and conditions
        </label>
        <Button label="Reset Password" :loading="isBusy" @click="submitReset" />
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
import { useRoute } from 'vue-router'
import Button from 'primevue/button'
import Password from 'primevue/password'
import AppDialog from '@/components/AppDialog.vue'
import PageIntro from '@/components/PageIntro.vue'
import { useAsyncState } from '@/composables/useAsyncState'
import { getApiMessage } from '@/services/api'
import { resetPassword } from '@/services/auth-service'

const route = useRoute()
const key = String(route.params.key)
const password1 = ref('')
const password2 = ref('')
const acceptTermsValue = ref(false)
const { isBusy, dialogVisible, isSuccess, message, showMessage } = useAsyncState()

async function submitReset(): Promise<void> {
  if (!acceptTermsValue.value) {
    showMessage('Please accept the terms and conditions.', false)
    return
  }

  if (!password1.value || !password2.value) {
    showMessage('Please enter and confirm your new password.', false)
    return
  }

  isBusy.value = true
  try {
    const response = await resetPassword(key, password1.value, password2.value, acceptTermsValue.value)
    showMessage(response.message ?? 'Password reset completed.', response.success !== false)
    if (response.success !== false) {
      password1.value = ''
      password2.value = ''
    }
  } catch (error) {
    showMessage(getApiMessage(error, 'Unable to reset your password.'), false)
  } finally {
    isBusy.value = false
  }
}
</script>
