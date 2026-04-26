export function buildGatewayUpstreamUrl(requestUrl: string, apiBaseUrl: string): string {
  const request = new URL(requestUrl)
  const upstream = new URL(apiBaseUrl)
  const upstreamPath = request.pathname.replace(/^\/api/, '') || '/'

  upstream.pathname = `${upstream.pathname.replace(/\/$/, '')}${upstreamPath}`
  upstream.search = request.search

  return upstream.toString()
}
