<template>
  <div>
    <label class="field-label">{{ label }}</label>
    <AutoComplete
      fluid
      option-label="NAME"
      :model-value="modelValue"
      :suggestions="suggestions"
      :dropdown="true"
      :force-selection="true"
      placeholder="Select or search for a participant"
      @complete="emit('search', $event.query)"
      @update:model-value="emit('update:modelValue', $event)"
    />
  </div>
</template>

<script setup lang="ts">
import AutoComplete from 'primevue/autocomplete'
import type { ParticipantSummary } from '@/services/types'

defineProps<{
  label: string
  modelValue: ParticipantSummary | null
  suggestions: ParticipantSummary[]
}>()

const emit = defineEmits<{
  (event: 'update:modelValue', value: ParticipantSummary | null): void
  (event: 'search', query: string): void
}>()
</script>
