<template>
  <section class="card-surface rounded-3xl p-5">
    <div class="flex flex-wrap items-center justify-between gap-4">
      <div>
        <h3 class="text-lg font-bold text-slate-900">Vesting Timeline</h3>
        <p class="text-sm text-[var(--app-muted)]">Inspect each vesting event or focus only on vested rows.</p>
      </div>
      <label class="flex items-center gap-2 text-sm text-[var(--app-muted)]">
        <input v-model="showVestedOnly" type="checkbox" />
        Show vested only
      </label>
    </div>
    <div class="mt-4 overflow-x-auto">
      <table class="app-table">
        <thead>
          <tr>
            <th>Period</th>
            <th>Vest Date</th>
            <th>Status</th>
            <th>Percent</th>
            <th>Shares</th>
            <th>Total Percent</th>
            <th>Total Shares</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="entry in filteredRows"
            :key="`${entry.Order}-${entry.VestDate}`"
            :class="entry.IsVested ? 'bg-emerald-50/60' : 'bg-amber-50/50'"
          >
            <td>{{ entry.Order }}</td>
            <td>{{ formatDate(entry.VestDate) }}</td>
            <td>
              <span
                class="inline-flex rounded-full px-3 py-1 text-xs font-bold uppercase tracking-[0.18em]"
                :class="entry.IsVested
                  ? 'bg-emerald-100 text-emerald-800'
                  : 'bg-amber-100 text-amber-800'"
              >
                {{ entry.IsVested ? 'Vested' : 'Unvested' }}
              </span>
            </td>
            <td>{{ formatNumber(entry.Percent, 3) }}%</td>
            <td>{{ formatNumber(entry.Shares, 3) }}</td>
            <td>{{ formatNumber(entry.TotalPercent, 3) }}</td>
            <td>{{ formatNumber(entry.TotalShares, 3) }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </section>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import type { VestScheduleEntry } from '@/services/types'
import { formatDate, formatNumber } from '@/utils/formatters'

const props = defineProps<{
  rows: VestScheduleEntry[]
}>()

const showVestedOnly = ref(false)

const filteredRows = computed(() =>
  props.rows.filter((entry) => (showVestedOnly.value ? entry.IsVested : true))
)
</script>
