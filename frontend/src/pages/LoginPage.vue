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
          <div v-if="form.isRegistering" class="flex items-center gap-3 rounded-2xl border border-[var(--app-border)] bg-white/60 px-4 py-3 text-sm font-semibold text-[var(--app-muted)]">
            <input :checked="form.acceptTerms" type="checkbox" disabled />
            <span>{{ form.acceptTerms ? 'Terms accepted' : 'Review terms required' }}</span>
            <Button class="ml-auto" type="button" severity="secondary" variant="text" label="View Terms" @click="openTermsDialog" />
          </div>
        </div>
        <div class="flex flex-wrap gap-3">
          <Button
            type="submit"
            :loading="isBusy"
            :disabled="form.isRegistering && !form.acceptTerms"
            :label="form.isRegistering ? 'Create Account' : 'Login'"
          />
          <p v-if="form.isRegistering && !form.acceptTerms" class="self-center text-sm font-semibold text-[var(--app-muted)]">
            Review and accept the Terms of Service before creating your account.
          </p>
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
            <h4 class="text-base font-bold text-slate-900">User content and comments</h4>
            <p>Certain parts of this website may offer the opportunity for users to post or exchange opinions, information, material, or data. Calypso Systems, LLC does not screen, edit, publish, or review comments before they appear on the website, and such comments do not necessarily reflect the views or opinions of Calypso Systems, LLC, its agents, or affiliates.</p>
            <p>To the extent permitted by applicable laws, Calypso Systems, LLC shall not be responsible or liable for comments or for any loss, cost, liability, damages, or expenses caused by any use of, posting of, or appearance of comments on this website. We reserve the right to monitor all comments and to remove any comments considered inappropriate, offensive, or otherwise in breach of these terms and conditions.</p>
            <p>You warrant and represent that you are entitled to post such content and have all necessary licenses and consents to do so, that your content does not infringe any intellectual property right or other proprietary right of any third party, that it does not contain defamatory, offensive, indecent, unlawful, or privacy-invasive material, and that it will not be used to solicit or promote unlawful or improper activity.</p>
          </section>

          <section class="mt-6 space-y-4">
            <h4 class="text-base font-bold text-slate-900">Hyperlinking and branding</h4>
            <p>The following organizations may link to our website without prior written approval: government agencies, search engines, news organizations, online directory distributors listing us in the directory in the same manner as other listed businesses, and systemwide accredited businesses except soliciting non-profit organizations, charity shopping malls, and charity fundraising groups.</p>
            <p>These organizations may link to our home page, publications, or other website information so long as the link is not misleading, does not falsely imply sponsorship, endorsement, or approval, and fits within the context of the linking party&apos;s site.</p>
            <p>We may consider and approve in our sole discretion other link requests from commonly known consumer or business information sources, associations, charities, online directory distributors, internet portals, accounting firms, law firms, consulting firms, educational institutions, and trade associations.</p>
            <p>No use of Calypso Systems, LLC&apos;s logo or other artwork will be allowed for linking absent a trademark license agreement.</p>
          </section>

          <section class="mt-6 space-y-4">
            <h4 class="text-base font-bold text-slate-900">Reservation of rights</h4>
            <p>We reserve the right at any time and in our sole discretion to request that you remove all links or any particular link to our website. You agree to immediately remove all links to our website upon such request. We also reserve the right to amend these terms and conditions and our linking policy at any time. By continuing to link to or use our website, you agree to be bound by and abide by these terms and conditions.</p>
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
            <p>We may use aggregated or de-identified operational information to maintain, secure, improve, support, and analyze the service, provided that such use does not identify your confidential tenant data as your own unless otherwise permitted by law or your separate agreement with us.</p>
          </section>

          <section class="mt-6 space-y-4">
            <h4 class="text-base font-bold text-slate-900">Removal requests and accuracy</h4>
            <p>If you find any link or content on our website objectionable, you may contact us about it. We will consider requests to remove links or content, but we are not obligated to do so or to respond directly in every case.</p>
            <p>While we endeavor to keep information accurate and current, we do not warrant that all information on the site is complete, continuously available, or up to date at all times.</p>
          </section>

          <section class="mt-6 space-y-4">
            <h4 class="text-base font-bold text-slate-900">Content liability</h4>
            <p>We shall have no responsibility or liability for any content appearing on your website or on third-party websites that link to or reference our site. You agree to indemnify, defend, and hold us harmless against all claims, liabilities, damages, losses, and expenses arising out of or based upon your website, your data, your use of the service, your violation of these terms, or your infringement of any rights of another party.</p>
          </section>

          <section class="mt-6 space-y-4">
            <h4 class="text-base font-bold text-slate-900">Disclaimer and limitation of liability</h4>
            <p>To the maximum extent permitted by applicable law, we exclude all representations, warranties, and conditions relating to our website, the IncTrak service, and your use of them, including any warranties implied by law in respect of merchantability, satisfactory quality, fitness for a particular purpose, title, non-infringement, and the use of reasonable care and skill.</p>
            <p>The service is provided on an &ldquo;as is&rdquo; and &ldquo;as available&rdquo; basis. We do not warrant that the service will be uninterrupted, error-free, secure, or free from harmful components, or that defects will always be corrected.</p>
            <p>Nothing in this disclaimer limits or excludes liability for death or personal injury resulting from negligence, liability for fraud or fraudulent misrepresentation, or any liability that may not be excluded or limited under applicable law.</p>
            <p>Subject to the preceding sentence, to the fullest extent permitted by law, we will not be liable for any indirect, incidental, consequential, special, exemplary, or punitive damages, or for any loss of profits, revenue, goodwill, business opportunity, data, or anticipated savings, whether arising in contract, tort, negligence, breach of statutory duty, or otherwise, even if advised of the possibility of such damages.</p>
            <p>To the extent that the website and information or services on the website are provided free of charge, we will not be liable for any loss or damage of any nature. Where liability cannot be fully excluded, our total aggregate liability arising out of or relating to the service shall not exceed the amounts actually paid by you to us for the service during the twelve months preceding the event giving rise to the claim.</p>
          </section>

          <section class="mt-6 space-y-4">
            <h4 class="text-base font-bold text-slate-900">Limited free use</h4>
            <p>The shared online version is intended to be free for startups, generally defined as businesses operating for less than three years and with fewer than fifteen participants, unless otherwise agreed.</p>
          </section>

          <section class="mt-6 space-y-4">
            <h4 class="text-base font-bold text-slate-900">Contact</h4>
            <p>This Terms and Conditions page was originally based on a template and has been adapted for IncTrak. If you have any questions regarding these terms, please contact <a class="font-semibold text-[var(--app-accent)]" href="mailto:contact@inctrak.com">contact@inctrak.com</a>.</p>
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
  </section>
</template>

<script setup lang="ts">
import axios from 'axios'
import { nextTick, reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'
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
const termsVisible = ref(false)
const canAcceptTerms = ref(false)
const termsScrollContainer = ref<HTMLElement | null>(null)

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

    const tenant = await provisionTenantSignup(companyName, tenantSlug, accessToken)
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
</script>
