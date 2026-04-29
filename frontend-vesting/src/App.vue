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

    <section v-if="quickVestSchedule.length > 0" class="mt-8">
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

        <div class="flex justify-end">
          <Button label="Close" @click="helpVisible = false" />
        </div>
      </div>
    </Dialog>
  </main>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'
import Message from 'primevue/message'
import PageIntro from '@/components/PageIntro.vue'
import VestingPeriodEditor from '@/components/VestingPeriodEditor.vue'
import VestingScheduleTable from '@/components/VestingScheduleTable.vue'
import { getApiMessage } from '@/services/api'
import { normalizeQuickStartDate } from '@/services/quick-vesting'
import { fetchQuickGrant, saveQuickGrant } from '@/services/vesting-service'
import type { AmountType, Grant, Period, PeriodType, VestScheduleEntry } from '@/services/types'

const isBusy = ref(false)
const dialogVisible = ref(false)
const dialogSuccess = ref(false)
const dialogTitle = ref('Vesting')
const dialogMessage = ref('')
const helpVisible = ref(false)

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

async function submitQuickGrant(): Promise<void> {
  isBusy.value = true
  try {
    const response = await saveQuickGrant(quickGrant, quickPeriods.value)
    quickVestSchedule.value = response.VestSchedule ?? []
    showDialog(response.message ?? 'Quick vesting calculated.', response.success !== false)
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
</script>
