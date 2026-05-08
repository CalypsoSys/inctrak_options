import test from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';

function read(path) {
  return readFileSync(new URL(path, import.meta.url), 'utf8');
}

test('tenant bootstrap sql stays clone-safe for PostgreSQL template databases', () => {
  const sql = read('../../inctrak.db/inctrak.sql');

  assert.doesNotMatch(sql, /\bCREATE DATABASE\b/i);
  assert.doesNotMatch(sql, /\\connect\b/i);
  assert.doesNotMatch(sql, /\bN'/);
  assert.match(sql, /CREATE EXTENSION IF NOT EXISTS "uuid-ossp";/);
  assert.match(sql, /CREATE TABLE SCHEMA_VERSION\(/);
  assert.match(sql, /INSERT INTO SCHEMA_VERSION \(SCHEMA_VERSION_PK, VERSION_NAME\)/);
  assert.match(sql, /VALUES \(1, 'inctrak-bootstrap-v1'\);/);
  assert.match(sql, /CREATE TABLE GROUPS\(/);
  assert.match(sql, /CREATE TABLE USERS\(/);
  assert.match(sql, /CREATE TABLE GRANTS\(/);
  assert.match(sql, /CREATE TABLE PERIODS\(/);
});
