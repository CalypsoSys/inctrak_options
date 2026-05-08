<template>
  <section class="card-surface rounded-3xl p-5">
    <div class="mb-4 flex items-center justify-between">
      <div>
        <h3 class="text-lg font-bold text-slate-900">Vesting Periods</h3>
        <p class="text-sm text-[var(--app-muted)]">Build simple or multi-stage schedules with calculated amounts or calculated period counts.</p>
      </div>
      <Button label="Add Period" icon="pi pi-plus" severity="secondary" variant="outlined" @click="$emit('add')" />
    </div>
    <div class="space-y-4">
      <article
        v-for="(period, index) in periods"
        :key="`${period.PERIOD_PK ?? 'new'}-${index}`"
        class="rounded-2xl border border-[var(--app-border)] bg-white/75 p-4"
      >
        <div class="mb-4 flex items-center justify-between">
          <h4 class="font-bold text-slate-900">Period {{ index + 1 }}</h4>
          <Button
            v-if="periods.length > 1"
            label="Remove"
            severity="danger"
            variant="text"
            @click="$emit('remove', index)"
          />
        </div>
        <div class="grid gap-4 md:grid-cols-5">
          <div>
            <label class="field-label">Period Amount</label>
            <input v-model.number="period.PERIOD_AMOUNT" class="field-input" min="1" type="number" />
          </div>
          <div>
            <label class="field-label">Period Duration</label>
            <select v-model.number="period.PERIOD_TYPE_FK" class="field-select">
              <option :value="null">Select duration</option>
              <option v-for="type in periodTypes" :key="type.PERIOD_TYPE_PK" :value="type.PERIOD_TYPE_PK">{{ type.NAME }}</option>
            </select>
          </div>
          <div>
            <label class="field-label">Amount Type</label>
            <select v-model.number="period.AMOUNT_TYPE_FK" class="field-select">
              <option :value="null">Select amount type</option>
              <option v-for="type in amountTypes" :key="type.AMOUNT_TYPE_PK" :value="type.AMOUNT_TYPE_PK">{{ type.NAME }}</option>
            </select>
          </div>
          <div>
            <label class="field-label">Amount</label>
            <input v-model.number="period.AMOUNT" class="field-input" step="0.000001" type="number" :disabled="period.EVEN_OVER_N === 1" />
          </div>
          <div>
            <label class="field-label">Number of Periods</label>
            <input v-model.number="period.INCREMENTS" class="field-input" min="1" type="number" :disabled="period.EVEN_OVER_N === 2" />
          </div>
        </div>
        <div class="mt-4 flex flex-wrap gap-6 text-sm text-[var(--app-muted)]">
          <label class="flex items-center gap-2">
            <input :checked="period.EVEN_OVER_N === 1" type="checkbox" @change="togglePeriodMode(period, 1, $event)" />
            Calculate amounts
          </label>
          <label class="flex items-center gap-2">
            <input :checked="period.EVEN_OVER_N === 2" type="checkbox" @change="togglePeriodMode(period, 2, $event)" />
            Calculate periods
          </label>
        </div>
      </article>
    </div>
  </section>
</template>

<script setup lang="ts">
import Button from 'primevue/button'
import type { AmountType, Period, PeriodType } from '@/services/types'

defineProps<{
  periods: Period[]
  periodTypes: PeriodType[]
  amountTypes: AmountType[]
}>()

defineEmits<{
  (event: 'add'): void
  (event: 'remove', index: number): void
}>()

function togglePeriodMode(period: Period, mode: number, event: Event): void {
  const target = event.target as HTMLInputElement
  period.EVEN_OVER_N = target.checked ? mode : 0
}
</script>
