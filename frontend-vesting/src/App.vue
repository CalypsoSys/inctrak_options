<template>
  <main class="mx-auto max-w-5xl px-4 py-10 md:px-8">
    <section>
      <article class="card-surface rounded-[2rem] p-8">
        <PageIntro
          eyebrow="Vesting Calculator"
          title="Model an equity vesting schedule in minutes"
          description="Estimate how shares vest over time with a simple public calculator built for founders, operators, and participants."
        >
          <template #actions>
            <Button label="How It Works" icon="pi pi-question-circle" severity="contrast" variant="outlined" @click="helpVisible = true" />
            <a class="rounded-full border border-[var(--app-border)] px-4 py-2 text-sm font-semibold text-[var(--app-muted)] transition hover:border-[var(--app-accent)] hover:text-[var(--app-accent)]" href="https://www.inctrak.com" target="_blank" rel="noopener noreferrer">
              Learn More
            </a>
          </template>
        </PageIntro>

        <div class="mt-4 text-sm text-[var(--app-muted)]">
          Need help tailoring a vesting schedule for your company?
          <button
            class="ml-1 font-semibold text-[var(--app-accent)] transition hover:text-slate-900"
            type="button"
            @click="openContactDialog"
          >
            Contact us
          </button>
          .
        </div>

        <div class="mt-8 rounded-[2rem] border border-[var(--app-border)] bg-white/70 p-5">
          <div class="max-w-3xl">
            <h2 class="text-lg font-bold text-slate-900">Describe the schedule in plain English</h2>
            <p class="mt-2 text-sm leading-6 text-[var(--app-muted)]">Start with a common vesting phrase and let IncTrak build the first draft for you.</p>
            <p class="mt-2 text-sm leading-6 text-[var(--app-muted)]">Best results usually include three things: the total duration, the cadence or cliff, and if you know them already, the shares granted and vesting start date.</p>
          </div>
          <div class="mt-4 rounded-[1.5rem] border-2 border-[var(--app-accent)]/20 bg-white/85 p-4 shadow-[0_18px_40px_rgba(15,118,110,0.08)]">
            <label class="field-label">Schedule Description</label>
            <textarea
              v-model="promptText"
              class="field-textarea min-h-52 w-full resize-y"
              placeholder="Example: I want a standard four-year time-based vesting schedule with a one-year cliff, monthly after."
            />
          </div>
          <div class="mt-4 flex justify-start">
            <Button label="Generate From Description" icon="pi pi-sparkles" :loading="isInterpreting" @click="generateFromPrompt(false)" />
            <Button
              v-if="showTryAlternate"
              class="ml-3"
              label="Try Alternate"
              icon="pi pi-refresh"
              severity="contrast"
              variant="outlined"
              :loading="isInterpreting"
              @click="applyAlternateInterpretation"
            />
            <Button
              v-if="showStillNotRight"
              class="ml-3"
              label="Still not right"
              icon="pi pi-times-circle"
              severity="contrast"
              variant="outlined"
              :loading="isInterpreting"
              @click="revealAiChoice = true"
            />
            <Button
              v-if="showUseAiInstead"
              class="ml-3"
              label="Use AI Instead"
              icon="pi pi-bolt"
              severity="contrast"
              variant="outlined"
              :loading="isInterpreting"
              @click="generateFromPrompt(true)"
            />
          </div>
          <div class="mt-4 flex flex-wrap gap-2">
            <button
              v-for="example in promptExamples"
              :key="example"
              class="rounded-full border border-[var(--app-border)] bg-white px-3 py-2 text-sm font-medium text-[var(--app-muted)] transition hover:border-[var(--app-accent)] hover:text-[var(--app-accent)]"
              type="button"
              @click="promptText = example"
            >
              {{ example }}
            </button>
          </div>
          <div v-if="interpretSummary" class="mt-4 rounded-2xl border border-emerald-200 bg-emerald-50/80 px-4 py-3 text-sm leading-6 text-emerald-900">
            {{ interpretSummary }}
          </div>
        </div>

        <div class="mt-8 grid gap-4 md:grid-cols-2">
          <div>
            <label class="field-label">Shares Granted</label>
            <input v-model.number="quickGrant.SHARES" class="field-input" min="1" type="number" />
            <p class="mt-2 text-xs leading-5 text-[var(--app-muted)]">Enter the total number of shares in the grant you want to model.</p>
          </div>
          <div>
            <label class="field-label">Vesting Start</label>
            <input v-model="quickGrant.VESTING_START" class="field-input" type="date" />
            <p class="mt-2 text-xs leading-5 text-[var(--app-muted)]">This is the date the vesting schedule begins.</p>
          </div>
        </div>

        <div class="mt-6">
          <VestingPeriodEditor
            :amount-types="quickAmountTypes"
            :period-types="quickPeriodTypes"
            :periods="quickPeriods"
            @add="addQuickPeriod"
            @help="helpVisible = true"
            @remove="removeQuickPeriod"
          />
        </div>

        <div class="mt-5 flex flex-wrap items-center gap-3">
          <Button label="Calculate Vesting" :loading="isBusy" @click="submitQuickGrant" />
          <p class="text-sm text-[var(--app-muted)]">This app stays public and uses the shared API only for quick vesting.</p>
        </div>
      </article>
    </section>

    <section v-if="quickVestSchedule.length > 0" ref="timelineSection" class="mt-8">
      <VestingScheduleTable :rows="quickVestSchedule" />
      <div class="mt-4 flex justify-end">
        <Button label="Back To Top" icon="pi pi-arrow-up" severity="contrast" variant="outlined" @click="scrollToTop" />
      </div>
    </section>

    <Dialog
      v-model:visible="dialogVisible"
      modal
      dismissable-mask
      :header="dialogTitle"
      :style="{ width: 'min(32rem, 92vw)' }"
    >
      <div class="space-y-3">
        <Message :severity="dialogSuccess ? 'success' : 'error'" :closable="false">
          {{ dialogMessage }}
        </Message>
        <div class="flex justify-end">
          <Button label="Close" @click="dialogVisible = false" />
        </div>
      </div>
    </Dialog>

    <Dialog
      v-model:visible="contactVisible"
      modal
      dismissable-mask
      header="Contact Us"
      :style="{ width: 'min(48rem, 94vw)' }"
    >
      <form class="space-y-4" @submit.prevent="submitContactForm">
        <p class="text-sm leading-6 text-[var(--app-muted)]">
          Tell us what you are working on and we will get the message in Slack right away.
        </p>
        <div class="grid gap-4 md:grid-cols-2">
          <div>
            <label class="field-label">Name</label>
            <input v-model="contactForm.Name" class="field-input" type="text" />
          </div>
          <div>
            <label class="field-label">Email</label>
            <input v-model="contactForm.EmailAddress" class="field-input" type="email" />
          </div>
        </div>
        <div>
          <label class="field-label">Subject</label>
          <input v-model="contactForm.Subject" class="field-input" type="text" />
        </div>
        <div>
          <label class="field-label">Message</label>
          <textarea
            v-model="contactForm.Message"
            class="field-textarea min-h-40 w-full resize-y"
            placeholder="Tell us about your vesting question, your company stage, or the workflow you want help with."
          />
        </div>
        <div class="rounded-2xl border border-[var(--app-border)] bg-white/70 p-4">
          <p class="text-xs font-semibold uppercase tracking-[0.24em] text-[var(--app-accent)]">Direct Contact</p>
          <div class="mt-3 grid gap-3 md:grid-cols-2">
            <a
              class="flex items-center gap-3 rounded-2xl border border-[var(--app-border)] px-4 py-3 text-sm font-medium text-slate-900 transition hover:border-[var(--app-accent)] hover:text-[var(--app-accent)]"
              href="tel:+12038487851"
            >
              <i class="pi pi-phone text-[var(--app-accent)]" />
              <span>+1-203-848-7851</span>
            </a>
            <a
              class="flex items-center gap-3 rounded-2xl border border-[var(--app-border)] px-4 py-3 text-sm font-medium text-slate-900 transition hover:border-[var(--app-accent)] hover:text-[var(--app-accent)]"
              href="mailto:contact@inctrak.com"
            >
              <i class="pi pi-envelope text-[var(--app-accent)]" />
              <span>contact@inctrak.com</span>
            </a>
          </div>
        </div>
        <div class="flex justify-end gap-3">
          <Button label="Close" severity="contrast" variant="outlined" type="button" @click="contactVisible = false" />
          <Button label="Submit" :loading="isSendingContact" type="submit" />
        </div>
      </form>
    </Dialog>

    <Dialog
      v-model:visible="helpVisible"
      modal
      dismissable-mask
      header="How To Build A Vesting Schedule"
      :style="{ width: 'min(44rem, 94vw)' }"
    >
      <div class="space-y-5 text-sm leading-7 text-[var(--app-muted)]">
        <p>Think of the calculator as a two-step process. First, describe the vesting logic clearly enough that IncTrak can draft the steps. Then review the draft and calculate the dated vesting timeline.</p>

        <div class="rounded-2xl border border-[var(--app-border)] bg-white/70 p-4">
          <p class="font-semibold text-slate-900">Fastest way to get a good result</p>
          <ol class="mt-3 list-decimal space-y-2 pl-5">
            <li>Start with the overall schedule shape: for example <strong class="text-slate-900">four years</strong>, <strong class="text-slate-900">three years quarterly</strong>, or <strong class="text-slate-900">one-year cliff, monthly after</strong>.</li>
            <li>Add the grant facts if you know them: <strong class="text-slate-900">50000 shares</strong> and a <strong class="text-slate-900">vesting start date</strong>.</li>
            <li>Review the generated periods, then calculate the full vesting timeline below.</li>
          </ol>
        </div>

        <div class="rounded-2xl border border-[var(--app-border)] bg-white/70 p-4">
          <p class="font-semibold text-slate-900">What the fields mean</p>
          <ul class="mt-3 list-disc space-y-2 pl-5">
            <li><strong class="text-slate-900">Length of Each Step</strong>: how long to wait between vesting events.</li>
            <li><strong class="text-slate-900">Step Unit</strong>: whether that timing is in years, months, weeks, or days.</li>
            <li><strong class="text-slate-900">Vests In</strong>: choose <strong>Shares</strong> for fixed share counts or <strong>Percentage</strong> for percent-based vesting.</li>
            <li><strong class="text-slate-900">Amount Per Step</strong>: how much vests each time the step occurs.</li>
            <li><strong class="text-slate-900">Number of Steps</strong>: how many times that vesting event repeats.</li>
          </ul>
        </div>

        <div class="rounded-2xl border border-[var(--app-border)] bg-white/70 p-4">
          <p class="font-semibold text-slate-900">Helpful shortcuts</p>
          <ul class="mt-3 list-disc space-y-2 pl-5">
            <li><strong class="text-slate-900">Split the total evenly across the number of steps</strong> fills in the amount for you after you choose how many steps there are.</li>
            <li><strong class="text-slate-900">Figure out the number of steps automatically from the amount</strong> works the other way around and calculates how many steps are needed.</li>
          </ul>
        </div>

        <div class="rounded-2xl border border-[var(--app-border)] bg-white/70 p-4">
          <p class="font-semibold text-slate-900">What happens after you click Generate</p>
          <ul class="mt-3 list-disc space-y-2 pl-5">
            <li>If the built-in interpreter understands your wording, it drafts the schedule immediately.</li>
            <li>If two low-cost built-in interpretations disagree, you may see <strong class="text-slate-900">Try Alternate</strong> so you can compare the other draft.</li>
            <li>If neither built-in draft feels right, use <strong class="text-slate-900">Still not right</strong> and then <strong class="text-slate-900">Use AI Instead</strong> as the last step.</li>
            <li>If the prompt also gives us both shares granted and vesting start, the page will calculate the vesting timeline automatically for you.</li>
          </ul>
        </div>

        <div class="rounded-2xl border border-[var(--app-border)] bg-white/70 p-4">
          <p class="font-semibold text-slate-900">Simple example</p>
          <p class="mt-2">For a 4-year monthly vesting schedule with equal vesting, you could use one period with:</p>
          <ul class="mt-3 list-disc space-y-2 pl-5">
            <li><strong class="text-slate-900">Length of Each Step</strong>: 1</li>
            <li><strong class="text-slate-900">Step Unit</strong>: Months</li>
            <li><strong class="text-slate-900">Vests In</strong>: Percentage</li>
            <li><strong class="text-slate-900">Number of Steps</strong>: 48</li>
            <li><strong class="text-slate-900">Split the total evenly across the number of steps</strong>: checked</li>
          </ul>
        </div>

        <div class="rounded-2xl border border-[var(--app-border)] bg-white/70 p-4">
          <p class="font-semibold text-slate-900">Natural-language example</p>
          <p class="mt-2">You can also describe a schedule in plain English, for example:</p>
          <p class="mt-3 rounded-2xl bg-slate-950/5 px-4 py-3 font-medium text-slate-900">"I want a standard four-year time-based vesting schedule with a one-year cliff, monthly after."</p>
          <p class="mt-3">That should translate into a 25% cliff after one year, then monthly vesting across the remaining 36 months.</p>
          <p class="mt-3 rounded-2xl bg-slate-950/5 px-4 py-3 font-medium text-slate-900">"Create a three-year quarterly vesting schedule for 100000 shares with vest start date 1/1/2022."</p>
          <p class="mt-3">That should fill the shares granted, vesting start, and quarterly vesting periods in one pass.</p>
        </div>

        <div class="flex justify-end">
          <Button label="Close" @click="helpVisible = false" />
        </div>
      </div>
    </Dialog>
  </main>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from 'vue'
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'
import Message from 'primevue/message'
import PageIntro from '@/components/PageIntro.vue'
import VestingPeriodEditor from '@/components/VestingPeriodEditor.vue'
import VestingScheduleTable from '@/components/VestingScheduleTable.vue'
import { getApiMessage } from '@/services/api'
import { canSubmitContactForm } from '@/services/contact-form'
import { sendContactMessage } from '@/services/contact-service'
import {
  canAutoCalculateFromPromptResult,
  shouldShowStillNotRight as shouldShowStillNotRightForProvider,
  shouldShowTryAlternate as shouldShowTryAlternateForProvider,
  shouldShowUseAiInstead as shouldShowUseAiInsteadForProvider
} from '@/services/prompt-flow'
import { buildPromptGrantPatch } from '@/services/prompt-interpret'
import { normalizeQuickStartDate } from '@/services/quick-vesting'
import { fetchQuickGrant, interpretQuickPrompt, saveQuickGrant } from '@/services/vesting-service'
import type { AmountType, FeedbackForm, Grant, Period, PeriodType, VestScheduleEntry } from '@/services/types'

const isBusy = ref(false)
const isInterpreting = ref(false)
const dialogVisible = ref(false)
const dialogSuccess = ref(false)
const dialogTitle = ref('Vesting')
const dialogMessage = ref('')
const contactVisible = ref(false)
const helpVisible = ref(false)
const isSendingContact = ref(false)
const timelineSection = ref<HTMLElement | null>(null)
const promptText = ref('I want a standard four-year time-based vesting schedule with a one-year cliff, monthly after.')
const interpretSummary = ref('')
const interpretProvider = ref('')
const interpretRequiresAi = ref(false)
const interpretAlternateProvider = ref('')
const revealAiChoice = ref(false)
const promptExamples = [
  'I want a standard four-year time-based vesting schedule with a one-year cliff, monthly after.',
  'Create a standard four-year monthly vesting schedule.',
  'Create a three-year quarterly vesting schedule.'
]

const quickGrant = reactive<Grant>({
  GRANT_PK: '',
  PARTICIPANT_FK: null,
  PLAN_FK: null,
  VESTING_SCHEDULE_FK: null,
  TERMINATION_FK: null,
  SHARES: 0,
  OPTION_PRICE: 0,
  DATE_OF_GRANT: '',
  VESTING_START: ''
})
const quickPeriods = ref<Period[]>([])
const quickPeriodTypes = ref<PeriodType[]>([])
const quickAmountTypes = ref<AmountType[]>([])
const quickVestSchedule = ref<VestScheduleEntry[]>([])
const contactForm = reactive<FeedbackForm>({
  EmailAddress: '',
  Name: '',
  MessageTypeFk: 7,
  Subject: '',
  Message: ''
})

onMounted(async () => {
  const quickData = await fetchQuickGrant()
  Object.assign(quickGrant, quickData.Grant, {
    VESTING_START: normalizeQuickStartDate(quickData.Grant.VESTING_START)
  })
  quickPeriods.value = quickData.Periods
  quickPeriodTypes.value = quickData.PeriodTypes
  quickAmountTypes.value = quickData.AmountTypes
})

watch(
  () => [quickGrant.SHARES, quickGrant.VESTING_START],
  () => {
    clearTimeline()
  }
)

watch(promptText, () => {
  interpretSummary.value = ''
  interpretProvider.value = ''
  interpretAlternateProvider.value = ''
  interpretRequiresAi.value = false
  revealAiChoice.value = false
})

watch(
  quickPeriods,
  () => {
    clearTimeline()
  },
  { deep: true }
)

function addQuickPeriod(): void {
  quickPeriods.value.push({
    PERIOD_AMOUNT: 1,
    PERIOD_TYPE_FK: null,
    AMOUNT_TYPE_FK: null,
    AMOUNT: 0,
    INCREMENTS: 1,
    ORDER: quickPeriods.value.length,
    EVEN_OVER_N: 0
  })
}

function removeQuickPeriod(index: number): void {
  quickPeriods.value.splice(index, 1)
}

async function generateFromPromptWithMode(strictAi: boolean): Promise<void> {
  return generateFromPromptCore(strictAi)
}

async function generateFromPromptCore(strictAi: boolean, preferredProvider?: string): Promise<void> {
  isInterpreting.value = true
  try {
    const response = await interpretQuickPrompt(promptText.value, strictAi, preferredProvider)
    if (response.success === false)
    {
      showDialog(response.message ?? 'Unable to interpret that vesting description yet.', false)
      return
    }

    quickPeriods.value = response.Periods
    Object.assign(quickGrant, buildPromptGrantPatch(response))
    quickPeriodTypes.value = response.PeriodTypes
    quickAmountTypes.value = response.AmountTypes
    interpretProvider.value = response.provider ?? ''
    interpretAlternateProvider.value = response.alternateProvider ?? ''
    interpretRequiresAi.value = response.requiresAi === true && strictAi === false
    revealAiChoice.value = strictAi
    const providerLabel = getInterpretProviderLabel(response.provider)
    const confidenceLabel = typeof response.confidence === 'number'
      ? `Confidence: ${Math.round(response.confidence * 100)}%.`
      : ''
    const summary = response.summary ?? 'Built a suggested vesting schedule from your description.'
    interpretSummary.value = [providerLabel, confidenceLabel, summary].filter(Boolean).join(' ')

    if (canAutoCalculate()) {
      await submitQuickGrant()
    }
  } catch (error) {
    showDialog(getApiMessage(error, 'Unable to interpret that vesting description yet.'), false)
  } finally {
    isInterpreting.value = false
  }
}

function generateFromPrompt(strictAi = false): Promise<void> {
  return generateFromPromptWithMode(strictAi)
}

function applyAlternateInterpretation(): Promise<void> {
  if (interpretAlternateProvider.value === '') {
    return Promise.resolve()
  }

  return generateFromPromptCore(false, interpretAlternateProvider.value)
}

async function submitQuickGrant(): Promise<void> {
  if (quickGrant.SHARES <= 0)
  {
    showDialog('Enter a Shares Granted value greater than zero before calculating vesting.', false)
    return
  }

  if (quickPeriods.value.length === 0)
  {
    showDialog('Add at least one vesting period before calculating vesting.', false)
    return
  }

  isBusy.value = true
  try {
    const response = await saveQuickGrant(quickGrant, quickPeriods.value)
    quickVestSchedule.value = response.VestSchedule ?? []
    if (response.success === false)
    {
      showDialog(response.message ?? 'Unable to calculate the quick vesting schedule.', false)
    }
    else
    {
      scrollToTimeline()
    }
  } catch (error) {
    showDialog(getApiMessage(error, 'Unable to calculate the quick vesting schedule.'), false)
  } finally {
    isBusy.value = false
  }
}

function showDialog(message: string, success: boolean): void {
  dialogMessage.value = message
  dialogSuccess.value = success
  dialogVisible.value = true
}

function openContactDialog(): void {
  contactVisible.value = true
}

function scrollToTimeline(): void {
  window.requestAnimationFrame(() => {
    timelineSection.value?.scrollIntoView({ behavior: 'smooth', block: 'start' })
  })
}

function scrollToTop(): void {
  window.requestAnimationFrame(() => {
    window.scrollTo({ top: 0, behavior: 'smooth' })
  })
}

function clearTimeline(): void {
  quickVestSchedule.value = []
}

async function submitContactForm(): Promise<void> {
  if (canSubmitContactForm(contactForm) === false) {
    showDialog('Please enter your name, email, subject, and message before submitting.', false)
    return
  }

  isSendingContact.value = true
  try {
    const response = await sendContactMessage(contactForm)
    contactVisible.value = false
    showDialog(
      response.success !== false
        ? 'Your message has been sent. We will reach out soon.'
        : (response.message ?? 'Unable to send your message right now.'),
      response.success !== false
    )
    if (response.success !== false) {
      contactForm.EmailAddress = ''
      contactForm.Name = ''
      contactForm.Subject = ''
      contactForm.Message = ''
    }
  } catch (error) {
    showDialog(getApiMessage(error, 'Unable to send your message right now.'), false)
  } finally {
    isSendingContact.value = false
  }
}

function canAutoCalculate(): boolean {
  return canAutoCalculateFromPromptResult(quickGrant.SHARES, quickGrant.VESTING_START, quickPeriods.value.length)
}

const showTryAlternate = computed(() => shouldShowTryAlternateForProvider(interpretAlternateProvider.value))

const showStillNotRight = computed(() => shouldShowStillNotRightForProvider(interpretProvider.value))

const showUseAiInstead = computed(() => shouldShowUseAiInsteadForProvider(revealAiChoice.value, interpretProvider.value))

function getInterpretProviderLabel(provider?: string): string {
  switch (provider) {
    case 'pattern':
      return 'Matched a standard vesting phrase.'
    case 'parser':
      return 'Used the built-in vesting language parser.'
    case 'llamasharp':
      return 'Used the local AI model.'
    case 'local-http':
      return 'Used the local AI endpoint.'
    default:
      return provider ? `Provider: ${provider}.` : ''
  }
}
</script>
