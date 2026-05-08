import { apiGet, apiPost } from '@/services/api'
import type { ApiResponse, FeedbackForm, MessageType } from '@/services/types'

export function fetchMessageTypes(): Promise<MessageType[]> {
  return apiGet<MessageType[]>('/api/feedback/message_types/')
}

export function fetchFeedbackForm(): Promise<FeedbackForm> {
  return apiGet<FeedbackForm>('/api/feedback/get_message/')
}

export function sendFeedback(form: FeedbackForm): Promise<ApiResponse> {
  return apiPost<ApiResponse>('/api/feedback/save_message/', form)
}
