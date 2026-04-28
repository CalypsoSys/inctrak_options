import { describe, expect, it } from 'vitest'
import { buildApiUrl } from '@/services/runtime-config'

describe('buildApiUrl', () => {
  it('keeps relative api paths relative by default', () => {
    expect(buildApiUrl('/api/company/summary/')).toBe('/api/company/summary/')
  })

  it('keeps absolute URLs unchanged', () => {
    expect(buildApiUrl('https://api.example.com/health')).toBe('https://api.example.com/health')
  })
})
