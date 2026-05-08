import { normalizeTenantSlug } from '@/services/tenant-slug'
import type { LoginForm } from '@/services/types'

export function createLoginForm(): LoginForm {
  return {
    email: '',
    password: '',
    confirmPassword: '',
    companyName: '',
    tenantSlug: '',
    isRegistering: false,
    acceptTerms: false
  }
}

export type PendingProvisioningMetadata = {
  companyName: string
  tenantSlug: string
}

export function resolvePendingProvisioningMetadata(metadata?: {
  company_name?: string
  tenant_slug?: string
}): PendingProvisioningMetadata | null {
  const companyName = metadata?.company_name?.trim() ?? ''
  const tenantSlug = normalizeTenantSlug(metadata?.tenant_slug ?? '')
  if (!companyName || !tenantSlug) {
    return null
  }

  return {
    companyName,
    tenantSlug
  }
}
