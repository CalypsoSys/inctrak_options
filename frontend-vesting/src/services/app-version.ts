import type { ApiResponse } from '@/services/types'

export const FRONTEND_VERSION = 'v1.0.0'

export type BackendVersionResponse = ApiResponse & {
  backendVersion?: string
  BackendVersion?: string
}

export function getBackendVersion(response: BackendVersionResponse): string {
  return response.backendVersion ?? response.BackendVersion ?? 'unknown'
}
