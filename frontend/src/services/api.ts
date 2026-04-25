import axios, { AxiosError, type AxiosRequestConfig } from 'axios'
import { buildApiUrl } from '@/services/runtime-config'

const SESSION_KEY = 'inctrak.session'

export type ApiFailure = {
  success?: boolean
  login?: boolean
  message?: string
}

type SessionShape = {
  uuid?: string
}

function getSessionUuid(): string | null {
  const raw = window.localStorage.getItem(SESSION_KEY)
  if (!raw) {
    return null
  }

  try {
    const parsed = JSON.parse(raw) as SessionShape
    return parsed.uuid ?? null
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

  const uuid = getSessionUuid()
  if (uuid) {
    config.headers['X-IncTrak-UUID'] = uuid
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

export function apiPost<T>(url: string, data?: unknown): Promise<T> {
  return request<T>({ method: 'POST', url, data })
}

export function apiDelete<T>(url: string): Promise<T> {
  return request<T>({ method: 'DELETE', url })
}
