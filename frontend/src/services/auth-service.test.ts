import { describe, expect, it } from 'vitest'
import { createLoginForm, resolvePendingProvisioningMetadata } from '@/services/auth-service'

describe('auth-service', () => {
  it('creates a login form without legacy server defaults', () => {
    expect(createLoginForm()).toEqual({
      email: '',
      password: '',
      confirmPassword: '',
      companyName: '',
      tenantSlug: '',
      isRegistering: false,
      acceptTerms: false
    })
  })

  it('resolves pending provisioning metadata from supabase user metadata', () => {
    expect(resolvePendingProvisioningMetadata({
      company_name: 'Calypso Systems',
      tenant_slug: 'calypso-systems'
    })).toEqual({
      companyName: 'Calypso Systems',
      tenantSlug: 'calypso-systems'
    })
  })

  it('returns null when signup metadata is incomplete', () => {
    expect(resolvePendingProvisioningMetadata({
      company_name: 'Calypso Systems'
    })).toBeNull()
  })
})
