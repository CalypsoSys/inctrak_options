import { apiClient } from '@/services/api'

export type AppSessionResponse = {
  success: boolean
  Role: 'admin' | 'optionee'
}

export async function fetchAppSession(accessToken?: string): Promise<AppSessionResponse> {
  const response = await apiClient.request<AppSessionResponse>({
    method: 'GET',
    url: '/api/control-plane/app-session',
    headers: accessToken
      ? {
          Authorization: `Bearer ${accessToken}`
        }
      : undefined
  })

  return response.data
}
