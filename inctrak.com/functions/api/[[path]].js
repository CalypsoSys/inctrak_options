export async function onRequest(context) {
  const { request, env } = context
  const apiBaseUrl = env.API_BASE_URL

  if (!apiBaseUrl) {
    return new Response('Missing API_BASE_URL configuration.', { status: 500 })
  }

  const headers = buildGatewayForwardHeaders(request.url, request.headers, env.INTERNAL_API_KEY)
  const upstreamResponse = await fetch(buildGatewayUpstreamUrl(request.url, apiBaseUrl), {
    method: request.method,
    headers,
    body: request.method === 'GET' || request.method === 'HEAD' ? undefined : request.body,
    redirect: 'manual'
  })

  const responseHeaders = new Headers(upstreamResponse.headers)
  responseHeaders.set('X-Api-Gateway', 'cloudflare-pages-function')

  return new Response(upstreamResponse.body, {
    status: upstreamResponse.status,
    statusText: upstreamResponse.statusText,
    headers: responseHeaders
  })
}

function buildGatewayUpstreamUrl(requestUrl, apiBaseUrl) {
  const request = new URL(requestUrl)
  const upstream = new URL(apiBaseUrl)
  const upstreamPath = request.pathname.replace(/^\/api/, '') || '/'

  upstream.pathname = `${upstream.pathname.replace(/\/$/, '')}${upstreamPath}`
  upstream.search = request.search

  return upstream.toString()
}

function buildGatewayForwardHeaders(requestUrl, requestHeaders, internalApiKey) {
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
