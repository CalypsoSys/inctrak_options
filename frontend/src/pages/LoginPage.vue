<template>
  <section class="space-y-6">
    <article class="card-surface rounded-[2rem] p-8">
      <PageIntro
        eyebrow="Access"
        title="Sign in"
        description="Use your IncTrak account credentials or create credentials for an invited email."
      />
      <div class="mt-6 rounded-[1.5rem] border border-amber-300 bg-amber-50 px-5 py-4">
        <h3 class="text-base font-bold text-amber-900">{{ accessGate.title }}</h3>
        <p class="mt-2 text-sm leading-6 text-amber-900/90">{{ accessGate.message }}</p>
      </div>

      <form v-if="pendingRecoverySession" class="mt-8 space-y-5" @submit.prevent="submitPasswordReset">
        <div class="grid gap-5 md:grid-cols-2">
          <div>
            <label class="field-label">New Password</label>
            <Password v-model="form.password" fluid toggle-mask :feedback="false" :disabled="isActionDisabled" />
          </div>
          <div>
            <label class="field-label">Confirm Password</label>
            <Password v-model="form.confirmPassword" fluid toggle-mask :feedback="false" :disabled="isActionDisabled" />
          </div>
        </div>

        <div class="flex flex-wrap gap-3">
          <Button
            type="submit"
            :loading="activeAction === 'password-reset'"
            :disabled="isActionDisabled"
            icon="pi pi-key"
            label="Update password"
          />
          <Button
            type="button"
            severity="secondary"
            variant="outlined"
            :disabled="isActionDisabled"
            label="Back to sign in"
            @click="cancelPasswordReset"
          />
        </div>
      </form>

      <template v-else>
        <div class="mt-8 grid rounded-2xl border border-[var(--app-border)] bg-white/60 p-1 sm:grid-cols-2" role="tablist" aria-label="Account access">
          <button
            type="button"
            role="tab"
            :aria-selected="activeTab === 'sign-in'"
            :class="tabClass('sign-in')"
            @click="activeTab = 'sign-in'"
          >
            Sign in
          </button>
          <button
            type="button"
            role="tab"
            :aria-selected="activeTab === 'create-account'"
            :class="tabClass('create-account')"
            @click="activeTab = 'create-account'"
          >
            Create account
          </button>
        </div>

        <form v-if="activeTab === 'sign-in'" class="mt-8 space-y-5" @submit.prevent="submitSignIn">
          <div>
            <label class="field-label">Email</label>
            <input v-model="form.email" class="field-input" type="email" :disabled="isActionDisabled" autocomplete="email" />
          </div>
          <div>
            <label class="field-label">Password</label>
            <Password v-model="form.password" fluid toggle-mask :feedback="false" :disabled="isActionDisabled" />
          </div>
          <div class="flex flex-wrap items-center justify-between gap-3">
            <Button
              type="submit"
              :loading="activeAction === 'sign-in'"
              :disabled="isActionDisabled"
              icon="pi pi-arrow-right"
              icon-pos="right"
              label="Sign in"
            />
            <button
              type="button"
              class="text-sm font-semibold text-[var(--app-accent)] transition hover:text-[var(--app-accent-strong)] disabled:cursor-not-allowed disabled:text-[var(--app-muted)]"
              :disabled="isActionDisabled"
              @click="submitPasswordRecovery"
            >
              Forgot password?
            </button>
          </div>
        </form>

        <form v-else class="mt-8 space-y-5" @submit.prevent="submitCreateAccount">
          <div>
            <label class="field-label">Email</label>
            <input v-model="form.email" class="field-input" type="email" :disabled="isActionDisabled" autocomplete="email" />
          </div>
          <div class="grid gap-5 md:grid-cols-2">
            <div>
              <label class="field-label">Password</label>
              <Password v-model="form.password" fluid toggle-mask :feedback="false" :disabled="isActionDisabled" />
            </div>
            <div>
              <label class="field-label">Confirm Password</label>
              <Password v-model="form.confirmPassword" fluid toggle-mask :feedback="false" :disabled="isActionDisabled" />
            </div>
          </div>
          <div class="flex flex-wrap gap-3">
            <Button
              type="submit"
              :loading="activeAction === 'create-account'"
              :disabled="isActionDisabled"
              icon="pi pi-user-plus"
              label="Create account"
            />
          </div>
        </form>

        <div class="mt-6 flex flex-wrap justify-end gap-3 rounded-[1.25rem] border border-[var(--app-border)] bg-white/60 px-4 py-3">
          <Button
            type="button"
            severity="secondary"
            variant="outlined"
            icon="pi pi-envelope"
            :loading="activeAction === 'magic-link'"
            :disabled="isActionDisabled"
            label="Email me a magic link instead"
            @click="submitMagicLink"
          />
        </div>
      </template>
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
      <div class="mt-6 flex flex-wrap gap-3">
        <a
          class="inline-flex items-center justify-center rounded-2xl border border-[var(--app-border)] bg-white/60 px-4 py-3 text-sm font-semibold text-[var(--app-muted)] transition hover:border-[var(--app-accent)] hover:text-[var(--app-accent)]"
          :href="signupAppUrl"
        >
          Need A New Workspace?
        </a>
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
import { computed, onMounted, reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Button from 'primevue/button'
import Password from 'primevue/password'
import AppDialog from '@/components/AppDialog.vue'
import PageIntro from '@/components/PageIntro.vue'
import { useAsyncState } from '@/composables/useAsyncState'
import { accessGate } from '@/services/access-gate'
import {
  createLoginForm,
  normalizeAuthEmail,
  resolvePendingProvisioningMetadata,
  validateAccountCreationForm,
  validateEmailAction,
  validatePasswordResetForm,
  validateSignInForm
} from '@/services/auth-service'
import { fetchAppSession } from '@/services/auth-session'
import { getApiMessage } from '@/services/api'
import { buildSignupAppUrl } from '@/services/runtime-config'
import {
  sendMagicLink,
  sendPasswordRecoveryEmail,
  signInWithPassword,
  signUpForAccount,
  updateSupabasePassword,
  type SupabaseSession
} from '@/services/supabase-auth'
import { provisionTenantSignup } from '@/services/tenant-signup'
import { useAuthStore } from '@/stores/auth'

type AuthTab = 'sign-in' | 'create-account'
type AuthAction = 'sign-in' | 'create-account' | 'magic-link' | 'password-recovery' | 'password-reset'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const signupAppUrl = buildSignupAppUrl()
const { isBusy, dialogVisible, isSuccess, message, showMessage } = useAsyncState()
const form = reactive(createLoginForm())
const activeTab = ref<AuthTab>('sign-in')
const activeAction = ref<AuthAction | null>(null)
const pendingRecoverySession = ref<SupabaseSession | null>(null)
const isActionDisabled = computed(() => accessGate.disabled || isBusy.value)

onMounted(() => {
  void completeRedirectedSupabaseSession()
})

function tabClass(tab: AuthTab): string[] {
  const isActive = activeTab.value === tab
  return [
    'rounded-[1rem] px-4 py-3 text-sm font-bold transition',
    isActive
      ? 'bg-[var(--app-accent)] text-white shadow-sm'
      : 'text-[var(--app-muted)] hover:bg-white hover:text-[var(--app-accent)]'
  ]
}

async function submitSignIn(): Promise<void> {
  if (accessGate.disabled) {
    showMessage(accessGate.message, false)
    return
  }

  const validationMessage = validateSignInForm(form.email, form.password)
  if (validationMessage) {
    showMessage(validationMessage, false)
    return
  }

  const email = normalizeAuthEmail(form.email)
  beginAction('sign-in')
  try {
    const session = await signInWithPassword(email, form.password)
    const role = await applySupabaseSession(session, email, true)
    showMessage('Login successful.', true)
    await navigateForRole(role)
  } catch (error) {
    showMessage(getApiMessage(error, 'Unable to complete the Supabase login request.'), false)
  } finally {
    finishAction()
  }
}

async function submitCreateAccount(): Promise<void> {
  if (accessGate.disabled) {
    showMessage(accessGate.message, false)
    return
  }

  const validationMessage = validateAccountCreationForm(form.email, form.password, form.confirmPassword)
  if (validationMessage) {
    showMessage(validationMessage, false)
    return
  }

  const email = normalizeAuthEmail(form.email)
  beginAction('create-account')
  try {
    const session = await signUpForAccount(email, form.password, getAuthRedirectUrl())
    form.password = ''
    form.confirmPassword = ''

    if (!session.access_token) {
      activeTab.value = 'sign-in'
      showMessage('Account created. Check your email to confirm it, then sign in.', true)
      return
    }

    try {
      const role = await applySupabaseSession(session, email, false)
      showMessage('Account created.', true)
      await navigateForRole(role)
    } catch (error) {
      if (isAccessResolutionError(error)) {
        activeTab.value = 'sign-in'
        showMessage('Account created. Ask your workspace admin to grant IncTrak access for this email.', true)
        return
      }

      throw error
    }
  } catch (error) {
    showMessage(getApiMessage(error, 'Unable to create the Supabase account.'), false)
  } finally {
    finishAction()
  }
}

async function submitMagicLink(): Promise<void> {
  if (accessGate.disabled) {
    showMessage(accessGate.message, false)
    return
  }

  const validationMessage = validateEmailAction(form.email)
  if (validationMessage) {
    showMessage(validationMessage, false)
    return
  }

  beginAction('magic-link')
  try {
    await sendMagicLink(normalizeAuthEmail(form.email), getAuthRedirectUrl())
    showMessage('If this email can access IncTrak, a magic link is on the way.', true)
  } catch (error) {
    showMessage(getApiMessage(error, 'Unable to email a magic link.'), false)
  } finally {
    finishAction()
  }
}

async function submitPasswordRecovery(): Promise<void> {
  if (accessGate.disabled) {
    showMessage(accessGate.message, false)
    return
  }

  const validationMessage = validateEmailAction(form.email)
  if (validationMessage) {
    showMessage(validationMessage, false)
    return
  }

  beginAction('password-recovery')
  try {
    await sendPasswordRecoveryEmail(normalizeAuthEmail(form.email), getAuthRedirectUrl())
    showMessage('If this email has an IncTrak account, a password reset link is on the way.', true)
  } catch (error) {
    showMessage(getApiMessage(error, 'Unable to email a password reset link.'), false)
  } finally {
    finishAction()
  }
}

async function submitPasswordReset(): Promise<void> {
  if (accessGate.disabled) {
    showMessage(accessGate.message, false)
    return
  }

  const session = pendingRecoverySession.value
  if (!session?.access_token) {
    showMessage('This password recovery link is missing a usable session.', false)
    pendingRecoverySession.value = null
    return
  }

  const validationMessage = validatePasswordResetForm(form.password, form.confirmPassword)
  if (validationMessage) {
    showMessage(validationMessage, false)
    return
  }

  beginAction('password-reset')
  try {
    await updateSupabasePassword(session.access_token, form.password)
    form.password = ''
    form.confirmPassword = ''
    pendingRecoverySession.value = null

    try {
      const role = await applySupabaseSession(session, normalizeAuthEmail(form.email) || null, false)
      showMessage('Password updated.', true)
      await navigateForRole(role)
    } catch (error) {
      if (isAccessResolutionError(error)) {
        activeTab.value = 'sign-in'
        showMessage('Password updated. Ask your workspace admin to grant IncTrak access for this email.', true)
        return
      }

      throw error
    }
  } catch (error) {
    showMessage(getApiMessage(error, 'Unable to update the password.'), false)
  } finally {
    finishAction()
  }
}

async function ensureAppSession(
  accessToken: string,
  userMetadata?: {
    company_name?: string
    tenant_slug?: string
  },
  allowPendingProvisioning = true
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

    if (!allowPendingProvisioning) {
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

async function applySupabaseSession(
  session: SupabaseSession,
  fallbackEmail: string | null,
  allowPendingProvisioning: boolean
): Promise<'admin' | 'optionee'> {
  if (!session.access_token || !session.refresh_token || !session.expires_in) {
    throw new Error('Supabase did not return a usable session.')
  }

  const expiresAt = session.expires_at ?? Math.floor(Date.now() / 1000) + session.expires_in
  const { appSession, tenant } = await ensureAppSession(
    session.access_token,
    session.user?.user_metadata,
    allowPendingProvisioning
  )
  authStore.setSession(
    session.access_token,
    session.refresh_token,
    expiresAt,
    appSession.Role,
    session.user?.email ?? fallbackEmail,
    tenant?.TenantId ?? null,
    tenant?.TenantSlug ?? null,
    tenant?.TenantDatabaseName ?? null
  )

  return appSession.Role
}

async function navigateForRole(role: 'admin' | 'optionee'): Promise<void> {
  await router.push(
    route.query.next && typeof route.query.next === 'string'
      ? route.query.next
      : role === 'admin'
        ? { name: 'stock-classes' }
        : { name: 'participant-stocks' }
  )
}

function beginAction(action: AuthAction): void {
  activeAction.value = action
  isBusy.value = true
}

function finishAction(): void {
  activeAction.value = null
  isBusy.value = false
}

function cancelPasswordReset(): void {
  pendingRecoverySession.value = null
  form.password = ''
  form.confirmPassword = ''
  activeTab.value = 'sign-in'
}

function isAccessResolutionError(error: unknown): boolean {
  if (!axios.isAxiosError(error) || !error.response) {
    return false
  }

  return error.response.status === 401 || error.response.status === 403
}

function getAuthRedirectUrl(): string {
  return `${window.location.origin}${window.location.pathname}#/auth/login`
}

function readSupabaseRedirectParams(): URLSearchParams | null {
  for (const source of [window.location.hash, window.location.search]) {
    const accessTokenIndex = source.indexOf('access_token=')
    const errorIndex = source.indexOf('error=')
    const startIndex = accessTokenIndex >= 0 ? accessTokenIndex : errorIndex
    if (startIndex >= 0) {
      return new URLSearchParams(source.slice(startIndex))
    }
  }

  return null
}

function readSupabaseRedirectSession(params: URLSearchParams): SupabaseSession | null {
  const accessToken = params.get('access_token')
  const refreshToken = params.get('refresh_token')
  const expiresIn = Number(params.get('expires_in') ?? '')
  const expiresAt = Number(params.get('expires_at') ?? '')

  if (!accessToken || !refreshToken) {
    return null
  }

  return {
    access_token: accessToken,
    refresh_token: refreshToken,
    expires_in: Number.isFinite(expiresIn) && expiresIn > 0 ? expiresIn : undefined,
    expires_at: Number.isFinite(expiresAt) && expiresAt > 0 ? expiresAt : undefined,
    type: params.get('type') ?? undefined
  }
}

async function completeRedirectedSupabaseSession(): Promise<void> {
  const params = readSupabaseRedirectParams()
  if (!params) {
    return
  }

  clearSupabaseRedirectParams()

  const errorDescription = params.get('error_description') || params.get('error')
  if (errorDescription) {
    showMessage(errorDescription, false)
    return
  }

  const session = readSupabaseRedirectSession(params)
  if (!session) {
    return
  }

  if (accessGate.disabled) {
    showMessage(accessGate.message, false)
    return
  }

  if (session.type === 'recovery') {
    pendingRecoverySession.value = session
    form.password = ''
    form.confirmPassword = ''
    showMessage('Enter a new password to finish resetting your account.', true)
    return
  }

  beginAction('magic-link')
  try {
    const role = await applySupabaseSession(session, null, true)
    showMessage('Login successful.', true)
    await navigateForRole(role)
  } catch (error) {
    showMessage(getApiMessage(error, 'Unable to complete the email link login.'), false)
  } finally {
    finishAction()
  }
}

function clearSupabaseRedirectParams(): void {
  window.history.replaceState(window.history.state, document.title, getAuthRedirectUrl())
}
</script>
