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

        <div class="mt-8 rounded-[2rem] border border-[var(--app-border)] bg-white/70 p-5">
          <div class="max-w-3xl">
            <h2 class="text-lg font-bold text-slate-900">Describe the schedule in plain English</h2>
            <p class="mt-2 text-sm leading-6 text-[var(--app-muted)]">Start with a common vesting phrase and let IncTrak build the first draft for you.</p>
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
            <Button label="Generate From Description" icon="pi pi-sparkles" :loading="isInterpreting" @click="generateFromPrompt" />
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
      v-model:visible="helpVisible"
      modal
      dismissable-mask
      header="How To Build A Vesting Schedule"
      :style="{ width: 'min(44rem, 94vw)' }"
    >
      <div class="space-y-5 text-sm leading-7 text-[var(--app-muted)]">
        <p>Each vesting period describes one repeating step in the schedule: how long each step is, what vests each time, and how many times that step repeats.</p>

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
        </div>

        <div class="flex justify-end">
          <Button label="Close" @click="helpVisible = false" />
        </div>
      </div>
    </Dialog>
  </main>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref, watch } from 'vue'
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'
import Message from 'primevue/message'
import PageIntro from '@/components/PageIntro.vue'
import VestingPeriodEditor from '@/components/VestingPeriodEditor.vue'
import VestingScheduleTable from '@/components/VestingScheduleTable.vue'
import { getApiMessage } from '@/services/api'
import { normalizeQuickStartDate } from '@/services/quick-vesting'
import { fetchQuickGrant, interpretQuickPrompt, saveQuickGrant } from '@/services/vesting-service'
import type { AmountType, Grant, Period, PeriodType, VestScheduleEntry } from '@/services/types'

const isBusy = ref(false)
const isInterpreting = ref(false)
const dialogVisible = ref(false)
const dialogSuccess = ref(false)
const dialogTitle = ref('Vesting')
const dialogMessage = ref('')
const helpVisible = ref(false)
const timelineSection = ref<HTMLElement | null>(null)
const promptText = ref('I want a standard four-year time-based vesting schedule with a one-year cliff, monthly after.')
const interpretSummary = ref('')
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

async function generateFromPrompt(): Promise<void> {
  isInterpreting.value = true
  try {
    const response = await interpretQuickPrompt(promptText.value)
    if (response.success === false)
    {
      showDialog(response.message ?? 'Unable to interpret that vesting description yet.', false)
      return
    }

    quickPeriods.value = response.Periods
    quickPeriodTypes.value = response.PeriodTypes
    quickAmountTypes.value = response.AmountTypes
    interpretSummary.value = response.summary ?? 'Built a suggested vesting schedule from your description.'
  } catch (error) {
    showDialog(getApiMessage(error, 'Unable to interpret that vesting description yet.'), false)
  } finally {
    isInterpreting.value = false
  }
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

function scrollToTimeline(): void {
  window.requestAnimationFrame(() => {
    timelineSection.value?.scrollIntoView({ behavior: 'smooth', block: 'start' })
  })
}

function clearTimeline(): void {
  quickVestSchedule.value = []
}
</script>
