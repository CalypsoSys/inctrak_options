import { describe, expect, it } from 'vitest'
import { buildApiUrl } from '@/services/runtime-config'

describe('buildApiUrl', () => {
  it('keeps relative api paths relative by default', () => {
    expect(buildApiUrl('/api/login/resetpassword/')).toBe('/api/login/resetpassword/')
  })

  it('keeps absolute URLs unchanged', () => {
    expect(buildApiUrl('https://api.example.com/health')).toBe('https://api.example.com/health')
  })
})
