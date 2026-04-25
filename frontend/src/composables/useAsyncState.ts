import { ref } from 'vue'

export function useAsyncState() {
  const isBusy = ref(false)
  const message = ref('')
  const isSuccess = ref(true)
  const dialogVisible = ref(false)

  function showMessage(nextMessage: string, success = true): void {
    message.value = nextMessage
    isSuccess.value = success
    dialogVisible.value = true
  }

  function hideMessage(): void {
    dialogVisible.value = false
  }

  return {
    isBusy,
    message,
    isSuccess,
    dialogVisible,
    showMessage,
    hideMessage
  }
}
