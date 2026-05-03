# IncTrak production runbook

This runbook is the single operational document for deploying the IncTrak API in your lab, wiring it to
Cloudflare, and validating the multi-site production setup.

Related docs:

- [cloudflare-pages-gateway.md](/home/joe/dotnet/inctrak_options/docs/cloudflare-pages-gateway.md:1)
- [inctrak_ubuntu_host_preparation.md](/home/joe/dotnet/inctrak_options/docs/inctrak_ubuntu_host_preparation.md:1)
- [inctrak_local_vscode.md](/home/joe/dotnet/inctrak_options/docs/inctrak_local_vscode.md:1)

## Intended production shape

Public sites on Cloudflare:

- `inctrak.com`
- `shared.inctrak.com`
- `signup.inctrak.com`
- `vesting.inctrak.com`
- `docs.inctrak.com`
- `blog.inctrak.com`

Private-ish lab origin behind Cloudflare Tunnel:

- `api-origin.inctrak.com`

Server-side stack in the lab:

- PostgreSQL in Docker
- `shared.inctrak.com` API in Docker
- `cloudflared` service on the host

## Directory layout on the server

Expected structure:

```text
/srv/stacks/inctrak
/srv/stacks/inctrak/api
  docker-compose.yml
  config.yaml
  scripts/
    compose-inctrak.sh
    render-config-env

/srv/stacks/inctrak/secrets
/srv/backups/inctrak/incoming
/srv/backups/inctrak/archive
/srv/backups/postgres
/srv/logs/inctrak/api
/srv/logs/inctrak/postgres
```

## Files that come from this repo

Copy or derive these from the repo:

- `shared.inctrak.com/Dockerfile`
- `docker/inctrak/docker-compose.yml`
- `scripts/inctrak/compose-inctrak.sh`
- `scripts/inctrak/config.example.yaml`
- `inctrak.db/control_plane.sql`
- `inctrak.db/inctrak.sql`
- `inctrak.db/inctrak_feedback.sql`
- the built API image tarball you create locally

## Server-local files that should not come from git

- `/srv/stacks/inctrak/api/config.yaml`
- Cloudflare Tunnel credentials
- real secrets and passwords

## 1. Build the API image locally in WSL

From the repo root in WSL:

```bash
cd /home/joe/dotnet/inctrak_options
mkdir -p /mnt/c/transfer
if [ -f /mnt/c/transfer/inctrak-api-latest.tar.gz ]; then mv /mnt/c/transfer/inctrak-api-latest.tar.gz /mnt/c/transfer/inctrak-api-latest.lastgood.tar.gz; fi
docker build --platform linux/amd64 -t inctrak-api:latest ./shared.inctrak.com
docker save inctrak-api:latest -o /mnt/c/transfer/inctrak-api-latest.tar
gzip -f /mnt/c/transfer/inctrak-api-latest.tar
```

That leaves:

```text
C:\transfer\inctrak-api-latest.tar.gz
```

## 2. Prepare the production config.yaml

On your workstation, copy the example config as a starting point:

```bash
cp scripts/inctrak/config.example.yaml /tmp/inctrak-config.production.yaml
```

Then adapt it for production. Recommended example:

```yaml
INCTRAK_API_IMAGE: inctrak-api:latest
INCTRAK_POSTGRES_IMAGE: postgres:18

ASPNETCORE_ENVIRONMENT: Production
INCTRAK_API_HOST_BIND: 127.0.0.1
INCTRAK_API_HOST_PORT: 8080
INCTRAK_POSTGRES_HOST_BIND: 127.0.0.1
INCTRAK_POSTGRES_HOST_PORT: 5432
INCTRAK_LOGS_HOST_PATH: /srv/logs/inctrak/api
INCTRAK_POSTGRES_LOGS_HOST_PATH: /srv/logs/inctrak/postgres

POSTGRES_DB: postgres
POSTGRES_USER: postgres
POSTGRES_PASSWORD: ${INCTRAK_DB_PASSWORD}

AppSettings:
  AccessLogPath: /app/logs/access.log
  ErrorLogPath: /app/logs/errors.log
  IncTrakDns: https://shared.inctrak.com
  AllowedOrigins:
    - https://inctrak.com
    - https://www.inctrak.com
    - https://shared.inctrak.com
    - https://signup.inctrak.com
    - https://vesting.inctrak.com
  RequireGatewaySecret: true
  GatewaySecretHeaderName: X-Internal-Api-Key
  GatewaySecret: ${INCTRAK_GATEWAY_SECRET}
  RateLimit:
    Enabled: true
    PermitLimit: 120
    WindowSeconds: 60
    QueueLimit: 0
  ControlPlaneConnection: Host=postgres;Port=5432;Database=inctrak_control;Username=postgres;Password=${INCTRAK_DB_PASSWORD}
  FeedbackConnection: Host=postgres;Port=5432;Database=inctrak_feedback;Username=postgres;Password=${INCTRAK_DB_PASSWORD}
  IncTrakConnection: Host=postgres;Port=5432;Database=inctrak_template;Username=postgres;Password=${INCTRAK_DB_PASSWORD}
  SupabaseUrl: ${INCTRAK_SUPABASE_URL}
  SupabaseAnonKey: ${INCTRAK_SUPABASE_PUBLISHABLE_KEY}
  SupabaseJwtSecret: ${INCTRAK_SUPABASE_JWT_SECRET}
  SlackFeedbackWebhookUrl: ${INCTRAK_SLACK_FEEDBACK_WEBHOOK_URL}
  TenantTemplateDatabaseName: inctrak_template
  TenantDatabasePrefix: inctrak_
  LocalAiModelPath: ${INCTRAK_LOCAL_AI_MODEL_PATH}
  LocalAiContextSize: 4096
  LocalAiGpuLayerCount: 999
  LocalAiMaxTokens: 512
  LocalAiEndpoint: ${INCTRAK_LOCAL_AI_ENDPOINT}
  LocalAiModel: ${INCTRAK_LOCAL_AI_MODEL}
  LocalAiApiKey: ${INCTRAK_LOCAL_AI_API_KEY}
```

Notes:

- PostgreSQL data persistence now lives in the named Docker volume `inctrak_postgres_data`
- `IncTrakConnection` is still present, but for production it is safest to point it at the real template database,
  not a dead `inctrak` database
- if you do not want local AI enabled on the server, leave those values blank

## 3. Build the shared YAML-to-env renderer

Build the shared renderer from the public repo:

```bash
cd /home/joe/gocode/babalu-yaml-env
go build -o /mnt/c/transfer/render-config-env ./cmd/babalu-yaml-env
```

That gives you:

```text
C:\transfer\render-config-env
```

## 4. Create the Docker Compose stack file

On the Ubuntu host, create:

```text
/srv/stacks/inctrak/api/docker-compose.yml
```

Recommended source file from this repo:

```text
docker/inctrak/docker-compose.yml
```

## 5. Create the server `config.yaml`

On the Ubuntu host:

```bash
cd /srv/stacks/inctrak/api
vi config.yaml
chmod 600 config.yaml
```

Important note:

- `scripts/compose-inctrak.sh` requires a renderer binary
- the default expected path is `scripts/render-config-env` beside the wrapper
- otherwise set `RENDER_BIN=/full/path/to/render-config-env` explicitly

## 6. Stage artifacts into `C:\transfer`

Recommended transfer staging:

```text
C:\transfer\inctrak-api-latest.tar.gz
C:\transfer\inctrak-config.production.yaml
C:\transfer\render-config-env
```

Copy the production config reference into transfer if you prepared it in WSL:

```bash
cp /tmp/inctrak-config.production.yaml /mnt/c/transfer/inctrak-config.production.yaml
```

## 7. Copy artifacts to the server

From Windows PowerShell, for example:

```powershell
scp C:\transfer\inctrak-api-latest.tar.gz joe@calypsoasus:/srv/stacks/inctrak/api/
scp C:\transfer\inctrak-config.production.yaml joe@calypsoasus:/srv/stacks/inctrak/api/config.yaml
scp C:\transfer\render-config-env joe@calypsoasus:/srv/stacks/inctrak/api/scripts/render-config-env
scp .\docker\inctrak\docker-compose.yml joe@calypsoasus:/srv/stacks/inctrak/api/docker-compose.yml
scp .\scripts\inctrak\compose-inctrak.sh joe@calypsoasus:/srv/stacks/inctrak/api/scripts/compose-inctrak.sh
```

After copy, on the Ubuntu host:

```bash
chmod +x /srv/stacks/inctrak/api/scripts/compose-inctrak.sh
chmod +x /srv/stacks/inctrak/api/scripts/render-config-env
```

## 8. Validate the rendered compose config

On the Ubuntu host:

```bash
cd /srv/stacks/inctrak/api
./scripts/compose-inctrak.sh config >/tmp/inctrak-compose.out
tail -n 30 /tmp/inctrak-compose.out
```

If required placeholders are missing, the renderer should fail fast before Docker Compose runs.

If the renderer is installed somewhere else on the host, invoke the wrapper like this:

```bash
RENDER_BIN=/opt/babalu-yaml-env/render-config-env ./scripts/compose-inctrak.sh config
```

## 9. Load the image on the server

On the Ubuntu host:

```bash
cd /srv/stacks/inctrak/api
gunzip -f inctrak-api-latest.tar.gz
docker load -i inctrak-api-latest.tar
```

## 10. Bootstrap PostgreSQL the first time

### Control-plane database

On the host, assuming the `postgres` container is already running:

```bash
docker exec -i inctrak-postgres psql -U postgres -c "CREATE DATABASE inctrak_control;"
docker exec -i inctrak-postgres psql -U postgres -d inctrak_control < /path/to/repo/inctrak.db/control_plane.sql
```

### Feedback database

`inctrak_feedback.sql` includes its own `CREATE DATABASE`, so run it from the default database:

```bash
docker exec -i inctrak-postgres psql -U postgres -d postgres < /path/to/repo/inctrak.db/inctrak_feedback.sql
```

### Template database

Create the real PostgreSQL template database and load `inctrak.sql`:

```bash
docker exec -i inctrak-postgres psql -U postgres -c "CREATE DATABASE inctrak_template;"
docker exec -i inctrak-postgres psql -U postgres -d inctrak_template < /path/to/repo/inctrak.db/inctrak.sql
```

Then mark it as a template:

```bash
docker exec -i inctrak-postgres psql -U postgres -d postgres -c "UPDATE pg_database SET datistemplate = true WHERE datname = 'inctrak_template';"
```

Note:

- `TenantSignupProvisioner` now expects a real PostgreSQL template database.

## 11. Bring up the stack

On the Ubuntu host:

```bash
cd /srv/stacks/inctrak/api
./scripts/compose-inctrak.sh up -d
./scripts/compose-inctrak.sh ps
./scripts/compose-inctrak.sh logs inctrak-api --tail=100
```

Check the API directly on the host:

```bash
curl -i http://127.0.0.1:8080/api/optionee/quick/
```

If gateway-secret enforcement is enabled, direct requests without the internal header should return `401`.

## 12. Bring up the Cloudflare Tunnel

Recommended ingress shape:

```yaml
tunnel: <your-tunnel-id>
credentials-file: /etc/cloudflared/<your-tunnel-id>.json

ingress:
  - hostname: api-origin.inctrak.com
    service: http://127.0.0.1:8080
  - service: http_status:404
```

Run or restart the tunnel:

```bash
sudo systemctl restart cloudflared
sudo systemctl status cloudflared --no-pager
```

## 13. Configure Cloudflare Pages projects

### `shared.inctrak.com`

- Root directory: `frontend`
- Build command: `npm ci && npm run build`
- Output: `dist`
- Custom domain: `shared.inctrak.com`
- Variables:
  - `API_BASE_URL=https://api-origin.inctrak.com/api`
  - `INTERNAL_API_KEY=<same as AppSettings__GatewaySecret>`
  - `VITE_SIGNUP_APP_URL=https://signup.inctrak.com`
  - `VITE_VESTING_APP_URL=https://vesting.inctrak.com`

### `signup.inctrak.com`

- Root directory: `frontend-signup`
- Build command: `npm ci && npm run build`
- Output: `dist`
- Custom domain: `signup.inctrak.com`
- Variables:
  - `VITE_API_BASE_URL=https://shared.inctrak.com/api`
  - `VITE_MAIN_APP_LOGIN_URL=https://shared.inctrak.com/login`

### `vesting.inctrak.com`

- Root directory: `frontend-vesting`
- Build command: `npm ci && npm run build`
- Output: `dist`
- Custom domain: `vesting.inctrak.com`
- Variables:
  - `VITE_API_BASE_URL=https://shared.inctrak.com/api`

### `inctrak.com`

- Root directory: `inctrak.com`
- No build command
- Output: `.`
- Custom domains:
  - `inctrak.com`
  - `www.inctrak.com`

### `docs.inctrak.com`

- Root directory: `docs.inctrak.com`
- No build command
- Output: `.`

### `blog.inctrak.com`

- Root directory: `blog.inctrak.com`
- No build command
- Output: `.`

## 14. Smoke-test checklist

### API and gateway

1. Visit `https://shared.inctrak.com`.
2. Confirm the public shell renders.
3. Confirm `https://shared.inctrak.com/api/optionee/quick/` returns data through the Pages gateway.

### Marketing site

1. Visit `https://inctrak.com`.
2. Submit the contact form.
3. Confirm:
   - Slack message arrives
   - feedback row is written to `inctrak_feedback.MESSAGE`

### Vesting site

1. Visit `https://vesting.inctrak.com`.
2. Run a quick interpret prompt.
3. Run `Calculate Vesting`.
4. Submit the contact popup.
5. Confirm:
   - public vesting Slack usage messages arrive
   - contact message is stored as message type `8`

### Signup and main app

1. Visit `https://signup.inctrak.com`.
2. Confirm the public shell loads.
3. Visit `https://shared.inctrak.com`.
4. Confirm the temporary lockout state shows as expected if that is still enabled.

## 15. Safe redeploy flow

For normal API-only redeploys:

1. Build new image locally in WSL.
2. Stage new `inctrak-api-latest.tar.gz` to `C:\transfer`.
3. Copy to `/srv/stacks/inctrak/api/`.
4. `docker load -i ...`
5. `./scripts/compose-inctrak.sh up -d`
6. Check logs.
7. Re-test:
   - `shared.inctrak.com`
   - `inctrak.com` contact form
   - `vesting.inctrak.com` interpret/calculate/contact

## 16. Notes about current repo behavior

These details matter when you do the first live deploy:

- `frontend/` already has a Pages Function API gateway
- `frontend-signup/` and `frontend-vesting/` currently rely on `VITE_API_BASE_URL` in production
- `inctrak.com` currently posts its contact form directly to `https://shared.inctrak.com/api/feedback/save_message/`
- the backend expects a real PostgreSQL template database named by `TenantTemplateDatabaseName`
- the quick vesting endpoint no longer depends on a dead `inctrak` runtime database

## Recommended next hardening after first production cut

After the first real deploy is stable, the next nice cleanup would be:

1. add the same Pages Function gateway pattern to `frontend-signup/`
2. add the same Pages Function gateway pattern to `frontend-vesting/`
3. optionally move the marketing contact form off the hardcoded `shared.inctrak.com/api/...` URL and onto a dedicated Pages Function or a shared config value

That is not required for the first production deployment, but it would make the site family more uniform.
