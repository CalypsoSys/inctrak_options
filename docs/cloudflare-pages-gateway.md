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
  -> shared.inctrak.com/api/* Pages Function gateway
  -> Cloudflare Tunnel hostname
  -> Dockerized ASP.NET Core API in the lab
```

For this repo as it exists today, that is the cleanest deployable pattern because:

- `frontend/` already has a Pages Function API gateway
- `inctrak.com` already posts its contact form to `https://shared.inctrak.com/api/...`
- `frontend-signup/` and `frontend-vesting/` can point at `https://shared.inctrak.com/api` without additional code changes

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

- `api-origin.inctrak.com`

That hostname is not where users browse the product. It is the upstream origin used by the Cloudflare Pages gateway.

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
- Build command: `npm ci && npm run build`
- Build output directory: `dist`

Pages Function:

```text
frontend/functions/api/[[path]].ts
```

Required Pages environment variables:

- `API_BASE_URL=https://api-origin.inctrak.com/api`
- `INTERNAL_API_KEY=<same value as AppSettings__GatewaySecret on the API>`
- `VITE_SIGNUP_APP_URL=https://signup.inctrak.com`
- `VITE_VESTING_APP_URL=https://vesting.inctrak.com`

Notes:

- This site is the primary browser-facing API gateway.
- Requests to `/api/*` are proxied to the lab API and get `X-Internal-Api-Key` automatically.
- This is also the current cross-origin API target for the marketing site, signup site, and vesting site.

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
- Build command: `npm ci && npm run build`
- Build output directory: `dist`

Required Pages environment variables:

- `VITE_API_BASE_URL=https://shared.inctrak.com/api`
- `VITE_MAIN_APP_LOGIN_URL=https://shared.inctrak.com/login`

Notes:

- This app does not currently include its own Pages Function gateway.
- In the current repo, the simplest production setup is to call the shared gateway at
  `https://shared.inctrak.com/api`.
- If you later want same-origin `/api/*` on this app too, copy the existing gateway function pattern from
  `frontend/`.

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
- Build command: `npm ci && npm run build`
- Build output directory: `dist`

Required Pages environment variables:

- `VITE_API_BASE_URL=https://shared.inctrak.com/api`

Notes:

- Like signup, this app currently uses the shared gateway in production.
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

- The marketing contact form already posts directly to:

```text
https://shared.inctrak.com/api/feedback/save_message/
```

- That means the marketing site depends on the shared SPA gateway being live.
- No extra Pages Function is needed in `inctrak.com/` unless you want to make that site fully independent later.

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
  - Cloudflare Pages `INTERNAL_API_KEY` for `shared.inctrak.com`

## CORS expectations

Because some sites call `https://shared.inctrak.com/api` cross-origin, the API must allow these origins in
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
  - hostname: api-origin.inctrak.com
    service: http://127.0.0.1:8080
  - service: http_status:404
```

This assumes the Dockerized API listens on:

```text
http://127.0.0.1:8080
```

on the Ubuntu host.

## Recommended first production sequence

1. Bring up the Dockerized API and PostgreSQL stack in the lab.
2. Bring up the Cloudflare Tunnel hostname `api-origin.inctrak.com`.
3. Configure `shared.inctrak.com` Pages with:
   - `API_BASE_URL=https://api-origin.inctrak.com/api`
   - `INTERNAL_API_KEY=<gateway secret>`
4. Deploy `shared.inctrak.com`.
5. Deploy `signup.inctrak.com` with `VITE_API_BASE_URL=https://shared.inctrak.com/api`.
6. Deploy `vesting.inctrak.com` with `VITE_API_BASE_URL=https://shared.inctrak.com/api`.
7. Deploy `inctrak.com`, `docs.inctrak.com`, and `blog.inctrak.com`.
8. Smoke-test:
   - marketing contact form
   - vesting interpret/calculate/contact
   - signup page render
   - main login page render

## Related docs

- [inctrak_ubuntu_host_preparation.md](/home/joe/dotnet/inctrak_options/docs/inctrak_ubuntu_host_preparation.md:1)
- [inctrak_production_runbook.md](/home/joe/dotnet/inctrak_options/docs/inctrak_production_runbook.md:1)
