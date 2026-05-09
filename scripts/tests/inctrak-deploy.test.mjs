import test from 'node:test';
import assert from 'node:assert/strict';
import { existsSync, readFileSync } from 'node:fs';

function read(path) {
  return readFileSync(new URL(path, import.meta.url), 'utf8');
}

function exists(path) {
  return existsSync(new URL(path, import.meta.url));
}

test('inctrak compose wrapper defaults to repo docker stack and local config file', () => {
  const script = read('../inctrak/compose-inctrak.sh');

  assert.match(script, /DEFAULT_STACK_DIR="\$REPO_ROOT\/docker\/inctrak"/);
  assert.match(script, /DEFAULT_CONFIG_FILE="\$SCRIPT_DIR\/config\.yaml"/);
  assert.match(script, /RENDERER_PATH="\$SCRIPT_DIR\/render-config-env"/);
  assert.match(script, /Render binary is not executable: \$RENDERER_PATH/);
  assert.match(script, /docker compose -f "\$COMPOSE_FILE" --env-file "\$TEMP_ENV_FILE" "\$@"/);
});

test('inctrak docker compose consumes flattened AppSettings environment variables', () => {
  const compose = read('../../docker/inctrak/docker-compose.yml');

  assert.match(compose, /- inctrak_postgres_data:\/var\/lib\/postgresql\/data/);
  assert.match(compose, /volumes:\s+inctrak_postgres_data:/s);
  assert.match(compose, /AppSettings__IncTrakDns: \$\{AppSettings__IncTrakDns\}/);
  assert.match(compose, /AppSettings__AllowedOrigins__0: \$\{AppSettings__AllowedOrigins__0\}/);
  assert.match(compose, /AppSettings__ControlPlaneConnection: \$\{AppSettings__ControlPlaneConnection\}/);
  assert.match(compose, /AppSettings__FeedbackConnection: \$\{AppSettings__FeedbackConnection\}/);
  assert.match(compose, /AppSettings__IncTrakConnection: \$\{AppSettings__IncTrakConnection\}/);
  assert.match(compose, /AppSettings__TenantTemplateDatabaseName: \$\{AppSettings__TenantTemplateDatabaseName\}/);
  assert.match(compose, /AppSettings__LocalAiModelPath: \$\{AppSettings__LocalAiModelPath\}/);
  assert.match(compose, /\$\{INCTRAK_API_HOST_BIND:-127\.0\.0\.1\}:\$\{INCTRAK_API_HOST_PORT:-8082\}:8080/);
});

test('inctrak config example includes top-level docker host settings and postgres credentials', () => {
  const config = read('../inctrak/config.example.yaml');

  assert.match(config, /^INCTRAK_API_IMAGE: inctrak-api:latest/m);
  assert.match(config, /^INCTRAK_POSTGRES_IMAGE: postgres:18/m);
  assert.match(config, /^INCTRAK_API_HOST_PORT: 8082/m);
  assert.match(config, /^INCTRAK_POSTGRES_HOST_PORT: 5432/m);
  assert.doesNotMatch(config, /^INCTRAK_POSTGRES_DATA_HOST_PATH:/m);
  assert.match(config, /^POSTGRES_PASSWORD: \$\{INCTRAK_DB_PASSWORD\}/m);
  assert.match(config, /^  ControlPlaneConnection: Host=localhost;Port=5432;Database=inctrak_control;Username=postgres;Password=\$\{INCTRAK_CONTROL_DB_PASSWORD\}/m);
});

test('production docs route Cloudflare Tunnel through host Caddy', () => {
  const runbook = read('../../docs/inctrak_production_runbook.md');
  const hostPrep = read('../../docs/inctrak_ubuntu_host_preparation.md');
  const caddy = read('../../docs/caddy_host_setup.md');
  const gateway = read('../../docs/cloudflare-pages-gateway.md');

  for (const doc of [runbook, hostPrep, caddy, gateway]) {
    assert.match(doc, /api\.inctrak\.com/);
    assert.doesNotMatch(doc, /api-origin\.inctrak\.com/);
    assert.match(doc, /127\.0\.0\.1:80/);
  }

  assert.match(runbook, /Cloudflare Tunnel -> host-installed Caddy/);
  assert.match(runbook, /Caddy -> `inctrak-api`/);
  assert.match(runbook, /reverse_proxy 127\.0\.0\.1:8082/);
  assert.match(hostPrep, /In the recommended steady state, Cloudflare Tunnel fronts Caddy/);
  assert.match(caddy, /sudo caddy validate --config \/etc\/caddy\/Caddyfile/);
  assert.match(gateway, /host-installed Caddy/);
  assert.doesNotMatch(gateway, /service: http:\/\/127\.0\.0\.1:8082/);
});

test('logrotate policies exist for caddy, api, and postgres logs', () => {
  assert.equal(exists('../caddy/caddy.logrotate'), true);
  assert.equal(exists('../inctrak/inctrak.logrotate'), true);

  const caddyLogrotate = read('../caddy/caddy.logrotate');
  const inctrakLogrotate = read('../inctrak/inctrak.logrotate');

  assert.match(caddyLogrotate, /\/srv\/logs\/caddy\/caddy\.log/);
  assert.match(caddyLogrotate, /systemctl reload caddy/);
  assert.match(inctrakLogrotate, /\/srv\/logs\/inctrak\/api\/access\.log/);
  assert.match(inctrakLogrotate, /\/srv\/logs\/inctrak\/api\/errors\.log/);
  assert.match(inctrakLogrotate, /\/srv\/logs\/inctrak\/postgres\/postgresql\.log/);
  assert.match(inctrakLogrotate, /copytruncate/);
});

test('all Vue apps have Pages API gateway functions', () => {
  const mainGateway = read('../../frontend/functions/api/[[path]].ts');
  const signupGateway = read('../../frontend-signup/functions/api/[[path]].ts');
  const vestingGateway = read('../../frontend-vesting/functions/api/[[path]].ts');

  for (const gateway of [mainGateway, signupGateway, vestingGateway]) {
    assert.match(gateway, /API_BASE_URL/);
    assert.match(gateway, /INTERNAL_API_KEY/);
    assert.match(gateway, /X-Internal-Api-Key/);
    assert.match(gateway, /X-Api-Gateway/);
  }
});
