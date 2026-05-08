<template>
  <section class="space-y-6">
    <PageIntro eyebrow="Participant" title="Option Summary" description="See granted, vested, and unvested shares by plan." />
    <article class="card-surface overflow-hidden rounded-[2rem] p-6">
      <div class="overflow-x-auto">
        <table class="app-table">
          <thead>
            <tr>
              <th>Plan</th>
              <th>Granted</th>
              <th>Vested</th>
              <th>Vested %</th>
              <th>Unvested</th>
              <th>Unvested %</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in items" :key="item.PLAN">
              <td>{{ item.PLAN }}</td>
              <td>{{ formatNumber(item.GRANTED) }}</td>
              <td>{{ formatNumber(item.VESTED, 3) }}</td>
              <td>{{ formatNumber(item.VEST_PCT, 3) }}</td>
              <td>{{ formatNumber(item.UNVESTED, 3) }}</td>
              <td>{{ formatNumber(item.UNVEST_PCT, 3) }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </article>
  </section>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import PageIntro from '@/components/PageIntro.vue'
import { fetchParticipantOptionSummary } from '@/services/participant-service'
import { formatNumber } from '@/utils/formatters'

const items = ref<Array<{
  PLAN: string
  GRANTED: number
  VESTED: number
  VEST_PCT: number
  UNVESTED: number
  UNVEST_PCT: number
}>>([])

onMounted(async () => {
  items.value = await fetchParticipantOptionSummary()
})
</script>
