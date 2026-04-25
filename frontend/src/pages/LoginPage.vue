<template>
  <section class="grid gap-6 xl:grid-cols-[1.2fr_0.8fr]">
    <article class="card-surface rounded-[2rem] p-8">
      <PageIntro
        eyebrow="Access"
        title="Login or register"
        description="Use internal credentials or Google sign-in. The same API endpoints remain in place; this page just modernizes the flow."
      />
      <form class="mt-8 space-y-5" @submit.prevent="submitForm">
        <div class="grid gap-5 md:grid-cols-2">
          <div>
            <label class="field-label">Username or Email</label>
            <input v-model="form.USER_NAME" class="field-input" :disabled="form.GOOGLE_LOGON" type="text" />
          </div>
          <div v-if="!form.GOOGLE_LOGON">
            <label class="field-label">Password</label>
            <Password v-model="form.PASSWORD" fluid toggle-mask :feedback="false" />
          </div>
        </div>
        <div v-if="form.IS_REGISTERING" class="grid gap-5 md:grid-cols-2">
          <div>
            <label class="field-label">Email Address</label>
            <input v-model="form.EMAIL_ADDRESS" class="field-input" type="email" />
          </div>
          <div>
            <label class="field-label">Company / Group Name</label>
            <input v-model="form.GROUP_NAME" class="field-input" type="text" />
          </div>
        </div>
        <div v-if="form.IS_REGISTERING && !form.GOOGLE_LOGON">
          <label class="field-label">Confirm Password</label>
          <Password v-model="form.PASSWORD2" fluid toggle-mask :feedback="false" />
        </div>
        <div class="grid gap-4 md:grid-cols-3">
          <label class="flex items-center gap-2 rounded-2xl border border-[var(--app-border)] bg-white/60 px-4 py-3 text-sm font-semibold text-[var(--app-muted)]">
            <input v-model="form.IS_REGISTERING" type="checkbox" />
            Register new account
          </label>
          <label class="flex items-center gap-2 rounded-2xl border border-[var(--app-border)] bg-white/60 px-4 py-3 text-sm font-semibold text-[var(--app-muted)]">
            <input v-model="form.GOOGLE_LOGON" type="checkbox" />
            Use Google
          </label>
          <label class="flex items-center gap-2 rounded-2xl border border-[var(--app-border)] bg-white/60 px-4 py-3 text-sm font-semibold text-[var(--app-muted)]">
            <input v-model="form.ACCEPT_TERMS" type="checkbox" />
            Accept terms
          </label>
        </div>
        <div class="flex flex-wrap gap-3">
          <Button type="submit" :loading="isBusy" :label="form.IS_REGISTERING ? 'Create Account' : 'Login'" />
          <RouterLink :to="{ name: 'reset-password-request' }">
            <Button label="Reset Password" severity="secondary" variant="outlined" />
          </RouterLink>
        </div>
      </form>
    </article>

    <article class="card-surface rounded-[2rem] p-8">
      <h3 class="text-lg font-bold text-slate-900">What happens after login?</h3>
      <ul class="mt-5 space-y-3 text-sm leading-6 text-[var(--app-muted)]">
        <li>Administrators land in the new admin workspace with stock classes, plans, participants, grants, and schedule tools.</li>
        <li>Participants land in the optionee area with stock summary, option summary, and grant detail views.</li>
        <li>Google callback redirects and email links now target the renamed route model.</li>
      </ul>
    </article>

    <AppDialog
      v-model:visible="dialogVisible"
      title="Access"
      :message="message"
      :success="isSuccess"
    />
  </section>
</template>

<script setup lang="ts">
import { onMounted, reactive } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Button from 'primevue/button'
import Password from 'primevue/password'
import AppDialog from '@/components/AppDialog.vue'
import PageIntro from '@/components/PageIntro.vue'
import { useAsyncState } from '@/composables/useAsyncState'
import { createLoginForm, fetchLoginDefaults, submitLogin } from '@/services/auth-service'
import { getApiMessage } from '@/services/api'
import { useAuthStore } from '@/stores/auth'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const { isBusy, dialogVisible, isSuccess, message, showMessage } = useAsyncState()
const form = reactive(createLoginForm())

onMounted(async () => {
  Object.assign(form, await fetchLoginDefaults())

  if (route.query.redirect === 'true') {
    const uuid = typeof route.query.uuid === 'string' ? route.query.uuid : ''
    const role = typeof route.query.role === 'string' ? route.query.role : null
    const success = route.query.success === 'true'
    const nextMessage = typeof route.query.message === 'string' ? route.query.message : 'Login response received.'

    if (success && uuid && (role === 'admin' || role === 'optionee')) {
      authStore.setSession(uuid, role)
    }

    showMessage(nextMessage, success)
  }
})

async function submitForm(): Promise<void> {
  if (!form.GOOGLE_LOGON && (!form.USER_NAME.trim() || !form.PASSWORD.trim())) {
    showMessage('Please enter a username or email and password.', false)
    return
  }

  if (form.IS_REGISTERING && !form.ACCEPT_TERMS) {
    showMessage('Please accept the terms and conditions.', false)
    return
  }

  isBusy.value = true
  try {
    const response = await submitLogin(form)
    if (response.google_redirect) {
      window.location.href = response.google_redirect
      return
    }

    showMessage(response.message ?? 'Login response received.', response.success !== false)
    if (response.success && response.uuid && response.Role) {
      authStore.setSession(response.uuid, response.Role)
      await router.push(
        route.query.next && typeof route.query.next === 'string'
          ? route.query.next
          : response.Role === 'admin'
            ? { name: 'stock-classes' }
            : { name: 'participant-stocks' }
      )
    }
  } catch (error) {
    showMessage(getApiMessage(error, 'Unable to complete the login request.'), false)
  } finally {
    isBusy.value = false
  }
}
</script>
