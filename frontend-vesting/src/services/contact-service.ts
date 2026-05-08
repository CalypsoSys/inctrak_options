import { apiPost } from '@/services/api'
import type { ApiResponse, FeedbackForm } from '@/services/types'

export function sendContactMessage(form: FeedbackForm): Promise<ApiResponse> {
  return apiPost<ApiResponse>('/api/feedback/save_message/', form)
}
