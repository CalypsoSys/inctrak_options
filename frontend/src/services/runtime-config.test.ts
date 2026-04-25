import { describe, expect, it } from 'vitest'
import { buildApiUrl } from '@/services/runtime-config'

describe('buildApiUrl', () => {
  it('builds relative API paths from runtime config', () => {
    window.IncTrakSiteConfig = { apiBaseUrl: 'https://api.example.com/' }
    expect(buildApiUrl('/api/login/get_creds/')).toBe('https://api.example.com/api/login/get_creds/')
  })

  it('keeps absolute URLs unchanged', () => {
    expect(buildApiUrl('https://api.example.com/health')).toBe('https://api.example.com/health')
  })
})
