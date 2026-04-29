<template>
  <section class="card-surface rounded-3xl p-5">
    <div class="mb-4 flex items-center justify-between">
      <div>
        <h3 class="text-lg font-bold text-slate-900">Vesting Periods</h3>
        <p class="text-sm text-[var(--app-muted)]">Describe each vesting step in plain pieces: how long it lasts, what vests each time, and how many times it repeats.</p>
      </div>
      <div class="flex flex-wrap gap-2">
        <Button label="How To Use This" icon="pi pi-question-circle" severity="contrast" variant="text" @click="$emit('help')" />
        <Button label="Add Period" icon="pi pi-plus" severity="secondary" variant="outlined" @click="$emit('add')" />
      </div>
    </div>
    <div class="space-y-4">
      <article
        v-for="(period, index) in periods"
        :key="`${period.PERIOD_PK ?? 'new'}-${index}`"
        class="rounded-2xl border border-[var(--app-border)] bg-white/75 p-4"
      >
        <div class="mb-4 flex items-center justify-between">
          <h4 class="font-bold text-slate-900">Period {{ index + 1 }}</h4>
          <Button v-if="periods.length > 1" label="Remove" severity="danger" variant="text" @click="$emit('remove', index)" />
        </div>
        <div class="grid gap-4 md:grid-cols-5">
          <div>
            <label class="field-label">Length of Each Step</label>
            <input v-model.number="period.PERIOD_AMOUNT" class="field-input" min="1" type="number" />
            <p class="mt-2 text-xs leading-5 text-[var(--app-muted)]">Example: enter <strong>1</strong> for every month, or <strong>12</strong> for every 12 months.</p>
          </div>
          <div>
            <label class="field-label">Step Unit</label>
            <select v-model.number="period.PERIOD_TYPE_FK" class="field-select">
              <option :value="null">Select duration</option>
              <option v-for="type in periodTypes" :key="type.PERIOD_TYPE_PK" :value="type.PERIOD_TYPE_PK">{{ type.NAME }}</option>
            </select>
            <p class="mt-2 text-xs leading-5 text-[var(--app-muted)]">Choose whether the step repeats in years, months, weeks, or days.</p>
          </div>
          <div>
            <label class="field-label">Vests In</label>
            <select v-model.number="period.AMOUNT_TYPE_FK" class="field-select">
              <option :value="null">Select amount type</option>
              <option v-for="type in amountTypes" :key="type.AMOUNT_TYPE_PK" :value="type.AMOUNT_TYPE_PK">{{ type.NAME }}</option>
            </select>
            <p class="mt-2 text-xs leading-5 text-[var(--app-muted)]">Use <strong>Shares</strong> for fixed share counts or <strong>Percentage</strong> for percent-based vesting.</p>
          </div>
          <div>
            <label class="field-label">Amount Per Step</label>
            <input v-model.number="period.AMOUNT" class="field-input" step="0.000001" type="number" :disabled="period.EVEN_OVER_N === 1" />
            <p class="mt-2 text-xs leading-5 text-[var(--app-muted)]">What vests each time this step happens.</p>
          </div>
          <div>
            <label class="field-label">Number of Steps</label>
            <input v-model.number="period.INCREMENTS" class="field-input" min="1" type="number" :disabled="period.EVEN_OVER_N === 2" />
            <p class="mt-2 text-xs leading-5 text-[var(--app-muted)]">How many times this step repeats before the next period starts.</p>
          </div>
        </div>
        <div class="mt-4 rounded-2xl border border-[var(--app-border)] bg-white/60 p-4">
          <p class="text-xs font-bold uppercase tracking-[0.24em] text-[var(--app-accent)]">Helpful shortcuts</p>
          <div class="mt-3 flex flex-wrap gap-6 text-sm text-[var(--app-muted)]">
          <label class="flex items-center gap-2">
            <input :checked="period.EVEN_OVER_N === 1" type="checkbox" @change="togglePeriodMode(period, 1, $event)" />
            Split the total evenly across the number of steps
          </label>
          <label class="flex items-center gap-2">
            <input :checked="period.EVEN_OVER_N === 2" type="checkbox" @change="togglePeriodMode(period, 2, $event)" />
            Figure out the number of steps automatically from the amount
          </label>
          </div>
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
  (event: 'help'): void
  (event: 'remove', index: number): void
}>()

function togglePeriodMode(period: Period, mode: number, event: Event): void {
  const target = event.target as HTMLInputElement
  period.EVEN_OVER_N = target.checked ? mode : 0
}
</script>
