import { apiClient } from '@/services/api'

export type AppSessionResponse = {
  success: boolean
  Role: 'admin' | 'optionee'
}

export type TenantHeaderContext = {
  tenantId?: string
  tenantSlug?: string
  tenantDatabaseName?: string
}

export async function fetchAppSession(accessToken?: string, tenantContext?: TenantHeaderContext): Promise<AppSessionResponse> {
  const host = window.location.hostname.toLowerCase()
  const includeTenantHeaders = host === '127.0.0.1' || host === 'localhost'
  const response = await apiClient.request<AppSessionResponse>({
    method: 'GET',
    url: '/api/control-plane/app-session',
    headers: {
      ...(accessToken
        ? {
            Authorization: `Bearer ${accessToken}`
          }
        : undefined),
      ...(includeTenantHeaders && tenantContext?.tenantId && tenantContext?.tenantSlug
        ? {
            'X-IncTrak-Tenant-Id': tenantContext.tenantId,
            'X-IncTrak-Tenant-Slug': tenantContext.tenantSlug
          }
        : undefined),
      ...(includeTenantHeaders && tenantContext?.tenantDatabaseName
        ? {
            'X-IncTrak-Tenant-Db': tenantContext.tenantDatabaseName
          }
        : undefined)
    }
  })

  return response.data
}
