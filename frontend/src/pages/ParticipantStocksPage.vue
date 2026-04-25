<template>
  <section class="space-y-6">
    <PageIntro eyebrow="Participant" title="Stock Summary" description="Review direct stock sales and holdings tied to your participant record." />
    <article class="card-surface overflow-hidden rounded-[2rem] p-6">
      <div class="overflow-x-auto">
        <table class="app-table">
          <thead>
            <tr>
              <th>Stock Class</th>
              <th>Date</th>
              <th>Price</th>
              <th>Shares</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in items" :key="item.STOCK_HOLDER_PK">
              <td>{{ item.STOCK_CLASS_NAME }}</td>
              <td>{{ formatDate(item.DATE_OF_SALE) }}</td>
              <td>{{ formatNumber(item.PRICE, 2) }}</td>
              <td>{{ formatNumber(item.SHARES) }}</td>
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
import { fetchParticipantStocks } from '@/services/participant-service'
import type { StockHolder } from '@/services/types'
import { formatDate, formatNumber } from '@/utils/formatters'

const items = ref<StockHolder[]>([])

onMounted(async () => {
  items.value = await fetchParticipantStocks()
})
</script>
