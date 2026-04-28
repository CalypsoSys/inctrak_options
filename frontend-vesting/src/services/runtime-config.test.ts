import { describe, expect, it, vi } from 'vitest'

describe('runtime-config', () => {
  it('keeps relative api paths relative when no base url is configured', async () => {
    vi.stubEnv('VITE_API_BASE_URL', '')
    const { buildApiUrl } = await import('@/services/runtime-config')
    expect(buildApiUrl('/api/optionee/quick/')).toBe('/api/optionee/quick/')
  })
})
