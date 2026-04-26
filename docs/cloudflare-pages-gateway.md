# Cloudflare Pages API Gateway

- local development: Vite proxies `/api/*` to the local `shared.inctrak.com` API
- deployed environments: Cloudflare Pages Functions proxy `/api/*` to the API origin

## Local development

The frontend calls relative `/api/*` URLs.

Vite proxies those requests to `VITE_API_PROXY_TARGET`, which defaults to:

```bash
http://localhost:5000
```

Example:

```bash
cd frontend
VITE_API_PROXY_TARGET=http://localhost:5000 npm run dev
```

The API skips gateway-secret enforcement in `Development`.

## Cloudflare Pages Functions

The Pages Function entrypoint is:

```text
frontend/functions/api/[[path]].ts
```

Recommended Cloudflare Pages build settings:

- Root directory: `frontend`
- Framework preset: `None`
- Build command: `npm ci && npm run build`
- Build output directory: `dist`

Required environment variables:

- `API_BASE_URL`
- `INTERNAL_API_KEY`

The Pages Function forwards requests to the upstream API and adds the internal API key using the configured backend
gateway header. The default header name is `X-Internal-Api-Key`.

## ASP.NET Core configuration

The backend now reads the following from `AppSettings__...` environment variables:

- `AllowedOrigins`
- `RequireGatewaySecret`
- `GatewaySecretHeaderName`
- `GatewaySecret`

Recommended deployment behavior:

- `Development`: do not require the gateway secret
- `Staging` / `Production`: set `AppSettings__RequireGatewaySecret=true` and configure `AppSettings__GatewaySecret`
