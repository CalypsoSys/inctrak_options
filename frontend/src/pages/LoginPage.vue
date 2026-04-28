<template>
  <section class="grid gap-6 xl:grid-cols-[1.2fr_0.8fr]">
    <article class="card-surface rounded-[2rem] p-8">
      <PageIntro
        eyebrow="Access"
        title="Login or register"
        description="Sign in with Supabase-backed credentials. App access is granted after your tenant membership and role are resolved."
      />
      <form class="mt-8 space-y-5" @submit.prevent="submitForm">
        <div class="grid gap-5 md:grid-cols-2">
          <div>
            <label class="field-label">Email Address</label>
            <input v-model="form.USER_NAME" class="field-input" type="text" />
          </div>
          <div>
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
        <div v-if="form.IS_REGISTERING">
          <label class="field-label">Confirm Password</label>
          <Password v-model="form.PASSWORD2" fluid toggle-mask :feedback="false" />
        </div>
        <div class="grid gap-4 md:grid-cols-2">
          <label class="flex items-center gap-2 rounded-2xl border border-[var(--app-border)] bg-white/60 px-4 py-3 text-sm font-semibold text-[var(--app-muted)]">
            <input v-model="form.IS_REGISTERING" type="checkbox" />
            Register new account
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
        <li>Registration creates Supabase credentials first; actual app access still depends on an IncTrak tenant membership.</li>
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
import { createLoginForm, fetchLoginDefaults } from '@/services/auth-service'
import { fetchAppSession } from '@/services/auth-session'
import { getApiMessage } from '@/services/api'
import { signInWithPassword, signUpWithPassword } from '@/services/supabase-auth'
import { useAuthStore } from '@/stores/auth'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const { isBusy, dialogVisible, isSuccess, message, showMessage } = useAsyncState()
const form = reactive(createLoginForm())

onMounted(async () => {
  Object.assign(form, await fetchLoginDefaults())
})

async function submitForm(): Promise<void> {
  const email = form.USER_NAME.trim()
  if (!email || !form.PASSWORD.trim()) {
    showMessage('Please enter an email address and password.', false)
    return
  }

  if (form.IS_REGISTERING && !form.ACCEPT_TERMS) {
    showMessage('Please accept the terms and conditions.', false)
    return
  }

  isBusy.value = true
  try {
    const session = form.IS_REGISTERING
      ? await signUpWithPassword(email, form.PASSWORD)
      : await signInWithPassword(email, form.PASSWORD)

    const expiresAt = session.expires_at ?? Math.floor(Date.now() / 1000) + session.expires_in
    const appSession = await fetchAppSession(session.access_token)
    authStore.setSession(
      session.access_token,
      session.refresh_token,
      expiresAt,
      appSession.Role,
      session.user?.email ?? email
    )

    showMessage(
      form.IS_REGISTERING
        ? 'Account created and signed in.'
        : 'Login successful.',
      true
    )

    if (appSession.Role) {
      await router.push(
        route.query.next && typeof route.query.next === 'string'
          ? route.query.next
          : appSession.Role === 'admin'
            ? { name: 'stock-classes' }
            : { name: 'participant-stocks' }
      )
    }
  } catch (error) {
    showMessage(
      getApiMessage(
        error,
        form.IS_REGISTERING
          ? 'Unable to create the Supabase account.'
          : 'Unable to complete the Supabase login request.'
      ),
      false
    )
  } finally {
    isBusy.value = false
  }
}
</script>
