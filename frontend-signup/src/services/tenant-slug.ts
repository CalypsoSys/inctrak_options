import { apiGet } from '@/services/api'

export type TenantSlugAvailabilityResponse = {
  success: boolean
  TenantSlug?: string
  Available?: boolean
  Message?: string
}

export function normalizeTenantSlug(value: string): string {
  return value
    .trim()
    .toLowerCase()
    .replace(/[^a-z0-9-]+/g, '-')
    .replace(/-{2,}/g, '-')
    .replace(/^-+|-+$/g, '')
    .slice(0, 63)
}

export function fetchTenantSlugAvailability(tenantSlug: string): Promise<TenantSlugAvailabilityResponse> {
  const normalized = normalizeTenantSlug(tenantSlug)
  return apiGet<TenantSlugAvailabilityResponse>(`/api/control-plane/slug-availability?tenantSlug=${encodeURIComponent(normalized)}`)
}
