import { describe, expect, it } from 'vitest'
import { FRONTEND_VERSION, getBackendVersion } from '@/services/app-version'

describe('app-version', () => {
  it('exposes the public frontend version', () => {
    expect(FRONTEND_VERSION).toBe('v1.0.0')
  })

  it('reads backend version response casing variants', () => {
    expect(getBackendVersion({ success: true, backendVersion: 'v1.0.0' })).toBe('v1.0.0')
    expect(getBackendVersion({ success: true, BackendVersion: 'v1.0.1' })).toBe('v1.0.1')
    expect(getBackendVersion({ success: true })).toBe('unknown')
  })
})
