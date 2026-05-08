<template>
  <Dialog
    :visible="visible"
    modal
    dismissable-mask
    :header="title"
    :style="{ width: 'min(32rem, 92vw)' }"
    @update:visible="emit('update:visible', $event)"
  >
    <div class="space-y-3">
      <Message :severity="success ? 'success' : 'error'" :closable="false">
        {{ message }}
      </Message>
      <div class="flex justify-end">
        <Button label="Close" @click="emit('update:visible', false)" />
      </div>
    </div>
  </Dialog>
</template>

<script setup lang="ts">
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'
import Message from 'primevue/message'

defineProps<{
  visible: boolean
  title: string
  message: string
  success: boolean
}>()

const emit = defineEmits<{
  (event: 'update:visible', value: boolean): void
}>()
</script>
