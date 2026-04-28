<template>
  <section class="space-y-6">
    <PageIntro eyebrow="Admin" title="Vesting Schedules" description="Design reusable vesting schedules with repeatable periods and calculated stages." />
    <div class="grid gap-6 xl:grid-cols-[0.9fr_1.1fr]">
      <article class="card-surface overflow-hidden rounded-[2rem] p-6">
        <div class="mb-4 flex items-center justify-between">
          <h3 class="text-lg font-bold text-slate-900">All schedules</h3>
          <Button label="New" icon="pi pi-plus" @click="loadItem(EMPTY_GUID)" />
        </div>
        <div class="overflow-x-auto">
          <table class="app-table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Periods</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="item in items" :key="item.SCHEDULE_PK">
                <td><button class="font-semibold text-[var(--app-accent)]" @click="loadItem(item.SCHEDULE_PK)">{{ item.NAME }}</button></td>
                <td>{{ formatNumber(item.PERIODS ?? 0) }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </article>

      <form class="space-y-6" @submit.prevent="saveItem">
        <article class="card-surface rounded-[2rem] p-6">
          <h3 class="text-lg font-bold text-slate-900">{{ currentId ? 'Edit schedule' : 'Create schedule' }}</h3>
          <div class="mt-5">
            <label class="field-label">Name</label>
            <input v-model="form.NAME" class="field-input" type="text" />
          </div>
          <FormActions
            :busy="isBusy"
            :show-delete="Boolean(currentId)"
            :show-cancel="Boolean(currentId)"
            submit-label="Save Schedule"
            @cancel="loadItem(EMPTY_GUID)"
            @delete="removeItem"
          />
        </article>

        <VestingPeriodEditor
          :amount-types="amountTypes"
          :period-types="periodTypes"
          :periods="periods"
          @add="addPeriod"
          @remove="removePeriod"
        />
      </form>
    </div>

    <AppDialog v-model:visible="dialogVisible" title="Vesting Schedules" :message="message" :success="isSuccess" />
  </section>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Button from 'primevue/button'
import AppDialog from '@/components/AppDialog.vue'
import FormActions from '@/components/FormActions.vue'
import PageIntro from '@/components/PageIntro.vue'
import VestingPeriodEditor from '@/components/VestingPeriodEditor.vue'
import { useAsyncState } from '@/composables/useAsyncState'
import { deleteSchedule, fetchSchedule, fetchSchedules, saveSchedule } from '@/services/admin-service'
import { getApiMessage } from '@/services/api'
import type { AmountType, Period, PeriodType, Schedule } from '@/services/types'
import { EMPTY_GUID } from '@/utils/constants'
import { formatNumber } from '@/utils/formatters'
import { validateSchedule } from '@/utils/validators'

const route = useRoute()
const router = useRouter()
const { isBusy, dialogVisible, isSuccess, message, showMessage } = useAsyncState()

const items = ref<Schedule[]>([])
const currentId = ref('')
const form = reactive<Schedule>({
  SCHEDULE_PK: EMPTY_GUID,
  NAME: ''
})
const periods = ref<Period[]>([])
const periodTypes = ref<PeriodType[]>([])
const amountTypes = ref<AmountType[]>([])

onMounted(async () => {
  await refreshItems()
  const routeId = typeof route.params.id === 'string' && route.params.id ? route.params.id : ''
  await loadItem(routeId || EMPTY_GUID)
})

async function refreshItems(): Promise<void> {
  items.value = await fetchSchedules()
}

async function loadItem(id = ''): Promise<void> {
  currentId.value = id && id !== EMPTY_GUID ? id : ''
  if (!id) {
    Object.assign(form, { SCHEDULE_PK: EMPTY_GUID, NAME: '' })
    periods.value = [{
      PERIOD_AMOUNT: 1,
      PERIOD_TYPE_FK: null,
      AMOUNT_TYPE_FK: null,
      AMOUNT: 0,
      INCREMENTS: 1,
      ORDER: 0,
      EVEN_OVER_N: 0
    }]
    await router.replace({ name: 'vesting-schedules' })
    return
  }

  const response = await fetchSchedule(id)
  Object.assign(form, response.Schedule)
  periods.value = response.Periods
  periodTypes.value = response.PeriodTypes
  amountTypes.value = response.AmountTypes
  await router.replace({ name: 'vesting-schedules', params: { id } })
}

function addPeriod(): void {
  periods.value.push({
    PERIOD_AMOUNT: 1,
    PERIOD_TYPE_FK: null,
    AMOUNT_TYPE_FK: null,
    AMOUNT: 0,
    INCREMENTS: 1,
    ORDER: periods.value.length,
    EVEN_OVER_N: 0
  })
}

function removePeriod(index: number): void {
  periods.value.splice(index, 1)
}

async function saveItem(): Promise<void> {
  const errors = validateSchedule(form, periods.value)
  if (errors.length > 0) {
    showMessage(errors.join(' '), false)
    return
  }

  isBusy.value = true
  try {
    const response = await saveSchedule({
      Key: currentId.value || EMPTY_GUID,
      UUID: EMPTY_GUID,
      Data: { ...form },
      Children: periods.value.map((period, index) => ({ ...period, ORDER: index }))
    })
    showMessage(response.message ?? 'Schedule saved.', response.success !== false)
    await refreshItems()
    await loadItem(response.key)
  } catch (error) {
    showMessage(getApiMessage(error, 'Unable to save the schedule.'), false)
  } finally {
    isBusy.value = false
  }
}

async function removeItem(): Promise<void> {
  if (!currentId.value) {
    return
  }

  isBusy.value = true
  try {
    const response = await deleteSchedule(currentId.value)
    showMessage(response.message ?? 'Schedule removed.', response.success !== false)
    items.value = response.Schedules
    await loadItem(EMPTY_GUID)
  } catch (error) {
    showMessage(getApiMessage(error, 'Unable to remove the schedule.'), false)
  } finally {
    isBusy.value = false
  }
}
</script>
