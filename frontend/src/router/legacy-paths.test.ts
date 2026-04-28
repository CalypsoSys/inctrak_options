import { describe, expect, it } from 'vitest'
import { resolveLegacyPath } from '@/router/legacy-paths'

describe('resolveLegacyPath', () => {
  it('routes retired legacy auth links back to login', () => {
    expect(resolveLegacyPath('/activateaccount/abc123')).toBe('/auth/login')
    expect(resolveLegacyPath('/resetpasswordlink/token-1')).toBe('/auth/login')
  })

  it('maps legacy admin routes into renamed admin routes', () => {
    expect(resolveLegacyPath('/company_stockclasses/-1')).toBe('/admin/stock-classes/')
    expect(resolveLegacyPath('/company_grants/grant-42')).toBe('/admin/grants/grant-42')
  })
})
