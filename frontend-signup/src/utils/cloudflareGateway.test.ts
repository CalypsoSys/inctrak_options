import { describe, expect, it } from 'vitest'
import { buildGatewayForwardHeaders, buildGatewayUpstreamUrl } from '@/utils/cloudflareGateway'

describe('buildGatewayUpstreamUrl', () => {
  it('maps signup api paths to the upstream api base url', () => {
    expect(buildGatewayUpstreamUrl(
      'https://signup.inctrak.com/api/signup/workspace?ref=home',
      'https://api.inctrak.com/api'
    )).toBe('https://api.inctrak.com/api/signup/workspace?ref=home')
  })

  it('forwards the original request host to the upstream api', () => {
    const headers = buildGatewayForwardHeaders(
      'https://signup.inctrak.com/api/control-plane/signup',
      {
        Host: 'attacker.example.com',
        Authorization: 'Bearer token'
      },
      'gateway-secret'
    )

    expect(headers.get('X-Forwarded-Host')).toBe('signup.inctrak.com')
    expect(headers.get('X-Forwarded-Proto')).toBe('https')
    expect(headers.get('X-Internal-Api-Key')).toBe('gateway-secret')
    expect(headers.get('Authorization')).toBe('Bearer token')
    expect(headers.has('Host')).toBe(false)
  })
})
