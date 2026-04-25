<template>
  <section class="space-y-6">
    <PageIntro
      eyebrow="Support"
      title="Contact IncTrak"
      description="Send product questions, sales inquiries, or implementation issues directly through the API-backed contact flow."
    />

    <form class="card-surface rounded-[2rem] p-8" @submit.prevent="submitForm">
      <div class="grid gap-5 md:grid-cols-2">
        <div>
          <label class="field-label">Email Address</label>
          <input v-model="form.EmailAddress" class="field-input" type="email" />
        </div>
        <div>
          <label class="field-label">Name</label>
          <input v-model="form.Name" class="field-input" type="text" />
        </div>
      </div>
      <div class="mt-5 grid gap-5 md:grid-cols-[14rem_minmax(0,1fr)]">
        <div>
          <label class="field-label">Category</label>
          <select v-model.number="form.MessageTypeFk" class="field-select">
            <option v-for="item in messageTypes" :key="item.Key" :value="item.Key">{{ item.Name }}</option>
          </select>
        </div>
        <div>
          <label class="field-label">Subject</label>
          <input v-model="form.Subject" class="field-input" type="text" />
        </div>
      </div>
      <div class="mt-5">
        <label class="field-label">Message</label>
        <textarea v-model="form.Message" class="field-textarea min-h-52" />
      </div>
      <div class="mt-6 flex flex-wrap items-center gap-3">
        <Button type="submit" label="Send Message" :loading="isBusy" />
        <a class="text-sm font-semibold text-[var(--app-accent)]" href="mailto:contact@inctrak.com">contact@inctrak.com</a>
      </div>
    </form>

    <AppDialog
      v-model:visible="dialogVisible"
      title="Contact"
      :message="message"
      :success="isSuccess"
    />
  </section>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import Button from 'primevue/button'
import AppDialog from '@/components/AppDialog.vue'
import PageIntro from '@/components/PageIntro.vue'
import { useAsyncState } from '@/composables/useAsyncState'
import { fetchFeedbackForm, fetchMessageTypes, sendFeedback } from '@/services/feedback-service'
import { getApiMessage } from '@/services/api'
import type { FeedbackForm, MessageType } from '@/services/types'

const { isBusy, dialogVisible, isSuccess, message, showMessage } = useAsyncState()

const form = reactive<FeedbackForm>({
  EmailAddress: '',
  Name: '',
  MessageTypeFk: 7,
  Subject: '',
  Message: ''
})

const messageTypes = ref<MessageType[]>([])

onMounted(async () => {
  messageTypes.value = await fetchMessageTypes()
  Object.assign(form, await fetchFeedbackForm())
})

async function submitForm(): Promise<void> {
  if (!form.Message.trim() && !form.Subject.trim()) {
    showMessage('Please enter at least a subject or message.', false)
    return
  }

  if (!form.EmailAddress.trim() && !form.Name.trim()) {
    showMessage('Please enter at least an email address or name.', false)
    return
  }

  isBusy.value = true
  try {
    const response = await sendFeedback(form)
    showMessage(response.message ?? 'Your message has been sent.', response.success !== false)
    if (response.success !== false) {
      form.EmailAddress = ''
      form.Name = ''
      form.Subject = ''
      form.Message = ''
    }
  } catch (error) {
    showMessage(getApiMessage(error, 'An error occurred while submitting your message.'), false)
  } finally {
    isBusy.value = false
  }
}
</script>
