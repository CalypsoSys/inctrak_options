import { apiDelete, apiGet, apiPost } from '@/services/api'
import type {
  ApiResponse,
  EntitySaveEnvelope,
  EntitySaveWithChildrenEnvelope,
  Grant,
  ParticipantDetail,
  ParticipantSummary,
  ParticipantType,
  Period,
  PeriodType,
  Plan,
  Schedule,
  SearchType,
  StockClass,
  StockHolder,
  TermFromType,
  Termination,
  AmountType
} from '@/services/types'

export type StockClassDetailResponse = ApiResponse & {
  StockClass: StockClass
}

export type PlanDetailResponse = ApiResponse & {
  Plan: Plan
  StockClasses: StockClass[]
}

export type ScheduleDetailResponse = ApiResponse & {
  Schedule: Schedule
  Periods: Period[]
  PeriodTypes: PeriodType[]
  AmountTypes: AmountType[]
}

export type ParticipantDetailResponse = ApiResponse & {
  Participant: ParticipantDetail
  PartTypes: ParticipantType[]
}

export type TerminationDetailResponse = ApiResponse & {
  Termination: Termination
  TermFromType: TermFromType[]
}

export type GrantDetailResponse = ApiResponse & {
  Grant: Grant
  Participant: ParticipantSummary | null
  Plans: Plan[]
  Vesting: Schedule[]
  Terminations: Termination[]
}

export type StockHolderDetailResponse = ApiResponse & {
  StockHolder: StockHolder
  Participant: ParticipantSummary | null
  StockClasses: StockClass[]
}

export function fetchStockClasses(): Promise<StockClass[]> {
  return apiGet<StockClass[]>('/api/company/stockclasses/')
}

export function fetchStockClass(id: string, uuid: string): Promise<StockClassDetailResponse> {
  return apiGet<StockClassDetailResponse>(`/api/company/stockclass/${id}/${uuid}`)
}

export function saveStockClass(payload: EntitySaveEnvelope<StockClass>): Promise<ApiResponse & { key: string }> {
  return apiPost<ApiResponse & { key: string }>('/api/company/stockclass/', payload)
}

export function deleteStockClass(id: string, uuid: string): Promise<ApiResponse & { StockClasses: StockClass[] }> {
  return apiDelete<ApiResponse & { StockClasses: StockClass[] }>(`/api/company/stockclass/${id}/${uuid}`)
}

export function fetchPlans(): Promise<Plan[]> {
  return apiGet<Plan[]>('/api/company/plans/')
}

export function fetchPlan(id: string, uuid: string): Promise<PlanDetailResponse> {
  return apiGet<PlanDetailResponse>(`/api/company/plan/${id}/${uuid}`)
}

export function savePlan(payload: EntitySaveEnvelope<Plan>): Promise<ApiResponse & { key: string }> {
  return apiPost<ApiResponse & { key: string }>('/api/company/plan/', payload)
}

export function deletePlan(id: string, uuid: string): Promise<ApiResponse & { Plans: Plan[] }> {
  return apiDelete<ApiResponse & { Plans: Plan[] }>(`/api/company/plan/${id}/${uuid}`)
}

export function fetchSchedules(): Promise<Schedule[]> {
  return apiGet<Schedule[]>('/api/company/schedules/')
}

export function fetchSchedule(id: string, uuid: string): Promise<ScheduleDetailResponse> {
  return apiGet<ScheduleDetailResponse>(`/api/company/schedule/${id}/${uuid}`)
}

export function saveSchedule(payload: EntitySaveWithChildrenEnvelope<Schedule, Period>): Promise<ApiResponse & { key: string }> {
  return apiPost<ApiResponse & { key: string }>('/api/company/schedule/', payload)
}

export function deleteSchedule(id: string, uuid: string): Promise<ApiResponse & { Schedules: Schedule[] }> {
  return apiDelete<ApiResponse & { Schedules: Schedule[] }>(`/api/company/schedule/${id}/${uuid}`)
}

export function fetchTerminations(): Promise<Termination[]> {
  return apiGet<Termination[]>('/api/company/terminations/')
}

export function fetchTermination(id: string, uuid: string): Promise<TerminationDetailResponse> {
  return apiGet<TerminationDetailResponse>(`/api/company/termination/${id}/${uuid}`)
}

export function saveTermination(payload: EntitySaveEnvelope<Termination>): Promise<ApiResponse & { key: string }> {
  return apiPost<ApiResponse & { key: string }>('/api/company/termination/', payload)
}

export function deleteTermination(id: string, uuid: string): Promise<ApiResponse & { Terminations: Termination[] }> {
  return apiDelete<ApiResponse & { Terminations: Termination[] }>(`/api/company/termination/${id}/${uuid}`)
}

export function searchParticipants(search: string, searchType: SearchType): Promise<ParticipantSummary[]> {
  return apiGet<ParticipantSummary[]>(`/api/company/participants/${encodeURIComponent(search)}/${searchType}/`)
}

export function searchParticipantLookup(search: string): Promise<ParticipantSummary[]> {
  return apiGet<ParticipantSummary[]>(`/api/company/participants/${encodeURIComponent(search)}/InLine/`)
}

export function fetchParticipant(id: string, uuid: string): Promise<ParticipantDetailResponse> {
  return apiGet<ParticipantDetailResponse>(`/api/company/participant/${id}/${uuid}`)
}

export function saveParticipant(payload: EntitySaveEnvelope<ParticipantDetail>): Promise<ApiResponse & { key: string }> {
  return apiPost<ApiResponse & { key: string }>('/api/company/participant/', payload)
}

export function deleteParticipant(id: string, uuid: string): Promise<ApiResponse & { Participants: ParticipantSummary[] }> {
  return apiDelete<ApiResponse & { Participants: ParticipantSummary[] }>(`/api/company/participant/${id}/${uuid}`)
}

export function searchGrants(search: string, searchType: SearchType): Promise<Grant[]> {
  return apiGet<Grant[]>(`/api/company/grants/${encodeURIComponent(search)}/${searchType}/`)
}

export function fetchGrant(id: string, uuid: string): Promise<GrantDetailResponse> {
  return apiGet<GrantDetailResponse>(`/api/company/grant/${id}/${uuid}`)
}

export function saveGrant(payload: EntitySaveEnvelope<Grant>): Promise<ApiResponse & { key: string }> {
  return apiPost<ApiResponse & { key: string }>('/api/company/grant/', payload)
}

export function deleteGrant(id: string, uuid: string): Promise<ApiResponse & { Grants: Grant[] }> {
  return apiDelete<ApiResponse & { Grants: Grant[] }>(`/api/company/grant/${id}/${uuid}`)
}

export function searchStockHolders(search: string, searchType: SearchType): Promise<StockHolder[]> {
  return apiGet<StockHolder[]>(`/api/company/stockholders/${encodeURIComponent(search)}/${searchType}/`)
}

export function fetchStockHolder(id: string, uuid: string): Promise<StockHolderDetailResponse> {
  return apiGet<StockHolderDetailResponse>(`/api/company/stockholder/${id}/${uuid}`)
}

export function saveStockHolder(payload: EntitySaveEnvelope<StockHolder>): Promise<ApiResponse & { key: string }> {
  return apiPost<ApiResponse & { key: string }>('/api/company/stockholder/', payload)
}

export function deleteStockHolder(id: string, uuid: string): Promise<ApiResponse & { StockHolders: StockHolder[] }> {
  return apiDelete<ApiResponse & { StockHolders: StockHolder[] }>(`/api/company/stockholder/${id}/${uuid}`)
}
