import test from 'node:test'
import assert from 'node:assert/strict'
import { renderConfigEnv } from './render-config-env.mjs'

test('renderConfigEnv flattens nested settings and arrays', async () => {
  const lines = await renderConfigEnv(`
AppSettings:
  AllowedOrigins:
    - https://inctrak.com
    - https://www.inctrak.com
  RequireGatewaySecret: true
`, 'env')

  const rendered = lines.join('\n')
  assert.match(rendered, /AppSettings__AllowedOrigins__0=https:\/\/inctrak\.com/)
  assert.match(rendered, /AppSettings__AllowedOrigins__1=https:\/\/www\.inctrak\.com/)
  assert.match(rendered, /AppSettings__RequireGatewaySecret=true/)
})

test('renderConfigEnv resolves placeholders in shell output', async () => {
  process.env.INCTRAK_GATEWAY_SECRET = 'gateway-secret'

  const lines = await renderConfigEnv(`
AppSettings:
  GatewaySecret: \${INCTRAK_GATEWAY_SECRET}
`, 'shell')

  assert.match(lines.join('\n'), /export AppSettings__GatewaySecret='gateway-secret'/)
})

test('renderConfigEnv fails when a required environment variable is missing', async () => {
  delete process.env.MISSING_SECRET

  await assert.rejects(
    renderConfigEnv('AppSettings:\n  GatewaySecret: ${MISSING_SECRET}\n', 'env'),
    /required environment variable is not set: MISSING_SECRET/
  )
})
