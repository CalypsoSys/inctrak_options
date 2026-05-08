import { apiGet } from '@/services/api'
import type { ApiResponse, Grant, StockHolder, VestScheduleEntry } from '@/services/types'

export type ParticipantGrantDetailResponse = ApiResponse & {
  Grant: Grant
  VestSchedule: VestScheduleEntry[]
}

export function fetchParticipantStocks(): Promise<StockHolder[]> {
  return apiGet<StockHolder[]>('/api/participant/stocks/')
}

export function fetchParticipantOptionSummary(): Promise<
  Array<{
    PLAN: string
    GRANTED: number
    VESTED: number
    VEST_PCT: number
    UNVESTED: number
    UNVEST_PCT: number
  }>
> {
  return apiGet('/api/participant/summary/')
}

export function fetchParticipantGrants(): Promise<Grant[]> {
  return apiGet<Grant[]>('/api/participant/grants/')
}

export function fetchParticipantGrant(id: string): Promise<ParticipantGrantDetailResponse> {
  return apiGet<ParticipantGrantDetailResponse>(`/api/participant/grant/${id}/`)
}
