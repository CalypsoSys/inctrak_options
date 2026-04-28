<template>
  <section class="space-y-6">
    <PageIntro eyebrow="Admin" title="Plans" description="Attach plans to stock classes and track total allocated versus granted shares." />
    <div class="grid gap-6 xl:grid-cols-[0.95fr_1.05fr]">
      <article class="card-surface overflow-hidden rounded-[2rem] p-6">
        <div class="mb-4 flex items-center justify-between">
          <h3 class="text-lg font-bold text-slate-900">All plans</h3>
          <Button label="New" icon="pi pi-plus" @click="loadItem(EMPTY_GUID)" />
        </div>
        <div class="overflow-x-auto">
          <table class="app-table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Stock Class</th>
                <th>Total Shares</th>
                <th>Granted</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="item in items" :key="item.PLAN_PK">
                <td><button class="font-semibold text-[var(--app-accent)]" @click="loadItem(item.PLAN_PK)">{{ item.NAME }}</button></td>
                <td>{{ item.STOCK_CLASS_NAME }}</td>
                <td>{{ formatNumber(item.TOTAL_SHARES) }}</td>
                <td>{{ formatNumber(item.GRANTED_SHARES ?? 0) }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </article>

      <form class="card-surface rounded-[2rem] p-6" @submit.prevent="saveItem">
        <h3 class="text-lg font-bold text-slate-900">{{ currentId ? 'Edit plan' : 'Create plan' }}</h3>
        <div class="mt-5 space-y-5">
          <div>
            <label class="field-label">Name</label>
            <input v-model="form.NAME" class="field-input" type="text" />
          </div>
          <div>
            <label class="field-label">Stock Class</label>
            <select v-model="form.STOCK_CLASS_FK" class="field-select">
              <option value="">Select a stock class</option>
              <option v-for="stockClass in stockClasses" :key="stockClass.STOCK_CLASS_PK" :value="stockClass.STOCK_CLASS_PK">{{ stockClass.NAME }}</option>
            </select>
          </div>
          <div>
            <label class="field-label">Total Shares</label>
            <input v-model.number="form.TOTAL_SHARES" class="field-input" min="1" type="number" />
          </div>
        </div>
        <FormActions
          :busy="isBusy"
          :show-delete="Boolean(currentId)"
          :show-cancel="Boolean(currentId)"
          submit-label="Save Plan"
          @cancel="loadItem(EMPTY_GUID)"
          @delete="removeItem"
        />
      </form>
    </div>

    <AppDialog v-model:visible="dialogVisible" title="Plans" :message="message" :success="isSuccess" />
  </section>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Button from 'primevue/button'
import AppDialog from '@/components/AppDialog.vue'
import FormActions from '@/components/FormActions.vue'
import PageIntro from '@/components/PageIntro.vue'
import { useAsyncState } from '@/composables/useAsyncState'
import { deletePlan, fetchPlan, fetchPlans, savePlan } from '@/services/admin-service'
import { getApiMessage } from '@/services/api'
import type { Plan, StockClass } from '@/services/types'
import { EMPTY_GUID } from '@/utils/constants'
import { formatNumber } from '@/utils/formatters'
import { validatePlan } from '@/utils/validators'

const route = useRoute()
const router = useRouter()
const { isBusy, dialogVisible, isSuccess, message, showMessage } = useAsyncState()

const items = ref<Plan[]>([])
const stockClasses = ref<StockClass[]>([])
const currentId = ref('')
const form = reactive<Plan>({
  PLAN_PK: EMPTY_GUID,
  NAME: '',
  STOCK_CLASS_FK: '',
  TOTAL_SHARES: 0
})

onMounted(async () => {
  await refreshItems()
  const routeId = typeof route.params.id === 'string' && route.params.id ? route.params.id : ''
  await loadItem(routeId || EMPTY_GUID)
})

async function refreshItems(): Promise<void> {
  items.value = await fetchPlans()
}

async function loadItem(id = ''): Promise<void> {
  currentId.value = id && id !== EMPTY_GUID ? id : ''
  if (!id) {
    Object.assign(form, { PLAN_PK: EMPTY_GUID, NAME: '', STOCK_CLASS_FK: '', TOTAL_SHARES: 0 })
    await router.replace({ name: 'plans' })
    return
  }

  const response = await fetchPlan(id)
  stockClasses.value = response.StockClasses
  Object.assign(form, response.Plan)
  await router.replace({ name: 'plans', params: { id } })
}

async function saveItem(): Promise<void> {
  const errors = validatePlan(form)
  if (errors.length > 0) {
    showMessage(errors.join(' '), false)
    return
  }

  isBusy.value = true
  try {
    const response = await savePlan({
      Key: currentId.value || EMPTY_GUID,
      UUID: EMPTY_GUID,
      Data: { ...form }
    })
    showMessage(response.message ?? 'Plan saved.', response.success !== false)
    await refreshItems()
    await loadItem(response.key)
  } catch (error) {
    showMessage(getApiMessage(error, 'Unable to save the plan.'), false)
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
    const response = await deletePlan(currentId.value)
    showMessage(response.message ?? 'Plan removed.', response.success !== false)
    items.value = response.Plans
    await loadItem(EMPTY_GUID)
  } catch (error) {
    showMessage(getApiMessage(error, 'Unable to remove the plan.'), false)
  } finally {
    isBusy.value = false
  }
}
</script>
