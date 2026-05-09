export function buildGatewayUpstreamUrl(requestUrl: string, apiBaseUrl: string): string {
  const request = new URL(requestUrl)
  const upstream = new URL(apiBaseUrl)
  const upstreamPath = request.pathname.replace(/^\/api/, '') || '/'

  upstream.pathname = `${upstream.pathname.replace(/\/$/, '')}${upstreamPath}`
  upstream.search = request.search

  return upstream.toString()
}

export function buildGatewayForwardHeaders(
  requestUrl: string,
  requestHeaders: HeadersInit,
  internalApiKey?: string
): Headers {
  const request = new URL(requestUrl)
  const headers = new Headers(requestHeaders)

  headers.set('Accept', 'application/json')
  headers.set('X-Forwarded-Host', request.host)
  headers.set('X-Forwarded-Proto', request.protocol.replace(':', ''))

  if (internalApiKey) {
    headers.set('X-Internal-Api-Key', internalApiKey)
  }

  headers.delete('Host')

  return headers
}
