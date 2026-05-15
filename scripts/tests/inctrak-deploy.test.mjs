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

  assert.match(compose, /- inctrak_postgres_data:\/var\/lib\/postgresql/);
  assert.doesNotMatch(compose, /\/var\/lib\/postgresql\/data/);
  assert.match(compose, /volumes:\s+inctrak_postgres_data:/s);
  assert.match(compose, /AppSettings__IncTrakDns: \$\{AppSettings__IncTrakDns\}/);
  assert.match(compose, /AppSettings__AllowedOrigins__0: \$\{AppSettings__AllowedOrigins__0\}/);
  assert.match(compose, /AppSettings__ControlPlaneConnection: \$\{AppSettings__ControlPlaneConnection\}/);
  assert.match(compose, /AppSettings__FeedbackConnection: \$\{AppSettings__FeedbackConnection\}/);
  assert.match(compose, /AppSettings__IncTrakConnection: \$\{AppSettings__IncTrakConnection\}/);
  assert.match(compose, /AppSettings__TenantTemplateDatabaseName: \$\{AppSettings__TenantTemplateDatabaseName\}/);
  assert.match(compose, /AppSettings__LocalAiModelPath: \$\{AppSettings__LocalAiModelPath\}/);
  assert.match(compose, /\$\{INCTRAK_LOCAL_AI_MODELS_HOST_PATH:-\/srv\/models\/inctrak\}:\/models:ro/);
  assert.match(compose, /\$\{INCTRAK_API_HOST_BIND:-127\.0\.0\.1\}:\$\{INCTRAK_API_HOST_PORT:-8082\}:8080/);
});

test('inctrak config example includes top-level docker host settings and postgres credentials', () => {
  const config = read('../inctrak/config.example.yaml');

  assert.match(config, /^INCTRAK_API_IMAGE: inctrak-api:latest/m);
  assert.match(config, /^INCTRAK_POSTGRES_IMAGE: postgres:18/m);
  assert.match(config, /^INCTRAK_API_HOST_PORT: 8082/m);
  assert.match(config, /^INCTRAK_POSTGRES_HOST_PORT: 5432/m);
  assert.match(config, /^INCTRAK_LOCAL_AI_MODELS_HOST_PATH: \/srv\/models\/inctrak/m);
  assert.match(config, /^  LocalAiModelPath: \/models\/qwen2\.5-1\.5b-instruct-q4_k_m\.gguf/m);
  assert.match(config, /^  LocalAiEndpoint: ""/m);
  assert.match(config, /^  LocalAiModel: ""/m);
  assert.match(config, /^  LocalAiApiKey: ""/m);
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

test('production runbook keeps config.yaml server-local', () => {
  const runbook = read('../../docs/inctrak_production_runbook.md');

  assert.match(runbook, /## Create the server config\.yaml/);
  assert.match(runbook, /cd \/srv\/stacks\/inctrak\/api\nvi config\.yaml\nchmod 600 config\.yaml/);
  assert.match(runbook, /scripts\/inctrak\/config\.example\.yaml/);
  assert.match(runbook, /Use\s+`\$\{VARIABLE_NAME\}` for secrets/);
  assert.match(runbook, /ControlPlaneConnection: Host=inctrak-postgres;Port=5432;Database=inctrak_control/);
  assert.match(runbook, /FeedbackConnection: Host=inctrak-postgres;Port=5432;Database=inctrak_feedback/);
  assert.match(runbook, /IncTrakConnection: Host=inctrak-postgres;Port=5432;Database=inctrak_template/);
  const renderCheck = runbook.match(/Validate the rendered compose config:\n\n```bash\n([\s\S]*?)\n```/);
  assert.ok(renderCheck);
  assert.match(renderCheck[1], /export INCTRAK_DB_PASSWORD=replace_me/);
  assert.match(renderCheck[1], /export INCTRAK_SUPABASE_URL=https:\/\/replace\.supabase\.co/);
  assert.match(renderCheck[1], /export INCTRAK_SUPABASE_PUBLISHABLE_KEY=replace_me/);
  assert.doesNotMatch(runbook, /\/tmp\/inctrak-config\.production\.yaml/);
  assert.doesNotMatch(runbook, /scp .*inctrak-config\.production\.yaml.*config\.yaml/);
});

test('production runbook smoke tests API with internal gateway key', () => {
  const runbook = read('../../docs/inctrak_production_runbook.md');

  assert.match(runbook, /gatewaySecret="replace_with_real_gateway_secret"/);
  assert.match(runbook, /curl -i -H "X-Internal-Api-Key: \$\{gatewaySecret\}" http:\/\/127\.0\.0\.1:8082\/api\/optionee\/quick\//);
  assert.match(runbook, /curl -i -H "Host: api\.inctrak\.com" -H "X-Internal-Api-Key: \$\{gatewaySecret\}" http:\/\/127\.0\.0\.1:80\/api\/optionee\/quick\//);
  assert.match(runbook, /compose-inctrak\.sh logs inctrak-api --tail=200/);
});

test('production runbook stages WSL repo files for PowerShell scp', () => {
  const runbook = read('../../docs/inctrak_production_runbook.md');

  assert.match(runbook, /## Stage and copy artifacts to the server/);
  assert.ok(runbook.includes('/mnt/c/transfer/inctrak-deploy/docker/inctrak'));
  assert.ok(runbook.includes('/mnt/c/transfer/inctrak-deploy/inctrak.db'));
  assert.ok(runbook.includes('cp docker/inctrak/docker-compose.yml /mnt/c/transfer/inctrak-deploy/docker/inctrak/docker-compose.yml'));
  assert.ok(runbook.includes('cp inctrak.db/control_plane.sql /mnt/c/transfer/inctrak-deploy/inctrak.db/control_plane.sql'));
  assert.ok(runbook.includes('cp inctrak.db/inctrak_feedback.sql /mnt/c/transfer/inctrak-deploy/inctrak.db/inctrak_feedback.sql'));
  assert.ok(runbook.includes('cp inctrak.db/inctrak.sql /mnt/c/transfer/inctrak-deploy/inctrak.db/inctrak.sql'));
  assert.ok(runbook.includes('/mnt/c/transfer/inctrak-models'));
  assert.ok(runbook.includes('cp /home/joe/models/qwen2.5-1.5b-instruct-q4_k_m.gguf /mnt/c/transfer/inctrak-models/qwen2.5-1.5b-instruct-q4_k_m.gguf'));
  assert.ok(runbook.includes(String.raw`$transfer = "C:\transfer\inctrak-deploy"`));
  assert.ok(runbook.includes(String.raw`$modelTransfer = "C:\transfer\inctrak-models"`));
  assert.ok(runbook.includes('scp "$transfer\\docker\\inctrak\\docker-compose.yml" ${server}:/srv/stacks/inctrak/api/docker-compose.yml'));
  assert.ok(runbook.includes('scp "$transfer\\inctrak.db\\control_plane.sql" ${server}:/srv/stacks/inctrak/api/inctrak.db/control_plane.sql'));
  assert.ok(runbook.includes('scp "$transfer\\inctrak.db\\inctrak_feedback.sql" ${server}:/srv/stacks/inctrak/api/inctrak.db/inctrak_feedback.sql'));
  assert.ok(runbook.includes('scp "$transfer\\inctrak.db\\inctrak.sql" ${server}:/srv/stacks/inctrak/api/inctrak.db/inctrak.sql'));
  assert.ok(runbook.includes('scp "$transfer\\scripts\\inctrak\\compose-inctrak.sh" ${server}:/srv/stacks/inctrak/api/scripts/compose-inctrak.sh'));
  assert.ok(runbook.includes('scp "$modelTransfer\\qwen2.5-1.5b-instruct-q4_k_m.gguf" ${server}:/srv/models/inctrak/'));
  assert.equal(runbook.includes('/path/to/repo/inctrak.db'), false);
  assert.equal(runbook.includes('scp -i'), false);
  assert.equal(runbook.includes('$pem'), false);
  assert.equal(runbook.includes(String.raw`scp .\docker\inctrak`), false);
  assert.equal(runbook.includes(String.raw`scp .\scripts\inctrak`), false);
});

test('cloudflared service checks avoid exposing tunnel token', () => {
  const runbook = read('../../docs/inctrak_production_runbook.md');
  const hostPrep = read('../../docs/inctrak_ubuntu_host_preparation.md');

  assert.match(hostPrep, /sudo cloudflared service install <paste-tunnel-token-from-cloudflare>/);
  assert.match(hostPrep, /systemctl is-active --quiet cloudflared/);
  assert.match(hostPrep, /Unit cloudflared\.service could not be found/);
  assert.match(runbook, /systemctl is-active --quiet cloudflared/);
  assert.match(runbook, /Avoid pasting\s+cloudflared status output/);
  assert.doesNotMatch(runbook, /systemctl status cloudflared --no-pager/);
  assert.doesNotMatch(hostPrep, /systemctl status cloudflared --no-pager/);
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
