<template>
  <section class="space-y-6">
    <PageIntro eyebrow="Participant" title="Grant Details" description="Inspect your grants and expand into detailed vesting schedules." />
    <div class="grid gap-6 xl:grid-cols-[0.95fr_1.05fr]">
      <article class="card-surface overflow-hidden rounded-[2rem] p-6">
        <div class="overflow-x-auto">
          <table class="app-table">
            <thead>
              <tr>
                <th>Plan</th>
                <th>Schedule</th>
                <th>Grant Date</th>
                <th>Vest Start</th>
                <th>Option Price</th>
                <th>Shares</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="item in items" :key="item.GRANT_PK">
                <td><button class="font-semibold text-[var(--app-accent)]" @click="loadItem(item.GRANT_PK)">{{ item.PLAN_NAME }}</button></td>
                <td>{{ item.VEST_NAME }}</td>
                <td>{{ formatDate(item.DATE_OF_GRANT) }}</td>
                <td>{{ formatDate(item.VESTING_START) }}</td>
                <td>{{ formatNumber(item.OPTION_PRICE, 2) }}</td>
                <td>{{ formatNumber(item.SHARES) }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </article>

      <article class="space-y-6" v-if="selectedGrant">
        <article class="card-surface rounded-[2rem] p-6">
          <h3 class="text-lg font-bold text-slate-900">{{ selectedGrant.PLAN_NAME }}</h3>
          <div class="mt-5 grid gap-4 md:grid-cols-2">
            <div><span class="field-label">Vesting Schedule</span><p>{{ selectedGrant.VEST_NAME }}</p></div>
            <div><span class="field-label">Termination Date</span><p>{{ formatDate(selectedGrant.TerminationDate ?? '') }}</p></div>
            <div><span class="field-label">Grant Date</span><p>{{ formatDate(selectedGrant.DATE_OF_GRANT) }}</p></div>
            <div><span class="field-label">Vest Start</span><p>{{ formatDate(selectedGrant.VESTING_START) }}</p></div>
            <div><span class="field-label">Vest End</span><p>{{ formatDate(selectedGrant.VestingEnd ?? '') }}</p></div>
            <div><span class="field-label">Option Price</span><p>{{ formatNumber(selectedGrant.OPTION_PRICE, 2) }}</p></div>
            <div><span class="field-label">Shares</span><p>{{ formatNumber(selectedGrant.SHARES) }}</p></div>
          </div>
        </article>

        <VestingScheduleTable v-if="vestSchedule.length > 0" :rows="vestSchedule" />
      </article>
    </div>
  </section>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import PageIntro from '@/components/PageIntro.vue'
import VestingScheduleTable from '@/components/VestingScheduleTable.vue'
import { fetchParticipantGrant, fetchParticipantGrants } from '@/services/participant-service'
import type { Grant, VestScheduleEntry } from '@/services/types'
import { formatDate, formatNumber } from '@/utils/formatters'

const route = useRoute()
const router = useRouter()

const items = ref<Grant[]>([])
const selectedGrant = ref<Grant | null>(null)
const vestSchedule = ref<VestScheduleEntry[]>([])

onMounted(async () => {
  items.value = await fetchParticipantGrants()
  const routeId = typeof route.params.id === 'string' && route.params.id ? route.params.id : ''
  if (routeId) {
    await loadItem(routeId)
  }
})

async function loadItem(id: string): Promise<void> {
  const response = await fetchParticipantGrant(id)
  selectedGrant.value = response.Grant
  vestSchedule.value = response.VestSchedule
  await router.replace({ name: 'participant-grants', params: { id } })
}
</script>
