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
      <section class="grid gap-6 xl:grid-cols-[1.05fr_0.95fr]">
        <article class="card-surface rounded-[2rem] p-8">
          <PageIntro
            eyebrow="Public"
            title="Equity operations without spreadsheet drift"
            description="Give finance, legal, and people teams a calmer way to manage grants, plans, participant access, and vesting from one shared workspace."
          />
          <div class="mt-8 grid gap-4 md:grid-cols-3">
            <StatCard
              label="Administrator Workflows"
              value="Plans"
              hint="Set up plans, classes, holders, grants, schedules, and participant records."
            />
            <StatCard
              label="Participant Access"
              value="Grants"
              hint="Give participants a clean place to review equity details and vesting progress."
            />
            <StatCard
              label="Public Entry Points"
              value="Signup"
              hint="Launch a workspace or run the standalone vesting calculator without entering the admin shell."
            />
          </div>
        </article>

        <article class="card-surface rounded-[2rem] p-8">
          <p class="text-xs font-bold uppercase tracking-[0.3em] text-[var(--app-accent)]">Standalone Tool</p>
          <h3 class="mt-3 text-3xl font-black tracking-tight text-slate-900">Public vesting calculator</h3>
          <p class="mt-4 max-w-xl text-sm leading-7 text-[var(--app-muted)] md:text-base">
            Run quick vesting scenarios in the dedicated public calculator built for founders, operators, counsel, and participants. It stays separate from the admin workspace, so people can explore schedules without needing an account first.
          </p>
          <div class="mt-8 grid gap-4 sm:grid-cols-3">
            <div class="rounded-3xl border border-[var(--app-border)] bg-white/70 p-4">
              <p class="text-xs font-bold uppercase tracking-[0.24em] text-[var(--app-muted)]">Fast</p>
              <p class="mt-3 text-lg font-bold text-slate-900">Plain-English setup</p>
              <p class="mt-2 text-sm leading-6 text-[var(--app-muted)]">Start from a standard vesting phrase or build each period step by step.</p>
            </div>
            <div class="rounded-3xl border border-[var(--app-border)] bg-white/70 p-4">
              <p class="text-xs font-bold uppercase tracking-[0.24em] text-[var(--app-muted)]">Flexible</p>
              <p class="mt-3 text-lg font-bold text-slate-900">Standalone scenarios</p>
              <p class="mt-2 text-sm leading-6 text-[var(--app-muted)]">Model cliffs, quarterly timelines, monthly vesting, and custom period mixes without logging in.</p>
            </div>
            <div class="rounded-3xl border border-[var(--app-border)] bg-white/70 p-4">
              <p class="text-xs font-bold uppercase tracking-[0.24em] text-[var(--app-muted)]">Connected</p>
              <p class="mt-3 text-lg font-bold text-slate-900">Contact the team</p>
              <p class="mt-2 text-sm leading-6 text-[var(--app-muted)]">Reach out directly from the calculator when you want help tailoring a vesting schedule.</p>
            </div>
          </div>
          <div class="mt-8 flex flex-wrap gap-3">
            <a
              :href="vestingAppUrl"
              class="inline-flex items-center justify-center rounded-2xl bg-[var(--app-accent)] px-5 py-3 text-sm font-semibold text-white transition hover:brightness-110"
            >
              Open Vesting Calculator
            </a>
            <RouterLink
              :to="{ name: 'contact' }"
              class="inline-flex items-center justify-center rounded-2xl border border-[var(--app-border)] px-5 py-3 text-sm font-semibold text-slate-900 transition hover:border-[var(--app-accent)] hover:text-[var(--app-accent)]"
            >
              Talk With IncTrak
            </RouterLink>
          </div>
          <div class="mt-8 rounded-[1.75rem] border border-[var(--app-border)] bg-slate-950/[0.03] p-5">
            <p class="text-sm font-semibold text-slate-900">Best fit for this public experience</p>
            <div class="mt-4 grid gap-3 md:grid-cols-2">
              <div class="rounded-2xl bg-white/80 p-4">
                <p class="font-semibold text-slate-900">Exploring a new vesting idea</p>
                <p class="mt-2 text-sm leading-6 text-[var(--app-muted)]">Model a standard four-year schedule, a quarterly timeline, or a more tailored vesting structure in minutes.</p>
              </div>
              <div class="rounded-2xl bg-white/80 p-4">
                <p class="font-semibold text-slate-900">Preparing for a real workspace</p>
                <p class="mt-2 text-sm leading-6 text-[var(--app-muted)]">When the rollout window opens, move from public planning into a branded company workspace for day-to-day equity administration.</p>
              </div>
            </div>
          </div>
        </article>
      </section>

      <section class="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
        <article class="card-surface overflow-hidden rounded-[2rem] border border-[var(--app-border)] bg-[linear-gradient(145deg,rgba(255,255,255,0.94),rgba(255,248,236,0.92))] p-6 xl:col-span-3">
          <div class="grid gap-6 lg:grid-cols-[0.9fr_1.1fr] lg:items-center">
            <div>
              <p class="text-xs font-bold uppercase tracking-[0.3em] text-[var(--app-accent)]">Why teams switch</p>
              <h3 class="mt-3 text-2xl font-black tracking-tight text-slate-900 md:text-3xl">A steadier operating system for equity administration</h3>
              <p class="mt-4 text-sm leading-7 text-[var(--app-muted)] md:text-base">
                IncTrak gives growing companies one place to move from draft vesting ideas to real plan administration, participant access, and grant visibility without living in disconnected spreadsheets.
              </p>
            </div>
            <div class="grid gap-4 md:grid-cols-3">
              <div class="rounded-3xl border border-[var(--app-border)] bg-white/75 p-5">
                <p class="text-xs font-bold uppercase tracking-[0.24em] text-[var(--app-muted)]">Clarity</p>
                <p class="mt-3 text-lg font-bold text-slate-900">Shared source of truth</p>
                <p class="mt-2 text-sm leading-6 text-[var(--app-muted)]">Keep plans, grants, participants, and vesting events aligned in one system.</p>
              </div>
              <div class="rounded-3xl border border-[var(--app-border)] bg-white/75 p-5">
                <p class="text-xs font-bold uppercase tracking-[0.24em] text-[var(--app-muted)]">Control</p>
                <p class="mt-3 text-lg font-bold text-slate-900">Purpose-built admin flows</p>
                <p class="mt-2 text-sm leading-6 text-[var(--app-muted)]">Support the real operational work behind option plans, participant records, and grants.</p>
              </div>
              <div class="rounded-3xl border border-[var(--app-border)] bg-white/75 p-5">
                <p class="text-xs font-bold uppercase tracking-[0.24em] text-[var(--app-muted)]">Access</p>
                <p class="mt-3 text-lg font-bold text-slate-900">Cleaner participant views</p>
                <p class="mt-2 text-sm leading-6 text-[var(--app-muted)]">Give employees and stakeholders a straightforward view of their equity details.</p>
              </div>
            </div>
          </div>
        </article>

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
import { computed, onMounted, ref } from 'vue'
import AppDialog from '@/components/AppDialog.vue'
import PageIntro from '@/components/PageIntro.vue'
import StatCard from '@/components/StatCard.vue'
import { useAsyncState } from '@/composables/useAsyncState'
import { fetchAdminSummary, fetchOptioneeSummary, type AdminSummaryResponse, type OptioneeSummaryResponse } from '@/services/dashboard-service'
import { buildVestingAppUrl } from '@/services/runtime-config'
import { useAuthStore } from '@/stores/auth'
import { formatNumber } from '@/utils/formatters'
import companyDashImage from '@/assets/images/company_dash.png'
import companyGrantImage from '@/assets/images/company_grant.png'
import companyScheduleImage from '@/assets/images/company_schedule.png'
import optioneeDashImage from '@/assets/images/optionee_dash.png'
import optioneeGrantImage from '@/assets/images/optionee_grants.png'
import optioneeSummaryImage from '@/assets/images/optionee_summary.png'

const authStore = useAuthStore()
const { dialogVisible, isSuccess, message } = useAsyncState()

const adminSummary = ref<AdminSummaryResponse | null>(null)
const optioneeSummary = ref<OptioneeSummaryResponse | null>(null)
const vestingAppUrl = buildVestingAppUrl()

const featureCards = [
  { eyebrow: 'Admin', title: 'Company Dashboard', description: 'See plan capacity, participant counts, grant activity, and workspace momentum from a single operational view.', image: companyDashImage },
  { eyebrow: 'Participant', title: 'Optionee Dashboard', description: 'Give participants a cleaner place to understand granted, vested, and unvested equity without spreadsheet handoffs.', image: optioneeDashImage },
  { eyebrow: 'Planning', title: 'Vesting Schedules', description: 'Design standard or multi-stage vesting schedules with cliffs, stages, and calculated periods that stay tied to the underlying record.', image: companyScheduleImage },
  { eyebrow: 'Grants', title: 'Grant Details', description: 'Inspect grant dates, vesting structure, termination rules, and timeline events from one detailed grant record.', image: optioneeGrantImage },
  { eyebrow: 'Operations', title: 'Grant Administration', description: 'Issue grants, maintain participant records, and keep day-to-day equity operations moving from one shared workspace.', image: companyGrantImage },
  { eyebrow: 'Summary', title: 'Participant Summaries', description: 'Break granted, vested, and unvested balances down by plan so every participant view stays grounded in the same source data.', image: optioneeSummaryImage }
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
  }
})
</script>
