import { apiGet, apiPost } from '@/services/api'
import type { ApiResponse, AmountType, Grant, Period, PeriodType, VestScheduleEntry } from '@/services/types'

export type AdminSummaryResponse = ApiResponse & {
  Plans: Array<{
    NAME: string
    TOTAL_SHARES: number
    GRANTED_SHARES: number
    GRANTS: number
    PARTICIPANTS: number
  }>
  Participants: number
  Grants: number
  Schedules: number
  Counts: Array<{
    name: string
    count: number
    order: number
  }>
}

export type OptioneeSummaryResponse = ApiResponse & {
  OverTime: Record<string, Record<string, number>>
  TotalGranted: number
  TotalVested: number
  TotalUnVested: number
}

export type QuickGrantResponse = ApiResponse & {
  Grant: Grant
  Periods: Period[]
  PeriodTypes: PeriodType[]
  AmountTypes: AmountType[]
  VestSchedule?: VestScheduleEntry[]
}

export function fetchAdminSummary(): Promise<AdminSummaryResponse> {
  return apiGet<AdminSummaryResponse>('/api/company/summary/')
}

export function fetchOptioneeSummary(): Promise<OptioneeSummaryResponse> {
  return apiGet<OptioneeSummaryResponse>('/api/optionee/summary/')
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
