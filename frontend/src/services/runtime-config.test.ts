import { describe, expect, it, vi } from 'vitest'
import { buildApiUrl, buildSignupAppUrl } from '@/services/runtime-config'

describe('buildApiUrl', () => {
  it('keeps relative api paths relative by default', () => {
    expect(buildApiUrl('/api/company/summary/')).toBe('/api/company/summary/')
  })

  it('keeps absolute URLs unchanged', () => {
    expect(buildApiUrl('https://api.example.com/health')).toBe('https://api.example.com/health')
  })

  it('uses the local public signup url on localhost', () => {
    vi.stubGlobal('window', {
      location: {
        hostname: '127.0.0.1'
      }
    })

    expect(buildSignupAppUrl()).toBe('http://127.0.0.1:5177')
  })
})
