import { buildGatewayForwardHeaders, buildGatewayUpstreamUrl } from '../../src/utils/cloudflareGateway.js'

export async function onRequest(context: {
  request: Request
  env: {
    API_BASE_URL?: string
    INTERNAL_API_KEY?: string
  }
}) {
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
