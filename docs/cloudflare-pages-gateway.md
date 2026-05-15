# Cloudflare Pages and API Gateway

This repo now deploys as a small family of sites, not a single frontend:

- `inctrak.com` = marketing site
- `shared.inctrak.com` = main shared SPA for login, admin, and participant flows
- `signup.inctrak.com` = public signup app
- `vesting.inctrak.com` = public vesting calculator
- `docs.inctrak.com` = product documentation
- `blog.inctrak.com` = static blog

The backend API stays in your lab on Docker and is exposed through Cloudflare Tunnel. The practical production
shape is:

```text
Browser
  -> Cloudflare Pages site
  -> same-origin /api/* Pages Function gateway
  -> Cloudflare Tunnel hostname
  -> host-installed Caddy
  -> Dockerized ASP.NET Core API in the lab
```

For this repo as it exists today, that is the cleanest deployable pattern because:

- `frontend/`, `frontend-signup/`, `frontend-vesting/`, and `inctrak.com` have Pages Function API gateways
- public browser code calls same-origin `/api/*`, and Pages Functions inject the internal API key server-side
- Caddy gives the lab host one stable local ingress layer behind Cloudflare Tunnel

## Recommended production topology

Use these public hostnames:

- `inctrak.com`
- `www.inctrak.com`
- `shared.inctrak.com`
- `signup.inctrak.com`
- `vesting.inctrak.com`
- `docs.inctrak.com`
- `blog.inctrak.com`

Use one tunnel-backed hostname for the real API origin, for example:

- `api.inctrak.com`

That hostname is not where users browse the product. It is the upstream origin used by the Cloudflare Pages gateway.

## Wildcard tenant subdomains

Tenant workspaces use hostnames like:

```text
joe.inctrak.com
apple.inctrak.com
```

Cloudflare Pages custom domains and Worker custom domains do not support one wildcard custom domain for all tenant
subdomains. Use a proxied wildcard DNS record plus a Worker route or tunnel route instead.

Recommended Cloudflare DNS shape:

```text
*.inctrak.com CNAME <your-tunnel-id>.cfargotunnel.com Proxied
```

Recommended Worker route shape when a Worker sits in front of tenant traffic:

```text
*.inctrak.com/*
```

Exact hostnames such as `shared.inctrak.com`, `signup.inctrak.com`, `vesting.inctrak.com`, `docs.inctrak.com`, and
`blog.inctrak.com` should remain explicit records/routes so they can keep their current Pages projects and static-site
behavior.

When tenant traffic reaches the API through a gateway, the gateway must preserve the original tenant host with:

```text
X-Forwarded-Host: <tenant>.inctrak.com
X-Forwarded-Proto: https
```

The API uses `X-Forwarded-Host` through ASP.NET Core forwarded headers, resolves the host in `cp_tenant_domains`, and
then selects the tenant database from `cp_tenants.tenant_db_name`. Do not rely on browser-supplied tenant database
headers in production.

## Site matrix

### 1. Main shared SPA

Directory:

```text
frontend/
```

Custom domain:

```text
shared.inctrak.com
```

Build settings:

- Root directory: `frontend`
- Framework preset: `None`
- Build command: `pnpm install --frozen-lockfile && pnpm run build`
- Build output directory: `dist`

Pages Function:

```text
frontend/functions/api/[[path]].ts
```

Required Pages environment variables:

- `API_BASE_URL=https://api.inctrak.com/api`
- `INTERNAL_API_KEY=<same value as AppSettings__GatewaySecret on the API>`
- `VITE_SIGNUP_APP_URL=https://signup.inctrak.com`
- `VITE_VESTING_APP_URL=https://vesting.inctrak.com`

Notes:

- This site has a browser-facing API gateway.
- Requests to `/api/*` are proxied to the lab API and get `X-Internal-Api-Key` automatically.
- The marketing site still posts its contact form to this gateway.

### 2. Public signup app

Directory:

```text
frontend-signup/
```

Custom domain:

```text
signup.inctrak.com
```

Build settings:

- Root directory: `frontend-signup`
- Framework preset: `None`
- Build command: `pnpm install --frozen-lockfile && pnpm run build`
- Build output directory: `dist`

Required Pages environment variables:

- `API_BASE_URL=https://api.inctrak.com/api`
- `INTERNAL_API_KEY=<same value as AppSettings__GatewaySecret on the API>`
- `VITE_MAIN_APP_LOGIN_URL=https://shared.inctrak.com/login`

Notes:

- This app includes its own Pages Function gateway at `frontend-signup/functions/api/[[path]].ts`.
- Leave `VITE_API_BASE_URL` unset in production so browser calls remain same-origin under `/api/*`.

### 3. Public vesting app

Directory:

```text
frontend-vesting/
```

Custom domain:

```text
vesting.inctrak.com
```

Build settings:

- Root directory: `frontend-vesting`
- Framework preset: `None`
- Build command: `pnpm install --frozen-lockfile && pnpm run build`
- Build output directory: `dist`

Required Pages environment variables:

- `API_BASE_URL=https://api.inctrak.com/api`
- `INTERNAL_API_KEY=<same value as AppSettings__GatewaySecret on the API>`

Notes:

- This app includes its own Pages Function gateway at `frontend-vesting/functions/api/[[path]].ts`.
- Leave `VITE_API_BASE_URL` unset in production so browser calls remain same-origin under `/api/*`.
- Public vesting interpret, calculate, and contact submissions all end up at the backend API.

### 4. Marketing site

Directory:

```text
inctrak.com/
```

Custom domains:

- `inctrak.com`
- `www.inctrak.com`

Recommended Pages settings:

- Root directory: `inctrak.com`
- Framework preset: `None`
- Build command: none
- Build output directory: `.`

Notes:

- The marketing contact form posts to same-origin `/api/feedback/save_message/`.
- This site includes its own Pages Function gateway at `inctrak.com/functions/api/[[path]].js`.
- Required Pages environment variables:

```text
API_BASE_URL=https://api.inctrak.com/api
INTERNAL_API_KEY=<same value as AppSettings__GatewaySecret on the API>
```

### 5. Docs site

Directory:

```text
docs.inctrak.com/
```

Custom domain:

```text
docs.inctrak.com
```

Recommended Pages settings:

- Root directory: `docs.inctrak.com`
- Framework preset: `None`
- Build command: none
- Build output directory: `.`

Notes:

- Pure static content
- No API dependency today

### 6. Blog site

Directory:

```text
blog.inctrak.com/
```

Custom domain:

```text
blog.inctrak.com
```

Recommended Pages settings:

- Root directory: `blog.inctrak.com`
- Framework preset: `None`
- Build command: none
- Build output directory: `.`

Notes:

- Pure static content in the current repo

## Local development

Current local ports:

- `frontend`: `http://127.0.0.1:5174`
- `frontend-vesting`: `http://127.0.0.1:5176`
- `frontend-signup`: `http://127.0.0.1:5177`
- API: `http://localhost:5000`

The Vite apps proxy `/api/*` to `VITE_API_PROXY_TARGET`, which defaults to:

```text
http://localhost:5000
```

## API gateway secret

The backend supports gateway-secret enforcement through:

- `AppSettings__RequireGatewaySecret`
- `AppSettings__GatewaySecretHeaderName`
- `AppSettings__GatewaySecret`

Recommended production behavior:

- set `AppSettings__RequireGatewaySecret=true`
- keep `GatewaySecretHeaderName=X-Internal-Api-Key`
- configure the same secret value in:
  - backend `AppSettings__GatewaySecret`
  - Cloudflare Pages `INTERNAL_API_KEY` for every Pages project that has a `/api/*` function

## CORS expectations

Because the Pages gateways forward browser request headers to the backend, the API must allow these origins in
production:

- `https://inctrak.com`
- `https://www.inctrak.com`
- `https://shared.inctrak.com`
- `https://signup.inctrak.com`
- `https://vesting.inctrak.com`

`docs.inctrak.com` and `blog.inctrak.com` do not currently need API access.

Recommended production value for `AppSettings:AllowedOrigins`:

```yaml
AppSettings:
  AllowedOrigins:
    - https://inctrak.com
    - https://www.inctrak.com
    - https://shared.inctrak.com
    - https://signup.inctrak.com
    - https://vesting.inctrak.com
```

## Cloudflare Tunnel hostname

Recommended tunnel ingress shape:

```yaml
tunnel: <your-tunnel-id>
credentials-file: /etc/cloudflared/<your-tunnel-id>.json

ingress:
  - hostname: api.inctrak.com
    service: http://127.0.0.1:80
  - service: http_status:404
```

This assumes host-installed Caddy listens on:

```text
http://127.0.0.1:80
```

and proxies `api.inctrak.com` to the Dockerized API at:

```text
http://127.0.0.1:8082
```

## Recommended first production sequence

1. Bring up the Dockerized API and PostgreSQL stack in the lab.
2. Bring up host-installed Caddy with `api.inctrak.com` proxying to `127.0.0.1:8082`.
3. Bring up the Cloudflare Tunnel hostname `api.inctrak.com` pointing to `127.0.0.1:80`.
4. Configure `shared.inctrak.com` Pages with:
   - `API_BASE_URL=https://api.inctrak.com/api`
   - `INTERNAL_API_KEY=<gateway secret>`
5. Configure `signup.inctrak.com`, `vesting.inctrak.com`, and `inctrak.com` Pages with the same `API_BASE_URL` and `INTERNAL_API_KEY`.
6. Deploy `shared.inctrak.com`, `signup.inctrak.com`, `vesting.inctrak.com`, and `inctrak.com`.
7. Deploy `docs.inctrak.com` and `blog.inctrak.com`.
8. Smoke-test:
   - marketing contact form
   - vesting interpret/calculate/contact
   - signup page render
   - main login page render

## Related docs

- [inctrak_ubuntu_host_preparation.md](inctrak_ubuntu_host_preparation.md)
- [inctrak_production_runbook.md](inctrak_production_runbook.md)
