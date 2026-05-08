import axios, { AxiosError, type AxiosRequestConfig } from 'axios'
import { buildApiUrl } from '@/services/runtime-config'

const SESSION_KEY = 'inctrak.session'
const DEV_TENANT_ID = import.meta.env.VITE_TENANT_ID?.trim()
const DEV_TENANT_SLUG = import.meta.env.VITE_TENANT_SLUG?.trim()
const DEV_TENANT_DB_NAME = import.meta.env.VITE_TENANT_DB_NAME?.trim()

function shouldUseTenantHeaderOverrides(): boolean {
  const host = window.location.hostname.toLowerCase()
  return host === '127.0.0.1' || host === 'localhost'
}

export type ApiFailure = {
  success?: boolean
  login?: boolean
  message?: string
}

type SessionShape = {
  accessToken?: string
  tenantId?: string | null
  tenantSlug?: string | null
  tenantDatabaseName?: string | null
}

function getSessionRecord(): SessionShape | null {
  const raw = window.localStorage.getItem(SESSION_KEY)
  if (!raw) {
    return null
  }

  try {
    return JSON.parse(raw) as SessionShape
  } catch {
    return null
  }
}

export const apiClient = axios.create()

apiClient.interceptors.request.use((config) => {
  if (config.url) {
    config.url = buildApiUrl(config.url)
  }

  config.headers = config.headers ?? {}

  const session = getSessionRecord()
  const accessToken = session?.accessToken ?? null
  if (accessToken && !config.headers.Authorization) {
    config.headers.Authorization = `Bearer ${accessToken}`
  }

  const tenantId = shouldUseTenantHeaderOverrides() ? session?.tenantId?.trim() || DEV_TENANT_ID : null
  const tenantSlug = shouldUseTenantHeaderOverrides() ? session?.tenantSlug?.trim() || DEV_TENANT_SLUG : null
  const tenantDatabaseName = shouldUseTenantHeaderOverrides() ? session?.tenantDatabaseName?.trim() || DEV_TENANT_DB_NAME : null

  if (tenantId && tenantSlug && !config.headers['X-IncTrak-Tenant-Id'] && !config.headers['X-IncTrak-Tenant-Slug']) {
    config.headers['X-IncTrak-Tenant-Id'] = tenantId
    config.headers['X-IncTrak-Tenant-Slug'] = tenantSlug
  }

  if (tenantDatabaseName && !config.headers['X-IncTrak-Tenant-Db']) {
    config.headers['X-IncTrak-Tenant-Db'] = tenantDatabaseName
  }

  return config
})

export function isApiFailure(value: unknown): value is ApiFailure {
  if (!value || typeof value !== 'object') {
    return false
  }

  const record = value as Record<string, unknown>
  return record.success === false || typeof record.message === 'string'
}

export function getApiMessage(error: unknown, fallback: string): string {
  if (axios.isAxiosError(error)) {
    const responseData = (error as AxiosError<ApiFailure>).response?.data
    if (responseData?.message) {
      return responseData.message
    }
  }

  if (error instanceof Error && error.message) {
    return error.message
  }

  return fallback
}

async function request<T>(config: AxiosRequestConfig): Promise<T> {
  const response = await apiClient.request<T>(config)
  return response.data
}

export function apiGet<T>(url: string): Promise<T> {
  return request<T>({ method: 'GET', url })
}

export function apiPost<T>(url: string, data?: unknown, config?: AxiosRequestConfig): Promise<T> {
  return request<T>({ method: 'POST', url, data, ...config })
}

export function apiDelete<T>(url: string): Promise<T> {
  return request<T>({ method: 'DELETE', url })
}
