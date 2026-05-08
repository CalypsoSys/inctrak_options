<template>
  <main class="mx-auto max-w-5xl px-4 py-10 md:px-8">
    <section>
      <article class="card-surface rounded-[2rem] p-8">
        <PageIntro
          eyebrow="Public Signup"
          title="Create your company workspace"
          description="Launch your company workspace with a branded company URL and a guided setup flow for your first administrator."
        >
          <template #actions>
            <Button label="How It Works" icon="pi pi-question-circle" severity="contrast" variant="outlined" @click="helpVisible = true" />
            <a
              class="rounded-full border border-[var(--app-border)] px-4 py-2 text-sm font-semibold text-[var(--app-muted)] transition hover:border-[var(--app-accent)] hover:text-[var(--app-accent)]"
              :href="mainAppLoginUrl"
            >
              Already Have An Account?
            </a>
          </template>
        </PageIntro>

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

          <div class="space-y-5">
            <div>
              <label class="field-label">Company Name</label>
              <input v-model="form.companyName" class="field-input" type="text" :disabled="accessGate.disabled" />
            </div>
            <div>
              <label class="field-label">Company URL</label>
              <div class="flex flex-col gap-3 md:flex-row md:items-center">
                <input
                  v-model="form.tenantSlug"
                  class="field-input flex-1"
                  type="text"
                  :disabled="accessGate.disabled"
                  autocapitalize="off"
                  spellcheck="false"
                  placeholder="your-company"
                  @input="slugWasManuallyEdited = true"
                />
                <span class="inline-flex items-center rounded-2xl border border-[var(--app-border)] bg-white/80 px-4 py-3 text-sm font-semibold text-[var(--app-muted)]">
                  .inctrak.com
                </span>
              </div>
              <p class="mt-2 text-xs leading-5 text-[var(--app-muted)]">
                Choose the company URL name you want people to recognize. Letters, numbers, and dashes only.
              </p>
              <p v-if="slugStatusMessage" class="mt-2 text-sm font-semibold" :class="slugAvailable === true ? 'text-emerald-700' : slugAvailable === false ? 'text-rose-600' : 'text-[var(--app-muted)]'">
                {{ slugStatusMessage }}
              </p>
            </div>
          </div>

          <div>
            <label class="field-label">Confirm Password</label>
            <Password v-model="form.confirmPassword" fluid toggle-mask :feedback="false" :disabled="accessGate.disabled" />
          </div>

          <div class="grid gap-4 md:grid-cols-[1fr_auto]">
            <div class="rounded-2xl border border-[var(--app-border)] bg-white/60 px-4 py-3 text-sm font-semibold text-[var(--app-muted)]">
              <div class="flex items-center gap-3">
                <input :checked="form.acceptTerms" type="checkbox" disabled />
                <span>{{ form.acceptTerms ? 'Terms accepted' : 'Review terms required' }}</span>
                <Button class="ml-auto" type="button" severity="secondary" variant="text" label="View Terms" @click="openTermsDialog" />
              </div>
            </div>
            <a
              class="inline-flex items-center justify-center rounded-2xl border border-[var(--app-border)] bg-white/60 px-4 py-3 text-sm font-semibold text-[var(--app-muted)] transition hover:border-[var(--app-accent)] hover:text-[var(--app-accent)]"
              :href="mainAppLoginUrl"
            >
              Back To Login
            </a>
          </div>

          <div class="flex flex-wrap gap-3">
            <Button
              type="submit"
              :loading="isBusy"
              :disabled="accessGate.disabled || !form.acceptTerms || slugAvailable !== true"
              :label="accessGate.disabled ? 'Coming Soon' : 'Create Workspace'"
            />
            <p v-if="submitBlockMessage" class="self-center text-sm font-semibold text-[var(--app-muted)]">
              {{ submitBlockMessage }}
            </p>
          </div>
        </form>
      </article>
    </section>

    <AppDialog
      v-model:visible="dialogVisible"
      title="Signup"
      :message="message"
      :success="isSuccess"
    />

    <Dialog
      v-model:visible="helpVisible"
      modal
      dismissable-mask
      header="How Signup Works"
      :style="{ width: 'min(42rem, 94vw)' }"
    >
      <div class="space-y-5 text-sm leading-7 text-[var(--app-muted)]">
        <div class="rounded-2xl border border-[var(--app-border)] bg-white/70 p-4">
          <p class="font-semibold text-slate-900">What to enter</p>
          <ol class="mt-3 list-decimal space-y-2 pl-5">
            <li>Use an email you can access right now.</li>
            <li>Choose a password you will remember.</li>
            <li>Enter the company name you want shown in IncTrak.</li>
            <li>Pick a short company URL name. We check availability before you can submit.</li>
          </ol>
        </div>

        <div class="rounded-2xl border border-[var(--app-border)] bg-white/70 p-4">
          <p class="font-semibold text-slate-900">What happens after submit</p>
          <ul class="mt-3 list-disc space-y-2 pl-5">
            <li>Supabase creates your credential record.</li>
            <li>If your project requires email confirmation, you will confirm first and then log in from the main app.</li>
            <li>If Supabase returns a working session immediately, IncTrak provisions your company workspace and first admin access automatically.</li>
          </ul>
        </div>

        <div class="rounded-2xl border border-[var(--app-border)] bg-white/70 p-4">
          <p class="font-semibold text-slate-900">Company URL tips</p>
          <ul class="mt-3 list-disc space-y-2 pl-5">
            <li>Keep it short and recognizable.</li>
            <li>Use letters, numbers, and dashes only.</li>
            <li>The company URL usually follows the company name automatically until you edit it yourself.</li>
          </ul>
        </div>

        <div class="flex justify-end">
          <Button label="Close" @click="helpVisible = false" />
        </div>
      </div>
    </Dialog>

    <Dialog
      v-model:visible="termsVisible"
      modal
      :draggable="false"
      :style="{ width: 'min(52rem, 94vw)' }"
      header="Terms of Service"
    >
      <div class="space-y-4">
        <p class="text-sm leading-6 text-[var(--app-muted)]">
          Please review these terms before creating your account. You must scroll to the end before the acceptance button becomes available.
        </p>
        <div
          ref="termsScrollContainer"
          class="max-h-[55vh] overflow-y-auto rounded-2xl border border-[var(--app-border)] bg-slate-50 p-5 text-sm leading-7 text-[var(--app-muted)]"
          @scroll="handleTermsScroll"
        >
          <section class="space-y-4">
            <h3 class="text-lg font-bold text-slate-900">Welcome to Calypso Systems, LLC</h3>
            <p>These terms and conditions outline the rules and regulations for the use of Calypso Systems, LLC&apos;s website, including the IncTrak platform and related tenant workspaces.</p>
            <p>By accessing this website, creating an account, or using an IncTrak workspace, you accept these terms and conditions in full. Do not continue to use the service if you do not accept all of the terms and conditions stated here.</p>
          </section>

          <section class="mt-6 space-y-4">
            <h4 class="text-base font-bold text-slate-900">Terminology and scope</h4>
            <p>&ldquo;Client&rdquo;, &ldquo;You&rdquo;, and &ldquo;Your&rdquo; refers to the person or entity accessing this website and accepting these terms and conditions. &ldquo;The Company&rdquo;, &ldquo;Ourselves&rdquo;, &ldquo;We&rdquo;, &ldquo;Our&rdquo;, and &ldquo;Us&rdquo; refers to Calypso Systems, LLC. &ldquo;Party&rdquo; or &ldquo;Parties&rdquo; refers to both the client and the company, or either one of them.</p>
            <p>All terms refer to the offer, acceptance, and consideration necessary to undertake the process of assisting the client in the most appropriate manner for providing the company&apos;s stated services and products, in accordance with and subject to applicable law.</p>
          </section>

          <section class="mt-6 space-y-4">
            <h4 class="text-base font-bold text-slate-900">Cookies</h4>
            <p>We employ the use of cookies. By using Calypso Systems, LLC&apos;s website, you consent to the use of cookies in accordance with our privacy policy.</p>
            <p>Most modern interactive web sites use cookies to retrieve user details for each visit. Cookies are used in some areas of our site to enable the functionality of those areas and to improve ease of use for visitors. Some of our affiliate or advertising partners may also use cookies.</p>
          </section>

          <section class="mt-6 space-y-4">
            <h4 class="text-base font-bold text-slate-900">License and intellectual property</h4>
            <p>Unless otherwise stated, Calypso Systems, LLC and its licensors own the intellectual property rights for all material on this website and the IncTrak service. All intellectual property rights are reserved. You may view and use pages from inctrak.com for your own internal business use, subject to the restrictions stated in these terms and conditions.</p>
            <p>You must not republish material from inctrak.com, sell, rent, or sub-license material from inctrak.com, reproduce, duplicate, or copy material from inctrak.com, or redistribute content from Calypso Systems, LLC unless content is specifically made for redistribution.</p>
          </section>

          <section class="mt-6 space-y-4">
            <h4 class="text-base font-bold text-slate-900">Accounts, security, and acceptable use</h4>
            <p>You are responsible for maintaining the confidentiality of your account credentials and for all activities that occur under your account. You agree to notify us promptly of any unauthorized use of your account or any other suspected security breach.</p>
            <p>You may use the service only for lawful business purposes and only with data and companies you are authorized to manage. You must not attempt to interfere with the availability, security, integrity, or normal operation of the service, nor use the service to store or transmit unlawful, infringing, deceptive, abusive, or malicious material.</p>
            <p>We may suspend, restrict, or terminate access to the service at any time if we reasonably believe your use violates these terms, threatens the security or stability of the platform, creates legal exposure, or risks harm to us, our users, or third parties.</p>
          </section>

          <section class="mt-6 space-y-4">
            <h4 class="text-base font-bold text-slate-900">Customer data and service use</h4>
            <p>You are responsible for the accuracy, legality, and completeness of all participant, grant, cap table, option, and related data entered into the service. You represent that you have the right to submit that data and to use the service in connection with it.</p>
            <p>The IncTrak service is an operational software tool. It is not legal, tax, accounting, securities, valuation, investment, or compensation advice. You are solely responsible for obtaining professional advice appropriate to your situation and for reviewing all outputs before relying on them for business, compliance, or transactional decisions.</p>
          </section>

          <section class="mt-6 space-y-4">
            <h4 class="text-base font-bold text-slate-900">Disclaimer and limitation of liability</h4>
            <p>To the maximum extent permitted by applicable law, we exclude all representations, warranties, and conditions relating to our website, the IncTrak service, and your use of them, including any warranties implied by law in respect of merchantability, satisfactory quality, fitness for a particular purpose, title, non-infringement, and the use of reasonable care and skill.</p>
            <p>The service is provided on an &ldquo;as is&rdquo; and &ldquo;as available&rdquo; basis. We do not warrant that the service will be uninterrupted, error-free, secure, or free from harmful components, or that defects will always be corrected.</p>
            <p>Subject to applicable law, we will not be liable for indirect, incidental, consequential, special, exemplary, or punitive damages, or for loss of profits, revenue, goodwill, data, or business opportunity.</p>
          </section>

          <section class="mt-6 space-y-4">
            <h4 class="text-base font-bold text-slate-900">Contact</h4>
            <p>If you have any questions regarding these terms, please contact <a class="font-semibold text-[var(--app-accent)]" href="mailto:contact@inctrak.com">contact@inctrak.com</a>.</p>
            <p class="rounded-2xl border border-emerald-200 bg-emerald-50 px-4 py-3 font-semibold text-emerald-700">
              End of Terms
            </p>
          </section>
        </div>
        <div class="flex flex-wrap items-center justify-between gap-3">
          <p class="text-sm font-semibold" :class="canAcceptTerms ? 'text-emerald-700' : 'text-[var(--app-muted)]'">
            {{ canAcceptTerms ? 'You have reached the end and can accept these terms.' : 'Scroll to the end to enable acceptance.' }}
          </p>
          <div class="flex gap-3">
            <Button type="button" severity="secondary" variant="text" label="Close" @click="termsVisible = false" />
            <Button type="button" :disabled="!canAcceptTerms" label="Accept Terms" @click="acceptTerms" />
          </div>
        </div>
      </div>
    </Dialog>
  </main>
</template>

<script setup lang="ts">
import axios from 'axios'
import { computed, nextTick, reactive, ref, watch } from 'vue'
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'
import Password from 'primevue/password'
import AppDialog from '@/components/AppDialog.vue'
import PageIntro from '@/components/PageIntro.vue'
import { useAsyncState } from '@/composables/useAsyncState'
import { accessGate } from '@/services/access-gate'
import { getApiMessage } from '@/services/api'
import { buildMainAppLoginUrl } from '@/services/runtime-config'
import { buildPendingSignupMessage } from '@/services/signup-messages'
import { signUpWithPassword } from '@/services/supabase-auth'
import { fetchTenantSlugAvailability, normalizeTenantSlug } from '@/services/tenant-slug'
import { provisionTenantSignup } from '@/services/tenant-signup'

const { isBusy, dialogVisible, isSuccess, message, showMessage } = useAsyncState()

const helpVisible = ref(false)
const termsVisible = ref(false)
const canAcceptTerms = ref(false)
const termsScrollContainer = ref<HTMLElement | null>(null)
const slugAvailable = ref<boolean | null>(null)
const slugStatusMessage = ref('')
const slugWasManuallyEdited = ref(false)
const mainAppLoginUrl = buildMainAppLoginUrl()

const form = reactive({
  email: '',
  password: '',
  confirmPassword: '',
  companyName: '',
  tenantSlug: '',
  acceptTerms: false
})

let slugAvailabilityTimer: ReturnType<typeof setTimeout> | null = null

const submitBlockMessage = computed(() => {
  if (accessGate.disabled) {
    return accessGate.message
  }

  if (!form.acceptTerms) {
    return 'Review and accept the Terms of Service before creating your account.'
  }

  if (slugAvailable.value !== true) {
    return slugStatusMessage.value || 'Choose an available company slug before creating your account.'
  }

  return ''
})

async function submitForm(): Promise<void> {
  if (accessGate.disabled) {
    showMessage(accessGate.message, false)
    return
  }

  const email = form.email.trim()
  const companyName = form.companyName.trim()
  const tenantSlug = normalizeTenantSlug(form.tenantSlug)

  if (!email || !form.password.trim()) {
    showMessage('Please enter an email address and password.', false)
    return
  }

  if (!companyName) {
    showMessage('Please enter a company name.', false)
    return
  }

  if (!tenantSlug) {
    showMessage('Please enter a company slug.', false)
    return
  }

  if (!form.confirmPassword.trim()) {
    showMessage('Please confirm your password.', false)
    return
  }

  if (form.password !== form.confirmPassword) {
    showMessage('Passwords do not match.', false)
    return
  }

  if (!form.acceptTerms) {
    showMessage('Please accept the terms and conditions.', false)
    return
  }

  isBusy.value = true
  try {
    const session = await signUpWithPassword(email, form.password, companyName, tenantSlug)

    if (!session.access_token) {
      showMessage(buildPendingSignupMessage(), true)
      form.password = ''
      form.confirmPassword = ''
      return
    }

    await ensureTenantProvisioning(session.access_token, companyName, tenantSlug)
    showMessage('Account created and workspace provisioned. Continue to the main login page to sign in.', true)
  } catch (error) {
    showMessage(getApiMessage(error, 'Unable to create the Supabase account.'), false)
  } finally {
    isBusy.value = false
  }
}

async function ensureTenantProvisioning(accessToken: string, companyName: string, tenantSlug: string): Promise<void> {
  try {
    await provisionTenantSignup(companyName, tenantSlug, accessToken)
  } catch (error) {
    if (!axios.isAxiosError(error) || !error.response || (error.response.status !== 401 && error.response.status !== 403)) {
      throw error
    }

    throw error
  }
}

function openTermsDialog(): void {
  termsVisible.value = true
  canAcceptTerms.value = false

  void nextTick(() => {
    if (termsScrollContainer.value) {
      termsScrollContainer.value.scrollTop = 0
    }
  })
}

function handleTermsScroll(): void {
  const container = termsScrollContainer.value
  if (!container) {
    return
  }

  const threshold = 16
  const reachedEnd = container.scrollTop + container.clientHeight >= container.scrollHeight - threshold
  if (reachedEnd) {
    canAcceptTerms.value = true
  }
}

function acceptTerms(): void {
  form.acceptTerms = true
  termsVisible.value = false
}

watch(
  () => form.companyName,
  (value) => {
    if (slugWasManuallyEdited.value) {
      return
    }

    form.tenantSlug = normalizeTenantSlug(value)
  }
)

watch(
  () => form.tenantSlug,
  (value) => {
    const normalized = normalizeTenantSlug(value)
    if (value !== normalized) {
      form.tenantSlug = normalized
      return
    }

    if (slugAvailabilityTimer) {
      clearTimeout(slugAvailabilityTimer)
    }

    if (!normalized) {
      slugAvailable.value = null
      slugStatusMessage.value = 'Enter a company slug.'
      return
    }

    slugAvailable.value = null
    slugStatusMessage.value = 'Checking company slug availability...'
    slugAvailabilityTimer = setTimeout(async () => {
      try {
        const response = await fetchTenantSlugAvailability(normalized)
        form.tenantSlug = response.TenantSlug ?? normalized
        slugAvailable.value = response.Available === true
        slugStatusMessage.value = response.Message ?? ''
      } catch {
        slugAvailable.value = false
        slugStatusMessage.value = 'Unable to verify the company slug right now.'
      }
    }, 250)
  }
)
</script>
