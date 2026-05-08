<template>
  <div class="card-surface rounded-3xl p-5">
    <div class="grid gap-4 md:grid-cols-[minmax(0,1fr)_12rem_auto]">
      <div>
        <label class="field-label" for="search-text">Search</label>
        <input
          id="search-text"
          v-model="modelValue"
          class="field-input"
          placeholder="Type at least 3 characters"
          @input="emit('update:modelValue', modelValue)"
          @keyup.enter="emit('search', true)"
        />
      </div>
      <div>
        <label class="field-label" for="search-mode">Match</label>
        <select id="search-mode" :value="searchType" class="field-select" @change="emit('update:searchType', ($event.target as HTMLSelectElement).value)">
          <option value="all">All terms</option>
          <option value="any">Any term</option>
          <option value="exact">Exact</option>
        </select>
      </div>
      <div class="flex items-end gap-3">
        <Button label="Search" :loading="busy" @click="emit('search', false)" />
        <Button label="Load recent" severity="secondary" variant="outlined" :disabled="busy" @click="emit('search', true)" />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import Button from 'primevue/button'

const props = defineProps<{
  modelValue: string
  searchType: string
  busy: boolean
}>()

const emit = defineEmits<{
  (event: 'update:modelValue', value: string): void
  (event: 'update:searchType', value: string): void
  (event: 'search', force: boolean): void
}>()

const modelValue = ref(props.modelValue)

watch(
  () => props.modelValue,
  (value) => {
    modelValue.value = value
  }
)
</script>
