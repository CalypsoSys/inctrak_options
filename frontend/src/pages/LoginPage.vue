<template>
  <section class="grid gap-6 xl:grid-cols-[1.05fr_0.95fr]">
    <article class="card-surface rounded-[2rem] p-8">
      <PageIntro
        eyebrow="Access"
        title="Login"
        description="Sign in with your Supabase-backed credentials. App access is granted after your tenant membership and role are resolved."
      />
      <form class="mt-8 space-y-5" @submit.prevent="submitForm">
        <div class="grid gap-5 md:grid-cols-2">
          <div>
            <label class="field-label">Email Address</label>
            <input v-model="form.email" class="field-input" type="email" />
          </div>
          <div>
            <label class="field-label">Password</label>
            <Password v-model="form.password" fluid toggle-mask :feedback="false" />
          </div>
        </div>
        <div class="flex flex-wrap gap-3">
          <Button type="submit" :loading="isBusy" label="Login" />
          <a
            class="inline-flex items-center justify-center rounded-2xl border border-[var(--app-border)] bg-white/60 px-4 py-3 text-sm font-semibold text-[var(--app-muted)] transition hover:border-[var(--app-accent)] hover:text-[var(--app-accent)]"
            :href="signupAppUrl"
          >
            Need A New Workspace?
          </a>
        </div>
      </form>
    </article>

    <article class="card-surface rounded-[2rem] p-8">
      <h3 class="text-lg font-bold text-slate-900">New company?</h3>
      <p class="mt-4 text-sm leading-6 text-[var(--app-muted)]">
        Workspace creation now lives in a separate public signup app. That keeps the main shared frontend focused on login and day-to-day work, while the signup flow stays tailored to first-time company setup.
      </p>
      <ul class="mt-5 space-y-3 text-sm leading-6 text-[var(--app-muted)]">
        <li>Create the first company workspace and tenant admin in the public signup flow.</li>
        <li>Confirm your email if Supabase requires it.</li>
        <li>Return here to log in and land in the right admin or participant experience.</li>
      </ul>
      <div class="mt-8 rounded-[1.5rem] border border-[var(--app-border)] bg-white/70 p-5">
        <h4 class="text-base font-bold text-slate-900">Already invited as a participant?</h4>
        <p class="mt-3 text-sm leading-6 text-[var(--app-muted)]">
          Use the same email your administrator linked to your participant record, then log in here directly. You do not need to create a separate workspace.
        </p>
      </div>
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
import axios from 'axios'
import { reactive } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Button from 'primevue/button'
import Password from 'primevue/password'
import AppDialog from '@/components/AppDialog.vue'
import PageIntro from '@/components/PageIntro.vue'
import { useAsyncState } from '@/composables/useAsyncState'
import { createLoginForm, resolvePendingProvisioningMetadata } from '@/services/auth-service'
import { fetchAppSession } from '@/services/auth-session'
import { getApiMessage } from '@/services/api'
import { buildSignupAppUrl } from '@/services/runtime-config'
import { signInWithPassword } from '@/services/supabase-auth'
import { provisionTenantSignup } from '@/services/tenant-signup'
import { useAuthStore } from '@/stores/auth'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const signupAppUrl = buildSignupAppUrl()
const { isBusy, dialogVisible, isSuccess, message, showMessage } = useAsyncState()
const form = reactive(createLoginForm())

async function submitForm(): Promise<void> {
  const email = form.email.trim()
  if (!email || !form.password.trim()) {
    showMessage('Please enter an email address and password.', false)
    return
  }

  isBusy.value = true
  try {
    const session = await signInWithPassword(email, form.password)

    if (!session.access_token || !session.refresh_token || !session.expires_in) {
      throw new Error('Supabase did not return a usable session.')
    }

    const expiresAt = session.expires_at ?? Math.floor(Date.now() / 1000) + session.expires_in
    const { appSession, tenant } = await ensureAppSession(session.access_token, session.user?.user_metadata)
    authStore.setSession(
      session.access_token,
      session.refresh_token,
      expiresAt,
      appSession.Role,
      session.user?.email ?? email,
      tenant?.TenantId ?? null,
      tenant?.TenantSlug ?? null,
      tenant?.TenantDatabaseName ?? null
    )

    showMessage('Login successful.', true)

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
    showMessage(getApiMessage(error, 'Unable to complete the Supabase login request.'), false)
  } finally {
    isBusy.value = false
  }
}

async function ensureAppSession(
  accessToken: string,
  userMetadata?: {
    company_name?: string
    tenant_slug?: string
  }
) {
  try {
    return {
      appSession: await fetchAppSession(accessToken),
      tenant: null
    }
  } catch (error) {
    if (!axios.isAxiosError(error) || !error.response || (error.response.status !== 401 && error.response.status !== 403)) {
      throw error
    }

    const metadata = resolvePendingProvisioningMetadata(userMetadata)
    if (!metadata) {
      throw error
    }

    const tenant = await provisionTenantSignup(metadata.companyName, metadata.tenantSlug, accessToken)
    return {
      appSession: await fetchAppSession(accessToken, {
        tenantId: tenant.TenantId,
        tenantSlug: tenant.TenantSlug,
        tenantDatabaseName: tenant.TenantDatabaseName
      }),
      tenant
    }
  }
}
</script>
