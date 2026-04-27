import test from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';

function read(path) {
  return readFileSync(new URL(path, import.meta.url), 'utf8');
}

test('control-plane bootstrap stays portable and contains core tables', () => {
  const sql = read('../../inctrak.db/control_plane.sql');

  assert.doesNotMatch(sql, /\bCREATE DATABASE\b/i);
  assert.doesNotMatch(sql, /\\connect\b/i);
  assert.match(sql, /CREATE TABLE cp_users\(/);
  assert.match(sql, /CREATE TABLE cp_tenants\(/);
  assert.match(sql, /CREATE TABLE cp_tenant_domains\(/);
  assert.match(sql, /CREATE TABLE cp_memberships\(/);
  assert.match(sql, /CREATE TABLE cp_provisioning_jobs\(/);
  assert.match(sql, /CREATE TABLE cp_reserved_slugs\(/);
  assert.match(sql, /INSERT INTO cp_reserved_slugs .*'signup'/);
  assert.match(sql, /INSERT INTO cp_reserved_slugs .*'vesting'/);
});
