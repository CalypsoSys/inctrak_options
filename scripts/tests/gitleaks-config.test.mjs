import test from 'node:test'
import assert from 'node:assert/strict'
import { readFileSync } from 'node:fs'

function read(path) {
  return readFileSync(new URL(path, import.meta.url), 'utf8')
}

test('gitleaks config extends the default rule set', () => {
  const config = read('../../.gitleaks.toml')

  assert.match(config, /title = "IncTrak Gitleaks configuration"/)
  assert.match(config, /\[extend\]/)
  assert.match(config, /useDefault = true/)
})

test('pre-commit config includes the gitleaks hook', () => {
  const config = read('../../.pre-commit-config.yaml')

  assert.match(config, /repo: https:\/\/github\.com\/gitleaks\/gitleaks/)
  assert.match(config, /rev: v8\.24\.2/)
  assert.match(config, /id: gitleaks/)
})

test('github actions workflow runs gitleaks with the repo config', () => {
  const workflow = read('../../.github/workflows/gitleaks.yml')

  assert.match(workflow, /name: gitleaks/)
  assert.match(workflow, /uses: actions\/checkout@v4/)
  assert.match(workflow, /uses: gitleaks\/gitleaks-action@v2/)
  assert.match(workflow, /GITLEAKS_CONFIG: \.gitleaks\.toml/)
})
