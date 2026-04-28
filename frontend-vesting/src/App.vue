<template>
  <main class="mx-auto max-w-7xl px-4 py-10 md:px-8">
    <section class="grid gap-8 xl:grid-cols-[1.05fr_0.95fr]">
      <article class="card-surface rounded-[2rem] p-8">
        <PageIntro
          eyebrow="Public Tool"
          title="Quick vesting without the admin shell"
          description="frontend-vesting is a separate public app for vesting.inctrak.com. It talks to the same API, but keeps the experience focused on schedule calculation."
        >
          <template #actions>
            <a class="rounded-full border border-[var(--app-border)] px-4 py-2 text-sm font-semibold text-[var(--app-muted)] transition hover:border-[var(--app-accent)] hover:text-[var(--app-accent)]" href="https://www.inctrak.com" target="_blank" rel="noopener noreferrer">
              Main Site
            </a>
          </template>
        </PageIntro>

        <div class="mt-8 grid gap-4 md:grid-cols-2">
          <div>
            <label class="field-label">Shares Granted</label>
            <input v-model.number="quickGrant.SHARES" class="field-input" min="1" type="number" />
          </div>
          <div>
            <label class="field-label">Vesting Start</label>
            <input v-model="quickGrant.VESTING_START" class="field-input" type="date" />
          </div>
        </div>

        <div class="mt-6">
          <VestingPeriodEditor
            :amount-types="quickAmountTypes"
            :period-types="quickPeriodTypes"
            :periods="quickPeriods"
            @add="addQuickPeriod"
            @remove="removeQuickPeriod"
          />
        </div>

        <div class="mt-5 flex flex-wrap items-center gap-3">
          <Button label="Calculate Vesting" :loading="isBusy" @click="submitQuickGrant" />
          <p class="text-sm text-[var(--app-muted)]">This app stays public and uses the shared API only for quick vesting.</p>
        </div>
      </article>

      <article class="card-surface rounded-[2rem] p-8">
        <h2 class="text-xl font-bold text-slate-900">Why split it out?</h2>
        <div class="mt-5 space-y-4 text-sm leading-7 text-[var(--app-muted)]">
          <p>Keeping vesting.inctrak.com separate means the public tool gets a smaller bundle, cleaner deployment, and no auth or tenant-routing baggage from the main IncTrak workspace.</p>
          <p>Local development also stays simple: the main frontend can live on <code>5174</code>, this app can live on <code>5176</code>, and both can talk to the same backend API on <code>5000</code>.</p>
          <p>The long-term production direction stays clean too: static assets for vesting, static assets for the main frontend, and one shared API host behind them.</p>
        </div>

        <div class="mt-8 grid gap-4 sm:grid-cols-3">
          <div class="rounded-3xl border border-[var(--app-border)] bg-white/70 p-4">
            <p class="text-xs font-bold uppercase tracking-[0.24em] text-[var(--app-accent)]">Port</p>
            <p class="mt-2 text-2xl font-black text-slate-900">5176</p>
            <p class="mt-2 text-sm text-[var(--app-muted)]">Dedicated local dev server</p>
          </div>
          <div class="rounded-3xl border border-[var(--app-border)] bg-white/70 p-4">
            <p class="text-xs font-bold uppercase tracking-[0.24em] text-[var(--app-accent)]">API</p>
            <p class="mt-2 text-2xl font-black text-slate-900">Shared</p>
            <p class="mt-2 text-sm text-[var(--app-muted)]">Still proxies to shared.inctrak.com locally</p>
          </div>
          <div class="rounded-3xl border border-[var(--app-border)] bg-white/70 p-4">
            <p class="text-xs font-bold uppercase tracking-[0.24em] text-[var(--app-accent)]">Hosting</p>
            <p class="mt-2 text-2xl font-black text-slate-900">Static</p>
            <p class="mt-2 text-sm text-[var(--app-muted)]">Ideal for vesting.inctrak.com</p>
          </div>
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
import { fetchQuickGrant, saveQuickGrant } from '@/services/vesting-service'
import type { AmountType, Grant, Period, PeriodType, VestScheduleEntry } from '@/services/types'
import { toDateInputValue } from '@/utils/formatters'

const isBusy = ref(false)
const dialogVisible = ref(false)
const dialogSuccess = ref(false)
const dialogTitle = ref('Vesting')
const dialogMessage = ref('')

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
    VESTING_START: toDateInputValue(quickData.Grant.VESTING_START)
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
