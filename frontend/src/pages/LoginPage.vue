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
            <input v-model="form.email" class="field-input" type="email" />
          </div>
          <div>
            <label class="field-label">Password</label>
            <Password v-model="form.password" fluid toggle-mask :feedback="false" />
          </div>
        </div>
        <div v-if="form.isRegistering">
          <div class="grid gap-5 md:grid-cols-2">
            <div>
              <label class="field-label">Company Name</label>
              <input v-model="form.companyName" class="field-input" type="text" />
            </div>
            <div>
              <label class="field-label">Company Slug</label>
              <input v-model="form.tenantSlug" class="field-input" type="text" autocapitalize="off" spellcheck="false" />
            </div>
          </div>
        </div>
        <div v-if="form.isRegistering">
          <label class="field-label">Confirm Password</label>
          <Password v-model="form.confirmPassword" fluid toggle-mask :feedback="false" />
        </div>
        <div class="grid gap-4 md:grid-cols-2">
          <label class="flex items-center gap-2 rounded-2xl border border-[var(--app-border)] bg-white/60 px-4 py-3 text-sm font-semibold text-[var(--app-muted)]">
            <input v-model="form.isRegistering" type="checkbox" />
            Register new account
          </label>
          <label v-if="form.isRegistering" class="flex items-center gap-2 rounded-2xl border border-[var(--app-border)] bg-white/60 px-4 py-3 text-sm font-semibold text-[var(--app-muted)]">
            <input v-model="form.acceptTerms" type="checkbox" />
            Accept terms
          </label>
        </div>
        <div class="flex flex-wrap gap-3">
          <Button type="submit" :loading="isBusy" :label="form.isRegistering ? 'Create Account' : 'Login'" />
        </div>
      </form>
    </article>

    <article class="card-surface rounded-[2rem] p-8">
      <h3 class="text-lg font-bold text-slate-900">What happens after login?</h3>
      <ul class="mt-5 space-y-3 text-sm leading-6 text-[var(--app-muted)]">
        <li>Administrators land in the new admin workspace with stock classes, plans, participants, grants, and schedule tools.</li>
        <li>Participants land in the optionee area with stock summary, option summary, and grant detail views.</li>
        <li>Registration now provisions the first company workspace and links the first tenant admin after Supabase auth succeeds.</li>
        <li>Participant access is controlled by linking their email to a participant record in the admin workspace.</li>
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
import axios from 'axios'
import { reactive } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Button from 'primevue/button'
import Password from 'primevue/password'
import AppDialog from '@/components/AppDialog.vue'
import PageIntro from '@/components/PageIntro.vue'
import { useAsyncState } from '@/composables/useAsyncState'
import { createLoginForm } from '@/services/auth-service'
import { fetchAppSession } from '@/services/auth-session'
import { getApiMessage } from '@/services/api'
import { provisionTenantSignup } from '@/services/tenant-signup'
import { signInWithPassword, signUpWithPassword } from '@/services/supabase-auth'
import { useAuthStore } from '@/stores/auth'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const { isBusy, dialogVisible, isSuccess, message, showMessage } = useAsyncState()
const form = reactive(createLoginForm())

async function submitForm(): Promise<void> {
  const email = form.email.trim()
  const companyName = form.companyName.trim()
  const tenantSlug = normalizeTenantSlug(form.tenantSlug)
  if (!email || !form.password.trim()) {
    showMessage('Please enter an email address and password.', false)
    return
  }

  if (form.isRegistering && !companyName) {
    showMessage('Please enter a company name.', false)
    return
  }

  if (form.isRegistering && !tenantSlug) {
    showMessage('Please enter a company slug.', false)
    return
  }

  if (form.isRegistering && !form.confirmPassword.trim()) {
    showMessage('Please confirm your password.', false)
    return
  }

  if (form.isRegistering && form.password !== form.confirmPassword) {
    showMessage('Passwords do not match.', false)
    return
  }

  if (form.isRegistering && !form.acceptTerms) {
    showMessage('Please accept the terms and conditions.', false)
    return
  }

  isBusy.value = true
  try {
    const session = form.isRegistering
      ? await signUpWithPassword(email, form.password, companyName, tenantSlug)
      : await signInWithPassword(email, form.password)

    if (form.isRegistering && !session.access_token) {
      showMessage('Account created. Check your email to confirm your account, then sign in to finish provisioning your company workspace.', true)
      form.password = ''
      form.confirmPassword = ''
      return
    }

    if (!session.access_token || !session.refresh_token || !session.expires_in) {
      throw new Error('Supabase did not return a usable session.')
    }

    const expiresAt = session.expires_at ?? Math.floor(Date.now() / 1000) + session.expires_in
    const resolvedCompanyName = companyName || session.user?.user_metadata?.company_name?.trim() || ''
    const resolvedTenantSlug = normalizeTenantSlug(tenantSlug || session.user?.user_metadata?.tenant_slug || '')
    const { appSession, tenant } = await ensureAppSession(session.access_token, resolvedCompanyName, resolvedTenantSlug)
    authStore.setSession(
      session.access_token,
      session.refresh_token,
      expiresAt,
      appSession.Role,
      session.user?.email ?? email,
      tenant?.TenantId ?? null,
      tenant?.TenantSlug ?? resolvedTenantSlug ?? null,
      tenant?.TenantDatabaseName ?? null
    )

    showMessage(
      form.isRegistering
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
        form.isRegistering
          ? 'Unable to create the Supabase account.'
          : 'Unable to complete the Supabase login request.'
      ),
      false
    )
  } finally {
    isBusy.value = false
  }
}

async function ensureAppSession(accessToken: string, companyName: string, tenantSlug: string) {
  try {
    return {
      appSession: await fetchAppSession(accessToken),
      tenant: null
    }
  } catch (error) {
    if (!axios.isAxiosError(error) || !error.response || (error.response.status !== 401 && error.response.status !== 403)) {
      throw error
    }

    if (!companyName || !tenantSlug) {
      throw error
    }

    const tenant = await provisionTenantSignup(companyName, tenantSlug)
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

function normalizeTenantSlug(value: string): string {
  return value
    .trim()
    .toLowerCase()
    .replace(/[^a-z0-9-]+/g, '-')
    .replace(/-{2,}/g, '-')
    .replace(/^-+|-+$/g, '')
    .slice(0, 63)
}
</script>
