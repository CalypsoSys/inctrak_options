<template>
  <div class="flex flex-wrap items-center gap-3 pt-2">
    <Button :loading="busy" :label="submitLabel" type="submit" />
    <Button
      v-if="showDelete"
      severity="danger"
      variant="outlined"
      :disabled="busy"
      :label="deleteLabel"
      @click="$emit('delete')"
    />
    <Button
      v-if="showCancel"
      severity="secondary"
      variant="text"
      :disabled="busy"
      label="Back to list"
      @click="$emit('cancel')"
    />
  </div>
</template>

<script setup lang="ts">
import Button from 'primevue/button'

defineProps<{
  busy: boolean
  submitLabel?: string
  deleteLabel?: string
  showDelete?: boolean
  showCancel?: boolean
}>()

defineEmits<{
  (event: 'delete'): void
  (event: 'cancel'): void
}>()
</script>
