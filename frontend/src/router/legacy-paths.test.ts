import { describe, expect, it } from 'vitest'
import { resolveLegacyPath } from '@/router/legacy-paths'

describe('resolveLegacyPath', () => {
  it('maps legacy auth links into renamed auth routes', () => {
    expect(resolveLegacyPath('/activateaccount/abc123')).toBe('/auth/activate/abc123')
    expect(resolveLegacyPath('/resetpasswordlink/token-1')).toBe('/auth/reset-password/token-1')
  })

  it('maps legacy admin routes into renamed admin routes', () => {
    expect(resolveLegacyPath('/company_stockclasses/-1')).toBe('/admin/stock-classes/')
    expect(resolveLegacyPath('/company_grants/grant-42')).toBe('/admin/grants/grant-42')
  })
})
