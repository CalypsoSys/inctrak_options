<template>
  <section class="space-y-6">
    <PageIntro eyebrow="Admin" title="Participants" description="Search, create, and manage participants plus linked user access state." />
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
          <h3 class="text-lg font-bold text-slate-900">Search results</h3>
          <Button label="New" icon="pi pi-plus" @click="loadItem(EMPTY_GUID)" />
        </div>
        <div class="overflow-x-auto">
          <table class="app-table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Grants</th>
                <th>Shares</th>
                <th>User</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="item in items" :key="item.PARTICIPANT_PK">
                <td><button class="font-semibold text-[var(--app-accent)]" @click="loadItem(item.PARTICIPANT_PK)">{{ item.NAME }}</button></td>
                <td>{{ formatNumber(item.TOTAL_GRANTS) }}</td>
                <td>{{ formatNumber(item.GRANTED_SHARES) }}</td>
                <td>{{ item.HAS_USER ? 'Yes' : 'No' }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </article>

      <form class="card-surface rounded-[2rem] p-6" @submit.prevent="saveItem">
        <h3 class="text-lg font-bold text-slate-900">{{ currentId ? 'Edit participant' : 'Create participant' }}</h3>
        <div class="mt-5 space-y-5">
          <div>
            <label class="field-label">Name</label>
            <input v-model="form.NAME" class="field-input" type="text" />
          </div>
          <div>
            <label class="field-label">Type</label>
            <select v-model.number="form.PARTICIPANT_TYPE_FK" class="field-select">
              <option :value="0">Select a type</option>
              <option v-for="item in participantTypes" :key="item.PARTICIPANT_TYPE_PK" :value="item.PARTICIPANT_TYPE_PK">{{ item.NAME }}</option>
            </select>
          </div>
          <div class="grid gap-4 md:grid-cols-2">
            <div>
              <label class="field-label">Username</label>
              <input v-model="form.USER_NAME" class="field-input" type="text" />
            </div>
            <div>
              <label class="field-label">Email Address</label>
              <input v-model="form.EMAIL_ADDRESS" class="field-input" type="email" />
            </div>
          </div>
          <div>
            <p class="field-label">User Action</p>
            <div class="grid gap-3 md:grid-cols-2">
              <label v-for="action in userActions" :key="action.value" class="flex items-center gap-2 rounded-2xl border border-[var(--app-border)] bg-white/60 px-4 py-3 text-sm font-semibold text-[var(--app-muted)]">
                <input v-model="form.USER_ACTION" :value="action.value" type="radio" />
                {{ action.label }}
              </label>
            </div>
          </div>
          <div class="grid gap-3 md:grid-cols-1">
            <label class="flex items-center gap-2 text-sm font-semibold text-[var(--app-muted)]">
              <input v-model="form.SEND_EMAIL" type="checkbox" />
              Send activation email
            </label>
          </div>
        </div>
        <FormActions
          :busy="isBusy"
          :show-delete="Boolean(currentId)"
          :show-cancel="Boolean(currentId)"
          submit-label="Save Participant"
          @cancel="loadItem(EMPTY_GUID)"
          @delete="removeItem"
        />
      </form>
    </div>

    <AppDialog v-model:visible="dialogVisible" title="Participants" :message="message" :success="isSuccess" />
  </section>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Button from 'primevue/button'
import AppDialog from '@/components/AppDialog.vue'
import FormActions from '@/components/FormActions.vue'
import PageIntro from '@/components/PageIntro.vue'
import SearchToolbar from '@/components/SearchToolbar.vue'
import { useAsyncState } from '@/composables/useAsyncState'
import { deleteParticipant, fetchParticipant, saveParticipant, searchParticipants } from '@/services/admin-service'
import { getApiMessage } from '@/services/api'
import type { ParticipantDetail, ParticipantSummary, ParticipantType } from '@/services/types'
import { useAuthStore } from '@/stores/auth'
import { EMPTY_GUID } from '@/utils/constants'
import { formatNumber } from '@/utils/formatters'
import { validateParticipant } from '@/utils/validators'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const { isBusy, dialogVisible, isSuccess, message, showMessage } = useAsyncState()

const searchText = ref('')
const searchType = ref('all')
const items = ref<ParticipantSummary[]>([])
const participantTypes = ref<ParticipantType[]>([])
const currentId = ref('')
const userActions = [
  { value: 'create_user', label: 'Create New User' },
  { value: 'update_user', label: 'Update User' },
  { value: 'delete_user', label: 'Delete User' },
  { value: 'none', label: 'No User Action' }
]
const form = reactive<ParticipantDetail>({
  PARTICIPANT_PK: EMPTY_GUID,
  PARTICIPANT_TYPE_FK: 0,
  NAME: '',
  USER_NAME: '',
  EMAIL_ADDRESS: '',
  USER_ACTION: 'create_user',
  SEND_EMAIL: true
})

onMounted(async () => {
  await runSearch(true)
  const routeId = typeof route.params.id === 'string' && route.params.id ? route.params.id : ''
  await loadItem(routeId || EMPTY_GUID)
})

async function runSearch(force = false): Promise<void> {
  const actualSearch = force ? '_____' : searchText.value
  const actualType = force ? '_____' : searchType.value
  items.value = await searchParticipants(actualSearch, actualType as never)
}

async function loadItem(id = ''): Promise<void> {
  currentId.value = id && id !== EMPTY_GUID ? id : ''
  if (!id) {
    Object.assign(form, {
      PARTICIPANT_PK: EMPTY_GUID,
      PARTICIPANT_TYPE_FK: 0,
      NAME: '',
      USER_NAME: '',
      EMAIL_ADDRESS: '',
      USER_ACTION: 'create_user',
      SEND_EMAIL: true
    })
    await router.replace({ name: 'participants' })
    return
  }

  const response = await fetchParticipant(id, authStore.uuid!)
  participantTypes.value = response.PartTypes
  Object.assign(form, response.Participant)
  await router.replace({ name: 'participants', params: { id } })
}

async function saveItem(): Promise<void> {
  const errors = validateParticipant(form)
  if (errors.length > 0) {
    showMessage(errors.join(' '), false)
    return
  }

  isBusy.value = true
  try {
    const response = await saveParticipant({
      Key: currentId.value || EMPTY_GUID,
      UUID: authStore.uuid!,
      Data: { ...form }
    })
    showMessage(response.message ?? 'Participant saved.', response.success !== false)
    await runSearch(true)
    await loadItem(response.key)
  } catch (error) {
    showMessage(getApiMessage(error, 'Unable to save the participant.'), false)
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
    const response = await deleteParticipant(currentId.value, authStore.uuid!)
    showMessage(response.message ?? 'Participant removed.', response.success !== false)
    items.value = response.Participants
    await loadItem(EMPTY_GUID)
  } catch (error) {
    showMessage(getApiMessage(error, 'Unable to remove the participant.'), false)
  } finally {
    isBusy.value = false
  }
}
</script>
