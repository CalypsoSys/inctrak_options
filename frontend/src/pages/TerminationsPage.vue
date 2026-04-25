<template>
  <section class="space-y-6">
    <PageIntro eyebrow="Admin" title="Termination Rules" description="Capture absolute dates or relative termination rules that grants can reuse." />
    <div class="grid gap-6 xl:grid-cols-[0.95fr_1.05fr]">
      <article class="card-surface overflow-hidden rounded-[2rem] p-6">
        <div class="mb-4 flex items-center justify-between">
          <h3 class="text-lg font-bold text-slate-900">All termination rules</h3>
          <Button label="New" icon="pi pi-plus" @click="loadItem(EMPTY_GUID)" />
        </div>
        <div class="overflow-x-auto">
          <table class="app-table">
            <thead>
              <tr>
                <th>Name</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="item in items" :key="item.TERMINATION_PK">
                <td><button class="font-semibold text-[var(--app-accent)]" @click="loadItem(item.TERMINATION_PK)">{{ item.NAME }}</button></td>
              </tr>
            </tbody>
          </table>
        </div>
      </article>

      <form class="card-surface rounded-[2rem] p-6" @submit.prevent="saveItem">
        <h3 class="text-lg font-bold text-slate-900">{{ currentId ? 'Edit termination rule' : 'Create termination rule' }}</h3>
        <div class="mt-5 space-y-5">
          <div>
            <label class="field-label">Name</label>
            <input v-model="form.NAME" class="field-input" type="text" />
          </div>
          <label class="flex items-center gap-2 text-sm font-semibold text-[var(--app-muted)]">
            <input v-model="form.IS_ABSOLUTE" type="checkbox" />
            Use an absolute date
          </label>
          <div v-if="form.IS_ABSOLUTE">
            <label class="field-label">Absolute Date</label>
            <input v-model="form.ABSOLUTE_DATE" class="field-input" type="date" />
          </div>
          <template v-else>
            <div>
              <label class="field-label">Terminate From</label>
              <select v-model.number="form.TERM_FROM_FK" class="field-select">
                <option :value="null">Select anchor</option>
                <option v-for="item in termFromTypes" :key="item.TERM_FROM_PK" :value="item.TERM_FROM_PK">{{ item.NAME }}</option>
              </select>
            </div>
            <div v-if="form.TERM_FROM_FK === 3">
              <label class="field-label">Specific Date</label>
              <input v-model="form.SPECIFIC_DATE" class="field-input" type="date" />
            </div>
            <div class="grid gap-4 md:grid-cols-3">
              <div>
                <label class="field-label">Years</label>
                <input v-model.number="form.YEARS" class="field-input" type="number" />
              </div>
              <div>
                <label class="field-label">Months</label>
                <input v-model.number="form.MONTHS" class="field-input" type="number" />
              </div>
              <div>
                <label class="field-label">Days</label>
                <input v-model.number="form.DAYS" class="field-input" type="number" />
              </div>
            </div>
          </template>
        </div>
        <FormActions
          :busy="isBusy"
          :show-delete="Boolean(currentId)"
          :show-cancel="Boolean(currentId)"
          submit-label="Save Rule"
          @cancel="loadItem(EMPTY_GUID)"
          @delete="removeItem"
        />
      </form>
    </div>

    <AppDialog v-model:visible="dialogVisible" title="Termination Rules" :message="message" :success="isSuccess" />
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
import { deleteTermination, fetchTermination, fetchTerminations, saveTermination } from '@/services/admin-service'
import { getApiMessage } from '@/services/api'
import type { TermFromType, Termination } from '@/services/types'
import { useAuthStore } from '@/stores/auth'
import { EMPTY_GUID } from '@/utils/constants'
import { validateTermination } from '@/utils/validators'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const { isBusy, dialogVisible, isSuccess, message, showMessage } = useAsyncState()

const items = ref<Termination[]>([])
const termFromTypes = ref<TermFromType[]>([])
const currentId = ref('')
const form = reactive<Termination>({
  TERMINATION_PK: EMPTY_GUID,
  NAME: '',
  IS_ABSOLUTE: false,
  ABSOLUTE_DATE: '',
  TERM_FROM_FK: null,
  SPECIFIC_DATE: '',
  YEARS: 0,
  MONTHS: 0,
  DAYS: 0
})

onMounted(async () => {
  await refreshItems()
  const routeId = typeof route.params.id === 'string' && route.params.id ? route.params.id : ''
  await loadItem(routeId || EMPTY_GUID)
})

async function refreshItems(): Promise<void> {
  items.value = await fetchTerminations()
}

async function loadItem(id = ''): Promise<void> {
  currentId.value = id && id !== EMPTY_GUID ? id : ''
  if (!id) {
    Object.assign(form, {
      TERMINATION_PK: EMPTY_GUID,
      NAME: '',
      IS_ABSOLUTE: false,
      ABSOLUTE_DATE: '',
      TERM_FROM_FK: null,
      SPECIFIC_DATE: '',
      YEARS: 0,
      MONTHS: 0,
      DAYS: 0
    })
    await router.replace({ name: 'termination-rules' })
    return
  }

  const response = await fetchTermination(id, authStore.uuid!)
  termFromTypes.value = response.TermFromType
  Object.assign(form, response.Termination)
  await router.replace({ name: 'termination-rules', params: { id } })
}

async function saveItem(): Promise<void> {
  const errors = validateTermination(form)
  if (errors.length > 0) {
    showMessage(errors.join(' '), false)
    return
  }

  isBusy.value = true
  try {
    const response = await saveTermination({
      Key: currentId.value || EMPTY_GUID,
      UUID: authStore.uuid!,
      Data: { ...form }
    })
    showMessage(response.message ?? 'Termination rule saved.', response.success !== false)
    await refreshItems()
    await loadItem(response.key)
  } catch (error) {
    showMessage(getApiMessage(error, 'Unable to save the termination rule.'), false)
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
    const response = await deleteTermination(currentId.value, authStore.uuid!)
    showMessage(response.message ?? 'Termination rule removed.', response.success !== false)
    items.value = response.Terminations
    await loadItem(EMPTY_GUID)
  } catch (error) {
    showMessage(getApiMessage(error, 'Unable to remove the termination rule.'), false)
  } finally {
    isBusy.value = false
  }
}
</script>
