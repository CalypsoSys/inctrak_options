<template>
  <section class="space-y-6">
    <template v-if="authStore.isAdmin">
      <PageIntro
        eyebrow="Admin Dashboard"
        title="Company summary"
        description="Keep the same equity-management features, but in a cleaner route model and component structure."
      />
      <div class="grid gap-4 md:grid-cols-4">
        <StatCard label="Plans" :value="formatNumber(adminSummary?.Plans.length ?? 0)" />
        <StatCard label="Schedules" :value="formatNumber(adminSummary?.Schedules ?? 0)" />
        <StatCard label="Participants" :value="formatNumber(adminSummary?.Participants ?? 0)" />
        <StatCard label="Grants" :value="formatNumber(adminSummary?.Grants ?? 0)" />
      </div>
      <article class="card-surface overflow-hidden rounded-[2rem] p-6">
        <h3 class="text-lg font-bold text-slate-900">Plan allocation snapshot</h3>
        <div class="mt-4 overflow-x-auto">
          <table class="app-table">
            <thead>
              <tr>
                <th>Plan</th>
                <th>Total Shares</th>
                <th>Granted</th>
                <th>Remaining</th>
                <th>Grants</th>
                <th>Participants</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="plan in adminSummary?.Plans ?? []" :key="plan.NAME">
                <td>{{ plan.NAME }}</td>
                <td>{{ formatNumber(plan.TOTAL_SHARES) }}</td>
                <td>{{ formatNumber(plan.GRANTED_SHARES) }}</td>
                <td>{{ formatNumber(plan.TOTAL_SHARES - plan.GRANTED_SHARES) }}</td>
                <td>{{ formatNumber(plan.GRANTS) }}</td>
                <td>{{ formatNumber(plan.PARTICIPANTS) }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </article>
    </template>

    <template v-else-if="authStore.isOptionee">
      <PageIntro
        eyebrow="Participant Dashboard"
        title="Grant summary"
        description="Track total granted, vested, and unvested shares without leaving the SPA."
      />
      <div class="grid gap-4 md:grid-cols-3">
        <StatCard label="Total Granted" :value="formatNumber(optioneeSummary?.TotalGranted ?? 0)" />
        <StatCard label="Total Vested" :value="formatNumber(optioneeSummary?.TotalVested ?? 0, 3)" />
        <StatCard label="Total Unvested" :value="formatNumber(optioneeSummary?.TotalUnVested ?? 0, 3)" />
      </div>
      <article class="card-surface rounded-[2rem] p-6">
        <h3 class="text-lg font-bold text-slate-900">Vesting over time</h3>
        <div class="mt-4 space-y-4">
          <div v-for="group in optioneeOverTimeRows" :key="group.label" class="rounded-2xl border border-[var(--app-border)] bg-white/60 p-4">
            <p class="font-semibold text-slate-900">{{ group.label }}</p>
            <div class="mt-3 grid gap-2 sm:grid-cols-2 xl:grid-cols-4">
              <div v-for="entry in group.entries" :key="`${group.label}-${entry.date}`" class="rounded-2xl bg-slate-950/4 px-3 py-2 text-sm">
                <span class="font-semibold text-slate-900">{{ entry.date }}</span>
                <span class="ml-2 text-[var(--app-muted)]">{{ formatNumber(entry.value, 3) }}</span>
              </div>
            </div>
          </div>
        </div>
      </article>
    </template>

    <template v-else>
      <section class="grid gap-6 xl:grid-cols-[1.1fr_0.9fr]">
        <article class="card-surface rounded-[2rem] p-8">
          <PageIntro
            eyebrow="Public"
            title="Equity operations without spreadsheet drift"
            description="The frontend is now a Vue 3 + TypeScript + Vite SPA, but it still exposes the same IncTrak features: quick vesting calculations, admin operations, and participant visibility."
          />
          <div class="mt-8 grid gap-4 md:grid-cols-3">
            <StatCard label="Admin Tools" value="7" hint="Stock classes, holders, plans, schedules, terminations, participants, grants" />
            <StatCard label="Participant Views" value="3" hint="Stocks, options, and grant detail pages" />
            <StatCard label="Auth Flows" value="5" hint="Login, register, activate, reset password, accept terms" />
          </div>
        </article>

        <article class="card-surface rounded-[2rem] p-8">
          <h3 class="text-lg font-bold text-slate-900">Quick vesting schedule</h3>
          <p class="mt-2 text-sm text-[var(--app-muted)]">Run the same quick calculation flow from the old home page, now inside the SPA.</p>
          <div class="mt-6 grid gap-4 md:grid-cols-2">
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
          <div class="mt-5">
            <Button label="Calculate Vesting" :loading="isBusy" @click="submitQuickGrant" />
          </div>
        </article>
      </section>

      <VestingScheduleTable v-if="quickVestSchedule.length > 0" :rows="quickVestSchedule" />

      <section class="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
        <article v-for="card in featureCards" :key="card.title" class="card-surface overflow-hidden rounded-[2rem]">
          <img :src="card.image" :alt="card.title" class="h-56 w-full object-cover" />
          <div class="p-5">
            <p class="text-xs font-bold uppercase tracking-[0.24em] text-[var(--app-accent)]">{{ card.eyebrow }}</p>
            <h3 class="mt-2 text-lg font-bold text-slate-900">{{ card.title }}</h3>
            <p class="mt-2 text-sm leading-6 text-[var(--app-muted)]">{{ card.description }}</p>
          </div>
        </article>
      </section>
    </template>

    <AppDialog
      v-model:visible="dialogVisible"
      title="Dashboard"
      :message="message"
      :success="isSuccess"
    />
  </section>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import Button from 'primevue/button'
import AppDialog from '@/components/AppDialog.vue'
import PageIntro from '@/components/PageIntro.vue'
import StatCard from '@/components/StatCard.vue'
import VestingPeriodEditor from '@/components/VestingPeriodEditor.vue'
import VestingScheduleTable from '@/components/VestingScheduleTable.vue'
import { useAsyncState } from '@/composables/useAsyncState'
import { fetchAdminSummary, fetchOptioneeSummary, fetchQuickGrant, saveQuickGrant, type AdminSummaryResponse, type OptioneeSummaryResponse } from '@/services/dashboard-service'
import { getApiMessage } from '@/services/api'
import type { AmountType, Grant, Period, PeriodType, VestScheduleEntry } from '@/services/types'
import { useAuthStore } from '@/stores/auth'
import { formatNumber, toDateInputValue } from '@/utils/formatters'
import companyDashImage from '@/assets/images/company_dash.png'
import companyGrantImage from '@/assets/images/company_grant.png'
import companyScheduleImage from '@/assets/images/company_schedule.png'
import optioneeDashImage from '@/assets/images/optionee_dash.png'
import optioneeGrantImage from '@/assets/images/optionee_grants.png'
import optioneeSummaryImage from '@/assets/images/optionee_summary.png'

const authStore = useAuthStore()
const { isBusy, dialogVisible, isSuccess, message, showMessage } = useAsyncState()

const adminSummary = ref<AdminSummaryResponse | null>(null)
const optioneeSummary = ref<OptioneeSummaryResponse | null>(null)
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

const featureCards = [
  { eyebrow: 'Admin', title: 'Company Dashboard', description: 'See plans, remaining shares, participant counts, and grant activity from a single view.', image: companyDashImage },
  { eyebrow: 'Participant', title: 'Optionee Dashboard', description: 'Give participants a clean view of granted, vested, and unvested equity data.', image: optioneeDashImage },
  { eyebrow: 'Planning', title: 'Vesting Schedules', description: 'Create simple or complex vesting timelines with cliffs, stages, and computed periods.', image: companyScheduleImage },
  { eyebrow: 'Grants', title: 'Grant Details', description: 'Inspect grant dates, vesting schedules, termination rules, and grant-level vesting events.', image: optioneeGrantImage },
  { eyebrow: 'Operations', title: 'Issue Grants', description: 'Assign grants to participants and keep administrator workflows in one place.', image: companyGrantImage },
  { eyebrow: 'Summary', title: 'Option Summary', description: 'Break granted, vested, and unvested balances down by plan for each participant.', image: optioneeSummaryImage }
]

const optioneeOverTimeRows = computed(() =>
  Object.entries(optioneeSummary.value?.OverTime ?? {}).map(([label, values]) => ({
    label,
    entries: Object.entries(values).map(([date, value]) => ({ date, value }))
  }))
)

onMounted(async () => {
  if (authStore.isAdmin) {
    adminSummary.value = await fetchAdminSummary()
    return
  }

  if (authStore.isOptionee) {
    optioneeSummary.value = await fetchOptioneeSummary()
    return
  }

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
    showMessage(response.message ?? 'Quick vesting calculated.', response.success !== false)
  } catch (error) {
    showMessage(getApiMessage(error, 'Unable to calculate the quick vesting schedule.'), false)
  } finally {
    isBusy.value = false
  }
}
</script>
