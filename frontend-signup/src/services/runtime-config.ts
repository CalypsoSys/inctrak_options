export function buildApiUrl(path: string): string {
  if (/^https?:\/\//i.test(path)) {
    return path
  }

  const normalizedPath = path.startsWith('/') ? path : `/${path}`
  const envValue = import.meta.env.VITE_API_BASE_URL?.trim()
  if (!envValue) {
    return normalizedPath
  }

  return `${envValue.replace(/\/+$/, '')}${normalizedPath}`
}

export function buildMainAppLoginUrl(): string {
  const explicit = import.meta.env.VITE_MAIN_APP_LOGIN_URL?.trim()
  if (explicit) {
    return explicit
  }

  const host = window.location.hostname.toLowerCase()
  if (host === '127.0.0.1' || host === 'localhost') {
    return 'http://127.0.0.1:5174/login'
  }

  return 'https://shared.inctrak.com/login'
}
