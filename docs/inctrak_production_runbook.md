# IncTrak production runbook

This runbook is the single operational document for deploying, validating, rolling back, and maintaining the IncTrak
API and PostgreSQL stack in the lab.

Related docs:

- [cloudflare-pages-gateway.md](cloudflare-pages-gateway.md)
- [inctrak_ubuntu_host_preparation.md](inctrak_ubuntu_host_preparation.md)
- [caddy_host_setup.md](caddy_host_setup.md)
- [inctrak_local_vscode.md](inctrak_local_vscode.md)

## Steady-state topology

Public sites on Cloudflare:

- `inctrak.com`
- `shared.inctrak.com`
- `signup.inctrak.com`
- `vesting.inctrak.com`
- `docs.inctrak.com`
- `blog.inctrak.com`

Private lab origin behind Cloudflare Tunnel:

- `api.inctrak.com`

Recommended request path:

- browser -> Cloudflare Pages
- `/api/*` -> Cloudflare Pages Functions
- Pages Functions -> Cloudflare Tunnel hostname for the API origin
- Cloudflare Tunnel -> host-installed Caddy on the Ubuntu host
- Caddy -> `inctrak-api`

At minimum, host Caddy should include:

```caddy
{
    auto_https off
}

http://api.inctrak.com {
    reverse_proxy 127.0.0.1:8082
}
```

## Server layout

Expected structure:

```text
/srv/stacks/inctrak
/srv/stacks/inctrak/api
  docker-compose.yml
  config.yaml
  inctrak-api-latest.tar
  scripts/
    compose-inctrak.sh
    render-config-env
    inctrak.logrotate

/srv/backups/inctrak/incoming
/srv/backups/postgres
/srv/logs/inctrak/api
/srv/logs/inctrak/postgres
/srv/logs/caddy
```

Create the required directories if they do not already exist:

```bash
sudo mkdir -p /srv/stacks/inctrak/api/scripts
sudo mkdir -p /srv/backups/inctrak/incoming
sudo mkdir -p /srv/backups/postgres
sudo mkdir -p /srv/logs/inctrak/api
sudo mkdir -p /srv/logs/inctrak/postgres
sudo mkdir -p /srv/logs/caddy

sudo chown -R $USER:$USER /srv/stacks/inctrak
sudo chown -R $USER:$USER /srv/backups/inctrak
sudo chown -R $USER:$USER /srv/logs/inctrak
sudo chown -R caddy:caddy /srv/logs/caddy
sudo chown 999:999 /srv/logs/inctrak/postgres
```

## Files from this repo

Copy or derive these from the repo:

- `shared.inctrak.com/Dockerfile`
- `docker/inctrak/docker-compose.yml`
- `scripts/inctrak/compose-inctrak.sh`
- `scripts/inctrak/config.example.yaml`
- `scripts/inctrak/inctrak.logrotate`
- `scripts/caddy/caddy.logrotate`
- `inctrak.db/control_plane.sql`
- `inctrak.db/inctrak.sql`
- `inctrak.db/inctrak_feedback.sql`
- the built API image tarball you create locally

Server-local files that must not come from git:

- `/srv/stacks/inctrak/api/config.yaml`
- Cloudflare Tunnel credentials
- real secrets and passwords

## Required secret inputs

Keep real values in the host shell environment, a password manager, or the server-local `config.yaml`. Do not commit
them.

| Name | Purpose |
| --- | --- |
| `INCTRAK_DB_PASSWORD` | PostgreSQL password used by production connection strings |
| `INCTRAK_CONTROL_DB_PASSWORD` | Optional separate local/control-plane password for development-style configs |
| `INCTRAK_GATEWAY_SECRET` | Internal API key injected by Cloudflare Pages Functions |
| `INCTRAK_SUPABASE_URL` | Supabase project URL |
| `INCTRAK_SUPABASE_PUBLISHABLE_KEY` | Supabase browser-safe publishable key |
| `INCTRAK_SUPABASE_JWT_SECRET` | Optional legacy HS256 fallback secret |
| `INCTRAK_SLACK_FEEDBACK_WEBHOOK_URL` | Feedback and public-usage Slack webhook |
| `INCTRAK_LOCAL_AI_MODEL_PATH` | Optional local GGUF model path |
| `INCTRAK_LOCAL_AI_ENDPOINT` | Optional OpenAI-compatible local AI endpoint |
| `INCTRAK_LOCAL_AI_MODEL` | Optional local AI model name |
| `INCTRAK_LOCAL_AI_API_KEY` | Optional local AI endpoint key |

## Build the API image locally

From the repo root in WSL:

```bash
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

## Build the shared YAML-to-env renderer

Build the shared renderer from its repo in WSL/Linux so the server receives a Linux binary:

```bash
cd ~/work/calypsosys-workbench/repos/babalu-yaml-env
mkdir -p /mnt/c/transfer
if [ -f /mnt/c/transfer/render-config-env ]; then mv /mnt/c/transfer/render-config-env /mnt/c/transfer/render-config-env.lastgood; fi
go build -o /mnt/c/transfer/render-config-env ./cmd/babalu_yaml_env
```

That gives you:

```text
C:\transfer\render-config-env
```

## Create the server config.yaml

Before any stack command, create the server-local `config.yaml` using
`scripts/inctrak/config.example.yaml` as the reference, then fill in the real production values. Use
`${VARIABLE_NAME}` for secrets so the YAML remains the single source of truth while secrets still come from the
host environment at runtime.

Then run on the Ubuntu host:

```bash
cd /srv/stacks/inctrak/api
vi config.yaml
chmod 600 config.yaml
```

Minimum structure:

```yaml
INCTRAK_API_IMAGE: inctrak-api:latest
INCTRAK_POSTGRES_IMAGE: postgres:18

ASPNETCORE_ENVIRONMENT: Production
INCTRAK_API_HOST_BIND: 127.0.0.1
INCTRAK_API_HOST_PORT: 8082
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

- PostgreSQL data persistence lives in the named Docker volume `inctrak_postgres_data`
- `IncTrakConnection` should point at the real template database in production
- if local AI is not enabled on the server, leave those values blank

## Copy artifacts to the server

From Windows PowerShell, for example:

```powershell
$server = "joe@192.168.50.95"

scp C:\transfer\inctrak-api-latest.tar.gz ${server}:/srv/stacks/inctrak/api/
scp C:\transfer\render-config-env ${server}:/srv/stacks/inctrak/api/scripts/render-config-env
scp .\docker\inctrak\docker-compose.yml ${server}:/srv/stacks/inctrak/api/docker-compose.yml
scp .\scripts\inctrak\compose-inctrak.sh ${server}:/srv/stacks/inctrak/api/scripts/compose-inctrak.sh
scp .\scripts\inctrak\inctrak.logrotate ${server}:/srv/stacks/inctrak/api/scripts/inctrak.logrotate
scp .\scripts\caddy\caddy.logrotate ${server}:/srv/stacks/inctrak/api/scripts/caddy.logrotate
```

After copying artifacts and editing `config.yaml`, on the Ubuntu host:

```bash
chmod +x /srv/stacks/inctrak/api/scripts/compose-inctrak.sh
chmod +x /srv/stacks/inctrak/api/scripts/render-config-env
chmod 600 /srv/stacks/inctrak/api/config.yaml
```

Install logrotate policies:

```bash
sudo cp /srv/stacks/inctrak/api/scripts/inctrak.logrotate /etc/logrotate.d/inctrak
sudo cp /srv/stacks/inctrak/api/scripts/caddy.logrotate /etc/logrotate.d/inctrak-caddy
sudo chmod 644 /etc/logrotate.d/inctrak /etc/logrotate.d/inctrak-caddy
sudo logrotate -d /etc/logrotate.d/inctrak
sudo logrotate -d /etc/logrotate.d/inctrak-caddy
```

## Preflight checks on the server

Run on the Ubuntu host:

```bash
cd /srv/stacks/inctrak/api
docker version
docker compose version
test -f config.yaml && echo "config.yaml present"
test -f docker-compose.yml && echo "compose file present"
test -x scripts/compose-inctrak.sh && echo "compose wrapper present"
test -x scripts/render-config-env && echo "render binary present"
sudo caddy validate --config /etc/caddy/Caddyfile
systemctl status caddy --no-pager
systemctl status cloudflared --no-pager
```

Validate the rendered compose config:

```bash
export INCTRAK_DB_PASSWORD=replace_me
export INCTRAK_GATEWAY_SECRET=replace_me
export INCTRAK_SUPABASE_URL=https://replace.supabase.co
export INCTRAK_SUPABASE_PUBLISHABLE_KEY=replace_me
export INCTRAK_SUPABASE_JWT_SECRET=replace_me
export INCTRAK_SLACK_FEEDBACK_WEBHOOK_URL=https://hooks.slack.com/services/replace/me
./scripts/compose-inctrak.sh config >/tmp/inctrak-compose.out
tail -n 30 /tmp/inctrak-compose.out
```

Use real values in the active shell session before running the wrapper for deployment.

## Load the API image

On the Ubuntu host:

```bash
cd /srv/stacks/inctrak/api
gunzip -f inctrak-api-latest.tar.gz
docker load -i inctrak-api-latest.tar
```

## Bring up PostgreSQL

Start the stack so PostgreSQL can become healthy:

```bash
cd /srv/stacks/inctrak/api
./scripts/compose-inctrak.sh up -d inctrak-postgres
./scripts/compose-inctrak.sh ps
./scripts/compose-inctrak.sh logs inctrak-postgres --tail=100
```

## Bootstrap PostgreSQL the first time

Copy the SQL files to a server-local staging location or pipe them over SSH from your workstation.

Control-plane database:

```bash
docker exec -i inctrak-postgres psql -U postgres -c "CREATE DATABASE inctrak_control;"
docker exec -i inctrak-postgres psql -U postgres -d inctrak_control < /path/to/repo/inctrak.db/control_plane.sql
```

Feedback database:

```bash
docker exec -i inctrak-postgres psql -U postgres -d postgres < /path/to/repo/inctrak.db/inctrak_feedback.sql
```

Template database:

```bash
docker exec -i inctrak-postgres psql -U postgres -c "CREATE DATABASE inctrak_template;"
docker exec -i inctrak-postgres psql -U postgres -d inctrak_template < /path/to/repo/inctrak.db/inctrak.sql
docker exec -i inctrak-postgres psql -U postgres -d postgres -c "UPDATE pg_database SET datistemplate = true WHERE datname = 'inctrak_template';"
```

`TenantSignupProvisioner` expects a real PostgreSQL template database.

## Verify PostgreSQL log-directory ownership

The official PostgreSQL container commonly writes logs as container user/group `999:999`.

```bash
stat -c '%u:%g %n' /srv/logs/inctrak/postgres
docker exec inctrak-postgres id
docker exec inctrak-postgres stat -c '%u:%g %n' /var/log/postgresql
```

If needed:

```bash
sudo chown 999:999 /srv/logs/inctrak/postgres
```

## Bring up the full stack

On the Ubuntu host:

```bash
cd /srv/stacks/inctrak/api
./scripts/compose-inctrak.sh up -d
./scripts/compose-inctrak.sh ps
./scripts/compose-inctrak.sh logs inctrak-api --tail=100
```

Check the API directly on the host:

```bash
curl -i http://127.0.0.1:8082/api/optionee/quick/
```

If gateway-secret enforcement is enabled, direct requests without `X-Internal-Api-Key` should return `401`.

Check the Caddy path:

```bash
curl -i -H "Host: api.inctrak.com" http://127.0.0.1:80/api/optionee/quick/
```

## Cloudflare Tunnel

Recommended ingress shape:

```yaml
tunnel: <your-tunnel-id>
credentials-file: /etc/cloudflared/<your-tunnel-id>.json

ingress:
  - hostname: api.inctrak.com
    service: http://127.0.0.1:80
  - service: http_status:404
```

Run or restart the tunnel:

```bash
sudo systemctl restart cloudflared
sudo systemctl status cloudflared --no-pager
```

## Cloudflare Pages projects

### `shared.inctrak.com`

- Root directory: `frontend`
- Build command: `npm ci && npm run build`
- Output: `dist`
- Custom domain: `shared.inctrak.com`
- Variables:
  - `API_BASE_URL=https://api.inctrak.com/api`
  - `INTERNAL_API_KEY=<same as AppSettings__GatewaySecret>`
  - `VITE_SIGNUP_APP_URL=https://signup.inctrak.com`
  - `VITE_VESTING_APP_URL=https://vesting.inctrak.com`

### `signup.inctrak.com`

- Root directory: `frontend-signup`
- Build command: `npm ci && npm run build`
- Output: `dist`
- Custom domain: `signup.inctrak.com`
- Variables:
  - `API_BASE_URL=https://api.inctrak.com/api`
  - `INTERNAL_API_KEY=<same as AppSettings__GatewaySecret>`
  - `VITE_MAIN_APP_LOGIN_URL=https://shared.inctrak.com/login`

### `vesting.inctrak.com`

- Root directory: `frontend-vesting`
- Build command: `npm ci && npm run build`
- Output: `dist`
- Custom domain: `vesting.inctrak.com`
- Variables:
  - `API_BASE_URL=https://api.inctrak.com/api`
  - `INTERNAL_API_KEY=<same as AppSettings__GatewaySecret>`

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

## Smoke-test checklist

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
3. Submit the signup form through `https://signup.inctrak.com/api`.
4. Visit `https://shared.inctrak.com`.
5. Confirm the login or temporary lockout state shows as expected.

## Safe redeploy flow

For normal API-only redeploys:

1. Build new image locally in WSL.
2. Stage new `inctrak-api-latest.tar.gz` to `C:\transfer`.
3. Copy to `/srv/stacks/inctrak/api/`.
4. Keep the prior image tarball as `inctrak-api-latest.lastgood.tar.gz`.
5. Load the image with `docker load -i inctrak-api-latest.tar`.
6. Run `./scripts/compose-inctrak.sh up -d`.
7. Check API, Caddy, and app logs.
8. Re-test `shared.inctrak.com`, `signup.inctrak.com`, `vesting.inctrak.com`, and the `inctrak.com` contact form.

Before tenant-affecting schema or provisioning changes, take a PostgreSQL backup and confirm the restore target:

```bash
mkdir -p /srv/backups/postgres
docker exec inctrak-postgres pg_dumpall -U postgres | gzip > /srv/backups/postgres/inctrak-predeploy-$(date +%Y%m%d-%H%M%S).sql.gz
```

## Rollback flow

If the new API image fails after deployment:

```bash
cd /srv/stacks/inctrak/api
docker load -i inctrak-api-latest.lastgood.tar
./scripts/compose-inctrak.sh up -d
./scripts/compose-inctrak.sh logs inctrak-api --tail=100
```

If the `.lastgood` image is still compressed:

```bash
gunzip -k inctrak-api-latest.lastgood.tar.gz
docker load -i inctrak-api-latest.lastgood.tar
./scripts/compose-inctrak.sh up -d
```

Database rollback should be handled deliberately from the backup made before the deploy. Do not restore over a live
database until you have confirmed the target environment and impact.

## Operational logs

Useful commands:

```bash
cd /srv/stacks/inctrak/api
./scripts/compose-inctrak.sh logs inctrak-api --tail=200
./scripts/compose-inctrak.sh logs inctrak-postgres --tail=200
sudo tail -n 100 /srv/logs/caddy/caddy.log
sudo journalctl -u caddy -n 100 --no-pager
sudo journalctl -u cloudflared -n 100 --no-pager
```

## Notes about current repo behavior

- all three Vue apps can use Cloudflare Pages Functions as their `/api/*` gateway
- `inctrak.com` currently posts its contact form directly to `https://shared.inctrak.com/api/feedback/save_message/`
- the backend expects a real PostgreSQL template database named by `TenantTemplateDatabaseName`
- the quick vesting endpoint no longer depends on a dead `inctrak` runtime database

## Later hardening

After the first real deploy is stable, consider moving the marketing contact form off the hardcoded
`shared.inctrak.com/api/...` URL and onto a dedicated Pages Function or a shared config value.
