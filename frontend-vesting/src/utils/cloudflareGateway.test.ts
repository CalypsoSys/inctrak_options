import { describe, expect, it } from 'vitest'
import { buildGatewayForwardHeaders, buildGatewayUpstreamUrl } from '@/utils/cloudflareGateway'

describe('buildGatewayUpstreamUrl', () => {
  it('maps vesting api paths to the upstream api base url', () => {
    expect(buildGatewayUpstreamUrl(
      'https://vesting.inctrak.com/api/vesting/quick/calculate?preview=true',
      'https://api.inctrak.com/api'
    )).toBe('https://api.inctrak.com/api/vesting/quick/calculate?preview=true')
  })

  it('forwards the original request host to the upstream api', () => {
    const headers = buildGatewayForwardHeaders(
      'https://vesting.inctrak.com/api/vesting/quick/calculate',
      {
        Host: 'attacker.example.com',
        Authorization: 'Bearer token'
      },
      'gateway-secret'
    )

    expect(headers.get('X-Forwarded-Host')).toBe('vesting.inctrak.com')
    expect(headers.get('X-Forwarded-Proto')).toBe('https')
    expect(headers.get('X-Internal-Api-Key')).toBe('gateway-secret')
    expect(headers.get('Authorization')).toBe('Bearer token')
    expect(headers.has('Host')).toBe(false)
  })
})
