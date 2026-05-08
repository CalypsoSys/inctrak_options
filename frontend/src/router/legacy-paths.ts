type LegacyPathResolver = (params: Record<string, string>) => string

const legacyRouteMap: Record<string, LegacyPathResolver> = {
  '/login': () => '/auth/login',
  '/activateaccount/:key': () => '/auth/login',
  '/resetpassword': () => '/auth/login',
  '/resetpasswordlink/:key': () => '/auth/login',
  '/accept_terms/:key': () => '/auth/login',
  '/privacy_policy': () => '/legal/privacy',
  '/contact': () => '/support/contact',
  '/company_stockclasses/:key': ({ key }) => `/admin/stock-classes/${normalizeLegacyKey(key)}`,
  '/company_stockholders/:key': ({ key }) => `/admin/stock-holders/${normalizeLegacyKey(key)}`,
  '/company_plans/:key': ({ key }) => `/admin/plans/${normalizeLegacyKey(key)}`,
  '/company_schedules/:key': ({ key }) => `/admin/vesting-schedules/${normalizeLegacyKey(key)}`,
  '/company_terminations/:key': ({ key }) => `/admin/termination-rules/${normalizeLegacyKey(key)}`,
  '/company_participants/:key': ({ key }) => `/admin/participants/${normalizeLegacyKey(key)}`,
  '/company_grants/:key': ({ key }) => `/admin/grants/${normalizeLegacyKey(key)}`,
  '/optionee_stocks/:key': () => '/participant/stocks',
  '/optionee_summary/:key': () => '/participant/options',
  '/optionee_grants/:key': ({ key }) => `/participant/grants/${normalizeLegacyKey(key)}`
}

function normalizeLegacyKey(key: string): string {
  return key === '-1' ? '' : key
}

function matchLegacyPath(path: string): { pattern: string; params: Record<string, string> } | null {
  const trimmedPath = path.replace(/\/+$/, '') || '/'

  for (const pattern of Object.keys(legacyRouteMap)) {
    const names: string[] = []
    const regex = new RegExp(
      '^' +
        pattern
          .replace(/\/+$/, '')
          .replace(/:[^/]+/g, (segment) => {
            names.push(segment.slice(1))
            return '([^/]+)'
          }) +
        '$'
    )

    const match = trimmedPath.match(regex)
    if (!match) {
      continue
    }

    const params = names.reduce<Record<string, string>>((accumulator, name, index) => {
      accumulator[name] = decodeURIComponent(match[index + 1])
      return accumulator
    }, {})

    return { pattern, params }
  }

  return null
}

// Preserve older hash-based bookmarks while routing retired auth links back to login.
export function resolveLegacyPath(path: string): string | null {
  const match = matchLegacyPath(path)
  if (!match) {
    return null
  }

  return legacyRouteMap[match.pattern](match.params)
}
