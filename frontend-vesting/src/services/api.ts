import axios, { AxiosError, type AxiosRequestConfig } from 'axios'
import { buildApiUrl } from '@/services/runtime-config'

export type ApiFailure = {
  success?: boolean
  message?: string
}

export const apiClient = axios.create()

apiClient.interceptors.request.use((config) => {
  if (config.url) {
    config.url = buildApiUrl(config.url)
  }

  config.headers = config.headers ?? {}
  return config
})

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
