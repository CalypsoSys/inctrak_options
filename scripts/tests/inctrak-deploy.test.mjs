import test from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';

function read(path) {
  return readFileSync(new URL(path, import.meta.url), 'utf8');
}

test('inctrak compose wrapper defaults to repo docker stack and local config file', () => {
  const script = read('../inctrak/compose-inctrak.sh');

  assert.match(script, /DEFAULT_STACK_DIR="\$REPO_ROOT\/docker\/inctrak"/);
  assert.match(script, /DEFAULT_CONFIG_FILE="\$SCRIPT_DIR\/config\.yaml"/);
  assert.match(script, /RENDER_BIN="\$\{RENDER_BIN:-\$SCRIPT_DIR\/render-config-env\}"/);
  assert.match(script, /Render binary is not executable: \$RENDER_BIN/);
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
});

test('inctrak config example includes top-level docker host settings and postgres credentials', () => {
  const config = read('../inctrak/config.example.yaml');

  assert.match(config, /^INCTRAK_API_IMAGE: inctrak-api:latest/m);
  assert.match(config, /^INCTRAK_POSTGRES_IMAGE: postgres:18/m);
  assert.match(config, /^INCTRAK_API_HOST_PORT: 8080/m);
  assert.match(config, /^INCTRAK_POSTGRES_HOST_PORT: 5432/m);
  assert.doesNotMatch(config, /^INCTRAK_POSTGRES_DATA_HOST_PATH:/m);
  assert.match(config, /^POSTGRES_PASSWORD: \$\{INCTRAK_DB_PASSWORD\}/m);
  assert.match(config, /^  ControlPlaneConnection: Host=localhost;Port=5432;Database=inctrak_control;Username=postgres;Password=\$\{INCTRAK_CONTROL_DB_PASSWORD\}/m);
});
