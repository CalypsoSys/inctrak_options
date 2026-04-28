import { describe, expect, it } from 'vitest'
import { normalizeTenantSlug } from '@/services/tenant-slug'

describe('tenant-slug', () => {
  it('normalizes a company name into a safe slug', () => {
    expect(normalizeTenantSlug('Calypso Systems, LLC')).toBe('calypso-systems-llc')
  })
})
