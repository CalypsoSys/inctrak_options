<template>
  <section>
    <article class="card-surface rounded-[2rem] p-8">
      <PageIntro
        eyebrow="Access"
        title="Login"
        description="Sign in with your credentials. App access is granted after your tenant membership and role are resolved."
      />
      <div class="mt-6 rounded-[1.5rem] border border-amber-300 bg-amber-50 px-5 py-4">
        <h3 class="text-base font-bold text-amber-900">{{ accessGate.title }}</h3>
        <p class="mt-2 text-sm leading-6 text-amber-900/90">{{ accessGate.message }}</p>
      </div>
      <form class="mt-8 space-y-5" @submit.prevent="submitForm">
        <div class="grid gap-5 md:grid-cols-2">
          <div>
            <label class="field-label">Email Address</label>
            <input v-model="form.email" class="field-input" type="email" :disabled="accessGate.disabled" />
          </div>
          <div>
            <label class="field-label">Password</label>
            <Password v-model="form.password" fluid toggle-mask :feedback="false" :disabled="accessGate.disabled" />
          </div>
        </div>
        <div class="flex flex-wrap gap-3">
          <Button type="submit" :loading="isBusy" :disabled="accessGate.disabled" label="Login" />
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
      <h3 class="text-lg font-bold text-slate-900">Bringing a new company onboard?</h3>
      <p class="mt-4 text-sm leading-6 text-[var(--app-muted)]">
        IncTrak is built to give each company a clean workspace, a branded company URL, and a straightforward path into day-to-day equity administration.
      </p>
      <ul class="mt-5 space-y-3 text-sm leading-6 text-[var(--app-muted)]">
        <li>Launch a dedicated company workspace when the rollout opens back up.</li>
        <li>Set up the first administrator to manage plans, grants, and participants.</li>
        <li>Invite participants into a more polished equity experience from the start.</li>
      </ul>
      <div class="mt-8 rounded-[1.5rem] border border-[var(--app-border)] bg-white/70 p-5">
        <h4 class="text-base font-bold text-slate-900">Already invited as a participant?</h4>
        <p class="mt-3 text-sm leading-6 text-[var(--app-muted)]">
          Once your company grants you access, you will use the same email tied to your participant record to review grants, vesting, and ownership details here.
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
import { accessGate } from '@/services/access-gate'
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
  if (accessGate.disabled) {
    showMessage(accessGate.message, false)
    return
  }

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
