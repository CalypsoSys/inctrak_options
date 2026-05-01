import { apiGet, apiPost } from '@/services/api'
import type { AmountType, ApiResponse, Grant, Period, PeriodType, QuickInterpretResponse, VestScheduleEntry } from '@/services/types'

export type QuickGrantResponse = ApiResponse & {
  Grant: Grant
  Periods: Period[]
  PeriodTypes: PeriodType[]
  AmountTypes: AmountType[]
  VestSchedule?: VestScheduleEntry[]
}

export function fetchQuickGrant(): Promise<QuickGrantResponse> {
  return apiGet<QuickGrantResponse>('/api/optionee/quick/')
}

export function saveQuickGrant(grant: Grant, periods: Period[]): Promise<QuickGrantResponse> {
  return apiPost<QuickGrantResponse>('/api/optionee/quick/', {
    Data: grant,
    Children: periods
  })
}

export function interpretQuickPrompt(prompt: string): Promise<QuickInterpretResponse> {
  return apiPost<QuickInterpretResponse>('/api/optionee/quick/interpret/', {
    Prompt: prompt,
    StrictAi: true
  })
}
