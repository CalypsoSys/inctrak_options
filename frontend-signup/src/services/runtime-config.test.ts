import { describe, expect, it, vi } from 'vitest'
import { buildApiUrl, buildMainAppLoginUrl } from '@/services/runtime-config'

describe('runtime-config', () => {
  it('keeps relative api paths when no base url is configured', () => {
    expect(buildApiUrl('/api/demo')).toBe('/api/demo')
  })

  it('uses the local frontend login url on localhost', () => {
    vi.stubGlobal('window', {
      location: {
        hostname: '127.0.0.1'
      }
    })

    expect(buildMainAppLoginUrl()).toBe('http://127.0.0.1:5174/login')
  })
})
