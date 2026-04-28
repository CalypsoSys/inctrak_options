<template>
  <section class="space-y-6">
    <PageIntro eyebrow="Admin" title="Grants" description="Search and manage grants, vesting schedules, plans, and termination rules." />
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
          <h3 class="text-lg font-bold text-slate-900">Grant results</h3>
          <Button label="New" icon="pi pi-plus" @click="loadItem(EMPTY_GUID)" />
        </div>
        <div class="overflow-x-auto">
          <table class="app-table">
            <thead>
              <tr>
                <th>Participant</th>
                <th>Plan</th>
                <th>Shares</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="item in items" :key="item.GRANT_PK">
                <td><button class="font-semibold text-[var(--app-accent)]" @click="loadItem(item.GRANT_PK)">{{ item.PARTICIPANT_NAME }}</button></td>
                <td>{{ item.PLAN_NAME }}</td>
                <td>{{ formatNumber(item.SHARES) }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </article>

      <form class="card-surface rounded-[2rem] p-6" @submit.prevent="saveItem">
        <h3 class="text-lg font-bold text-slate-900">{{ currentId ? 'Edit grant' : 'Create grant' }}</h3>
        <div class="mt-5 space-y-5">
          <ParticipantLookup
            label="Participant"
            v-model="selectedParticipant"
            :suggestions="participantSuggestions"
            @search="searchParticipantOptions"
          />
          <div class="grid gap-5 md:grid-cols-2">
            <div>
              <label class="field-label">Plan</label>
              <select v-model="form.PLAN_FK" class="field-select">
                <option :value="null">Select a plan</option>
                <option v-for="item in plans" :key="item.PLAN_PK" :value="item.PLAN_PK">{{ item.NAME }}</option>
              </select>
            </div>
            <div>
              <label class="field-label">Vesting Schedule</label>
              <select v-model="form.VESTING_SCHEDULE_FK" class="field-select">
                <option :value="null">Select a schedule</option>
                <option v-for="item in schedules" :key="item.SCHEDULE_PK" :value="item.SCHEDULE_PK">{{ item.NAME }}</option>
              </select>
            </div>
          </div>
          <div class="grid gap-5 md:grid-cols-2">
            <div>
              <label class="field-label">Date of Grant</label>
              <input v-model="form.DATE_OF_GRANT" class="field-input" type="date" />
            </div>
            <div>
              <label class="field-label">Vesting Start</label>
              <input v-model="form.VESTING_START" class="field-input" type="date" />
            </div>
          </div>
          <div class="grid gap-5 md:grid-cols-2">
            <div>
              <label class="field-label">Option Price</label>
              <input v-model.number="form.OPTION_PRICE" class="field-input" step="0.000001" type="number" />
            </div>
            <div>
              <label class="field-label">Shares Granted</label>
              <input v-model.number="form.SHARES" class="field-input" min="1" type="number" />
            </div>
          </div>
          <div>
            <label class="field-label">Termination Rule</label>
            <select v-model="form.TERMINATION_FK" class="field-select">
              <option :value="null">Select a termination rule</option>
              <option v-for="item in terminations" :key="item.TERMINATION_PK" :value="item.TERMINATION_PK">{{ item.NAME }}</option>
            </select>
          </div>
        </div>
        <FormActions
          :busy="isBusy"
          :show-delete="Boolean(currentId)"
          :show-cancel="Boolean(currentId)"
          submit-label="Save Grant"
          @cancel="loadItem(EMPTY_GUID)"
          @delete="removeItem"
        />
      </form>
    </div>

    <AppDialog v-model:visible="dialogVisible" title="Grants" :message="message" :success="isSuccess" />
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
import { deleteGrant, fetchGrant, saveGrant, searchGrants, searchParticipantLookup } from '@/services/admin-service'
import { getApiMessage } from '@/services/api'
import type { Grant, ParticipantSummary, Plan, Schedule, Termination } from '@/services/types'
import { EMPTY_GUID } from '@/utils/constants'
import { formatNumber, toDateInputValue } from '@/utils/formatters'
import { validateGrant } from '@/utils/validators'

const route = useRoute()
const router = useRouter()
const { isBusy, dialogVisible, isSuccess, message, showMessage } = useAsyncState()

const searchText = ref('')
const searchType = ref('all')
const items = ref<Grant[]>([])
const plans = ref<Plan[]>([])
const schedules = ref<Schedule[]>([])
const terminations = ref<Termination[]>([])
const participantSuggestions = ref<ParticipantSummary[]>([])
const selectedParticipant = ref<ParticipantSummary | null>(null)
const currentId = ref('')
const form = reactive<Grant>({
  GRANT_PK: EMPTY_GUID,
  PARTICIPANT_FK: null,
  PLAN_FK: null,
  VESTING_SCHEDULE_FK: null,
  TERMINATION_FK: null,
  SHARES: 0,
  OPTION_PRICE: 0,
  DATE_OF_GRANT: '',
  VESTING_START: ''
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
  items.value = await searchGrants(actualSearch, actualType as never)
}

async function searchParticipantOptions(query: string): Promise<void> {
  participantSuggestions.value = await searchParticipantLookup(query || '_____')
}

async function loadItem(id = ''): Promise<void> {
  currentId.value = id && id !== EMPTY_GUID ? id : ''
  if (!id) {
    Object.assign(form, {
      GRANT_PK: EMPTY_GUID,
      PARTICIPANT_FK: null,
      PLAN_FK: null,
      VESTING_SCHEDULE_FK: null,
      TERMINATION_FK: null,
      SHARES: 0,
      OPTION_PRICE: 0,
      DATE_OF_GRANT: '',
      VESTING_START: ''
    })
    selectedParticipant.value = null
    await router.replace({ name: 'grants' })
    return
  }

  const response = await fetchGrant(id)
  plans.value = response.Plans
  schedules.value = response.Vesting
  terminations.value = response.Terminations
  selectedParticipant.value = response.Participant
  Object.assign(form, response.Grant, {
    DATE_OF_GRANT: toDateInputValue(response.Grant.DATE_OF_GRANT),
    VESTING_START: toDateInputValue(response.Grant.VESTING_START)
  })
  await router.replace({ name: 'grants', params: { id } })
}

async function saveItem(): Promise<void> {
  const errors = validateGrant(form)
  if (errors.length > 0) {
    showMessage(errors.join(' '), false)
    return
  }

  isBusy.value = true
  try {
    const response = await saveGrant({
      Key: currentId.value || EMPTY_GUID,
      UUID: EMPTY_GUID,
      Data: { ...form }
    })
    showMessage(response.message ?? 'Grant saved.', response.success !== false)
    await runSearch(true)
    await loadItem(response.key)
  } catch (error) {
    showMessage(getApiMessage(error, 'Unable to save the grant.'), false)
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
    const response = await deleteGrant(currentId.value)
    showMessage(response.message ?? 'Grant removed.', response.success !== false)
    items.value = response.Grants
    await loadItem(EMPTY_GUID)
  } catch (error) {
    showMessage(getApiMessage(error, 'Unable to remove the grant.'), false)
  } finally {
    isBusy.value = false
  }
}
</script>
