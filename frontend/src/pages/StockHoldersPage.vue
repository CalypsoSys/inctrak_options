<template>
  <section class="space-y-6">
    <PageIntro eyebrow="Admin" title="Stock Holders" description="Track direct share holders by participant, stock class, sale date, and price per share." />
    <SearchToolbar
      v-model="searchText"
      :busy="isBusy"
      :search-type="searchType"
      @update:search-type="searchType = $event"
      @search="runSearch"
    />
    <div class="grid gap-6 xl:grid-cols-[0.95fr_1.05fr]">
      <article class="card-surface overflow-hidden rounded-[2rem] p-6">
        <div class="mb-4 flex items-center justify-between">
          <h3 class="text-lg font-bold text-slate-900">Stock holder results</h3>
          <Button label="New" icon="pi pi-plus" @click="loadItem(EMPTY_GUID)" />
        </div>
        <div class="overflow-x-auto">
          <table class="app-table">
            <thead>
              <tr>
                <th>Participant</th>
                <th>Stock Class</th>
                <th>Shares</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="item in items" :key="item.STOCK_HOLDER_PK">
                <td><button class="font-semibold text-[var(--app-accent)]" @click="loadItem(item.STOCK_HOLDER_PK)">{{ item.PARTICIPANT_NAME }}</button></td>
                <td>{{ item.STOCK_CLASS_NAME }}</td>
                <td>{{ formatNumber(item.SHARES) }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </article>

      <form class="card-surface rounded-[2rem] p-6" @submit.prevent="saveItem">
        <h3 class="text-lg font-bold text-slate-900">{{ currentId ? 'Edit stock holder' : 'Create stock holder' }}</h3>
        <div class="mt-5 space-y-5">
          <ParticipantLookup
            label="Participant"
            v-model="selectedParticipant"
            :suggestions="participantSuggestions"
            @search="searchParticipantOptions"
          />
          <div>
            <label class="field-label">Stock Class</label>
            <select v-model="form.STOCK_CLASS_FK" class="field-select">
              <option :value="null">Select a stock class</option>
              <option v-for="item in stockClasses" :key="item.STOCK_CLASS_PK" :value="item.STOCK_CLASS_PK">{{ item.NAME }}</option>
            </select>
          </div>
          <div>
            <label class="field-label">Date of Sale</label>
            <input v-model="form.DATE_OF_SALE" class="field-input" type="date" />
          </div>
          <div class="grid gap-5 md:grid-cols-2">
            <div>
              <label class="field-label">Price Per Share</label>
              <input v-model.number="form.PRICE" class="field-input" step="0.000001" type="number" />
            </div>
            <div>
              <label class="field-label">Shares</label>
              <input v-model.number="form.SHARES" class="field-input" min="1" type="number" />
            </div>
          </div>
        </div>
        <FormActions
          :busy="isBusy"
          :show-delete="Boolean(currentId)"
          :show-cancel="Boolean(currentId)"
          submit-label="Save Stock Holder"
          @cancel="loadItem(EMPTY_GUID)"
          @delete="removeItem"
        />
      </form>
    </div>

    <AppDialog v-model:visible="dialogVisible" title="Stock Holders" :message="message" :success="isSuccess" />
  </section>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Button from 'primevue/button'
import AppDialog from '@/components/AppDialog.vue'
import FormActions from '@/components/FormActions.vue'
import PageIntro from '@/components/PageIntro.vue'
import ParticipantLookup from '@/components/ParticipantLookup.vue'
import SearchToolbar from '@/components/SearchToolbar.vue'
import { useAsyncState } from '@/composables/useAsyncState'
import { deleteStockHolder, fetchStockHolder, saveStockHolder, searchParticipantLookup, searchStockHolders } from '@/services/admin-service'
import { getApiMessage } from '@/services/api'
import type { ParticipantSummary, StockClass, StockHolder } from '@/services/types'
import { EMPTY_GUID } from '@/utils/constants'
import { formatNumber, toDateInputValue } from '@/utils/formatters'
import { validateStockHolder } from '@/utils/validators'

const route = useRoute()
const router = useRouter()
const { isBusy, dialogVisible, isSuccess, message, showMessage } = useAsyncState()

const searchText = ref('')
const searchType = ref('all')
const items = ref<StockHolder[]>([])
const stockClasses = ref<StockClass[]>([])
const participantSuggestions = ref<ParticipantSummary[]>([])
const selectedParticipant = ref<ParticipantSummary | null>(null)
const currentId = ref('')
const form = reactive<StockHolder>({
  STOCK_HOLDER_PK: EMPTY_GUID,
  PARTICIPANT_FK: null,
  STOCK_CLASS_FK: null,
  SHARES: 0,
  PRICE: 0,
  DATE_OF_SALE: ''
})

watch(selectedParticipant, (value) => {
  form.PARTICIPANT_FK = value?.PARTICIPANT_PK ?? null
})

onMounted(async () => {
  await runSearch(true)
  const routeId = typeof route.params.id === 'string' && route.params.id ? route.params.id : ''
  await loadItem(routeId || EMPTY_GUID)
})

async function runSearch(force = false): Promise<void> {
  const actualSearch = force ? '_____' : searchText.value
  const actualType = force ? '_____' : searchType.value
  items.value = await searchStockHolders(actualSearch, actualType as never)
}

async function searchParticipantOptions(query: string): Promise<void> {
  participantSuggestions.value = await searchParticipantLookup(query || '_____')
}

async function loadItem(id = ''): Promise<void> {
  currentId.value = id && id !== EMPTY_GUID ? id : ''
  if (!id) {
    Object.assign(form, {
      STOCK_HOLDER_PK: EMPTY_GUID,
      PARTICIPANT_FK: null,
      STOCK_CLASS_FK: null,
      SHARES: 0,
      PRICE: 0,
      DATE_OF_SALE: ''
    })
    selectedParticipant.value = null
    await router.replace({ name: 'stock-holders' })
    return
  }

  const response = await fetchStockHolder(id)
  stockClasses.value = response.StockClasses
  selectedParticipant.value = response.Participant
  Object.assign(form, response.StockHolder, {
    DATE_OF_SALE: toDateInputValue(response.StockHolder.DATE_OF_SALE)
  })
  await router.replace({ name: 'stock-holders', params: { id } })
}

async function saveItem(): Promise<void> {
  const errors = validateStockHolder(form)
  if (errors.length > 0) {
    showMessage(errors.join(' '), false)
    return
  }

  isBusy.value = true
  try {
    const response = await saveStockHolder({
      Key: currentId.value || EMPTY_GUID,
      UUID: EMPTY_GUID,
      Data: { ...form }
    })
    showMessage(response.message ?? 'Stock holder saved.', response.success !== false)
    await runSearch(true)
    await loadItem(response.key)
  } catch (error) {
    showMessage(getApiMessage(error, 'Unable to save the stock holder.'), false)
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
    const response = await deleteStockHolder(currentId.value)
    showMessage(response.message ?? 'Stock holder removed.', response.success !== false)
    items.value = response.StockHolders
    await loadItem(EMPTY_GUID)
  } catch (error) {
    showMessage(getApiMessage(error, 'Unable to remove the stock holder.'), false)
  } finally {
    isBusy.value = false
  }
}
</script>
