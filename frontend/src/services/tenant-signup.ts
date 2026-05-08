import { apiPost } from '@/services/api'

export type TenantSignupResponse = {
  success: boolean
  message?: string
  TenantId?: string
  TenantSlug?: string
  TenantDatabaseName?: string
  Created?: boolean
}

export function provisionTenantSignup(
  companyName: string,
  tenantSlug: string,
  accessToken: string
): Promise<TenantSignupResponse> {
  return apiPost<TenantSignupResponse>(
    '/api/control-plane/signup',
    {
      CompanyName: companyName,
      TenantSlug: tenantSlug
    },
    {
      headers: {
        Authorization: `Bearer ${accessToken}`
      }
    }
  )
}
