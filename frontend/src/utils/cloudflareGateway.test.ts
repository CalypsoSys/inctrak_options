import { describe, expect, it } from 'vitest'
import { buildGatewayUpstreamUrl } from '@/utils/cloudflareGateway'

describe('buildGatewayUpstreamUrl', () => {
  it('maps relative api paths to the upstream api base url', () => {
    expect(buildGatewayUpstreamUrl(
      'https://frontend.example.com/api/login/get_creds/?x=1',
      'https://shared.inctrak.com/api'
    )).toBe('https://shared.inctrak.com/api/login/get_creds/?x=1')
  })
})
