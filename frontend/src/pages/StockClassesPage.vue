<template>
  <section class="space-y-6">
    <PageIntro eyebrow="Admin" title="Stock Classes" description="Create and maintain capitalization buckets used by plans and stock holders." />
    <div class="grid gap-6 xl:grid-cols-[0.95fr_1.05fr]">
      <article class="card-surface overflow-hidden rounded-[2rem] p-6">
        <div class="mb-4 flex items-center justify-between">
          <h3 class="text-lg font-bold text-slate-900">All stock classes</h3>
          <Button label="New" icon="pi pi-plus" @click="loadItem()" />
        </div>
        <div class="overflow-x-auto">
          <table class="app-table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Total Shares</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="item in items" :key="item.STOCK_CLASS_PK">
                <td><button class="font-semibold text-[var(--app-accent)]" @click="loadItem(item.STOCK_CLASS_PK)">{{ item.NAME }}</button></td>
                <td>{{ formatNumber(item.TOTAL_SHARES) }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </article>

      <form class="card-surface rounded-[2rem] p-6" @submit.prevent="saveItem">
        <h3 class="text-lg font-bold text-slate-900">{{ currentId ? 'Edit stock class' : 'Create stock class' }}</h3>
        <div class="mt-5 space-y-5">
          <div>
            <label class="field-label">Name</label>
            <input v-model="form.NAME" class="field-input" type="text" />
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
          submit-label="Save Stock Class"
          @cancel="loadItem()"
          @delete="removeItem"
        />
      </form>
    </div>

    <AppDialog v-model:visible="dialogVisible" title="Stock Classes" :message="message" :success="isSuccess" />
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
import { deleteStockClass, fetchStockClass, fetchStockClasses, saveStockClass } from '@/services/admin-service'
import { getApiMessage } from '@/services/api'
import type { StockClass } from '@/services/types'
import { EMPTY_GUID } from '@/utils/constants'
import { formatNumber } from '@/utils/formatters'
import { validateStockClass } from '@/utils/validators'

const route = useRoute()
const router = useRouter()
const { isBusy, dialogVisible, isSuccess, message, showMessage } = useAsyncState()

const items = ref<StockClass[]>([])
const currentId = ref('')
const form = reactive<StockClass>({
  STOCK_CLASS_PK: EMPTY_GUID,
  NAME: '',
  TOTAL_SHARES: 0
})

onMounted(async () => {
  await refreshItems()
  const routeId = typeof route.params.id === 'string' && route.params.id ? route.params.id : ''
  if (routeId) {
    await loadItem(routeId)
  }
})

async function refreshItems(): Promise<void> {
  items.value = await fetchStockClasses()
}

async function loadItem(id = ''): Promise<void> {
  currentId.value = id
  if (!id) {
    Object.assign(form, { STOCK_CLASS_PK: EMPTY_GUID, NAME: '', TOTAL_SHARES: 0 })
    await router.replace({ name: 'stock-classes' })
    return
  }

  const response = await fetchStockClass(id)
  Object.assign(form, response.StockClass)
  await router.replace({ name: 'stock-classes', params: { id } })
}

async function saveItem(): Promise<void> {
  const errors = validateStockClass(form)
  if (errors.length > 0) {
    showMessage(errors.join(' '), false)
    return
  }

  isBusy.value = true
  try {
    const response = await saveStockClass({
      Key: currentId.value || EMPTY_GUID,
      Data: { ...form }
    })
    showMessage(response.message ?? 'Stock class saved.', response.success !== false)
    await refreshItems()
    await loadItem(response.key)
  } catch (error) {
    showMessage(getApiMessage(error, 'Unable to save the stock class.'), false)
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
    const response = await deleteStockClass(currentId.value)
    showMessage(response.message ?? 'Stock class removed.', response.success !== false)
    items.value = response.StockClasses
    await loadItem()
  } catch (error) {
    showMessage(getApiMessage(error, 'Unable to remove the stock class.'), false)
  } finally {
    isBusy.value = false
  }
}
</script>
