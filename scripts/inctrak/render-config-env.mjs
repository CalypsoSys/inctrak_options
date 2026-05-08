#!/usr/bin/env node

import { readFile, writeFile } from 'node:fs/promises'

function parseArgs(argv) {
  const args = {
    format: 'env',
    output: null,
    configPath: null
  }

  for (let index = 0; index < argv.length; index += 1) {
    const arg = argv[index]

    if (arg === '--format') {
      args.format = argv[index + 1]
      index += 1
      continue
    }

    if (arg === '--output') {
      args.output = argv[index + 1]
      index += 1
      continue
    }

    if (!args.configPath) {
      args.configPath = arg
      continue
    }

    throw new Error(`Unexpected argument: ${arg}`)
  }

  if (!args.configPath) {
    throw new Error('Usage: render-config-env.mjs --format env|shell [--output path] /path/to/config.yaml')
  }

  if (args.format !== 'env' && args.format !== 'shell') {
    throw new Error(`Unsupported format: ${args.format}`)
  }

  return args
}

function countIndent(line) {
  let indent = 0
  while (indent < line.length && line[indent] === ' ') {
    indent += 1
  }

  return indent
}

function preprocessYaml(yamlText) {
  return yamlText
    .split(/\r?\n/)
    .map((line) => {
      const raw = line.replace(/\t/g, '    ')
      const trimmed = raw.trim()

      if (!trimmed || trimmed.startsWith('#')) {
        return null
      }

      return {
        indent: countIndent(raw),
        text: trimmed
      }
    })
    .filter(Boolean)
}

function parseScalar(value) {
  if (value === 'true' || value === 'false') {
    return value
  }

  return value
}

function parseArray(lines, state, indent) {
  const values = []

  while (state.index < lines.length) {
    const line = lines[state.index]
    if (line.indent < indent) {
      break
    }

    if (line.indent !== indent || !line.text.startsWith('- ')) {
      throw new Error(`Invalid sequence item near "${line.text}"`)
    }

    const itemValue = line.text.slice(2).trim()
    state.index += 1

    if (!itemValue) {
      throw new Error('Nested sequence items must define a scalar value.')
    }

    values.push(parseScalar(itemValue))
  }

  return values
}

function parseMap(lines, state, indent) {
  const output = {}

  while (state.index < lines.length) {
    const line = lines[state.index]
    if (line.indent < indent) {
      break
    }

    if (line.indent !== indent) {
      throw new Error(`Unexpected indentation near "${line.text}"`)
    }

    if (line.text.startsWith('- ')) {
      throw new Error(`Unexpected sequence item near "${line.text}"`)
    }

    const separatorIndex = line.text.indexOf(':')
    if (separatorIndex < 1) {
      throw new Error(`Invalid mapping entry: "${line.text}"`)
    }

    const key = line.text.slice(0, separatorIndex).trim()
    const rawValue = line.text.slice(separatorIndex + 1).trim()
    state.index += 1

    if (rawValue) {
      output[key] = parseScalar(rawValue)
      continue
    }

    const next = lines[state.index]
    if (!next || next.indent <= indent) {
      output[key] = ''
      continue
    }

    output[key] = next.text.startsWith('- ')
      ? parseArray(lines, state, next.indent)
      : parseMap(lines, state, next.indent)
  }

  return output
}

function parseYamlConfig(yamlText) {
  const lines = preprocessYaml(yamlText)
  if (!lines.length) {
    return {}
  }

  const state = { index: 0 }
  return parseMap(lines, state, lines[0].indent)
}

function resolvePlaceholders(value) {
  return value.replace(/\$\{([A-Za-z_][A-Za-z0-9_]*)\}/g, (match, envName) => {
    const envValue = process.env[envName]
    if (envValue == null) {
      throw new Error(`required environment variable is not set: ${envName}`)
    }

    return envValue
  })
}

function flattenConfig(prefix, value, output) {
  if (Array.isArray(value)) {
    value.forEach((item, index) => {
      flattenConfig(`${prefix}__${index}`, item, output)
    })
    return
  }

  if (value && typeof value === 'object') {
    for (const [childKey, childValue] of Object.entries(value)) {
      const nextPrefix = prefix ? `${prefix}__${childKey}` : childKey
      flattenConfig(nextPrefix, childValue, output)
    }
    return
  }

  if (!prefix) {
    throw new Error('Scalar values are not allowed at the config root')
  }

  output[prefix] = resolvePlaceholders(String(value ?? ''))
}

function shellQuote(value) {
  if (!value) {
    return "''"
  }

  return `'${value.replaceAll("'", `'\"'\"'`)}'`
}

export async function renderConfigEnv(yamlText, format = 'env') {
  const parsed = parseYamlConfig(yamlText)
  const flattened = {}
  flattenConfig('', parsed, flattened)

  return Object.keys(flattened)
    .sort()
    .map((key) => (format === 'shell'
      ? `export ${key}=${shellQuote(flattened[key])}`
      : `${key}=${flattened[key]}`))
}

async function main() {
  const args = parseArgs(process.argv.slice(2))
  const yamlText = await readFile(args.configPath, 'utf8')
  const rendered = await renderConfigEnv(yamlText, args.format)
  const body = rendered.join('\n')

  if (args.output) {
    await writeFile(args.output, body ? `${body}\n` : '')
    return
  }

  process.stdout.write(body)
  if (body) {
    process.stdout.write('\n')
  }
}

if (import.meta.url === `file://${process.argv[1]}`) {
  main().catch((error) => {
    console.error(error.message)
    process.exit(1)
  })
}
