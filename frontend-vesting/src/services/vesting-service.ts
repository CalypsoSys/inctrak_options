import { apiGet, apiPost } from '@/services/api'
import type { BackendVersionResponse } from '@/services/app-version'
import { buildQuickGrantSavePayload } from '@/services/quick-grant-payload'
import type { AmountType, ApiResponse, Grant, Period, PeriodType, QuickInterpretResponse, VestScheduleEntry } from '@/services/types'

export type QuickGrantResponse = ApiResponse & {
  Grant?: Grant
  Periods?: Period[]
  PeriodTypes?: PeriodType[]
  AmountTypes?: AmountType[]
  VestSchedule?: VestScheduleEntry[]
}

export function fetchQuickGrant(): Promise<QuickGrantResponse> {
  return apiGet<QuickGrantResponse>('/api/optionee/quick/')
}

export function fetchBackendVersion(): Promise<BackendVersionResponse> {
  return apiGet<BackendVersionResponse>('/api/version/')
}

export function saveQuickGrant(grant: Grant, periods: Period[]): Promise<QuickGrantResponse> {
  return apiPost<QuickGrantResponse>('/api/optionee/quick/', buildQuickGrantSavePayload(grant, periods))
}

export function interpretQuickPrompt(prompt: string, strictAi = false, preferredProvider?: string, allowAiFallback = false): Promise<QuickInterpretResponse> {
  return apiPost<QuickInterpretResponse>('/api/optionee/quick/interpret/', {
    Prompt: prompt,
    StrictAi: strictAi,
    AllowAiFallback: allowAiFallback,
    PreferredProvider: preferredProvider ?? null
  })
}
