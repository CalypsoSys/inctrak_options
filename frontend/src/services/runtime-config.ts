const DEFAULT_API_BASE_URL = 'https://localhost:5001'

export function getApiBaseUrl(): string {
  const runtimeValue = window.IncTrakSiteConfig?.apiBaseUrl?.trim()
  const envValue = import.meta.env.VITE_API_BASE_URL?.trim()
  const baseUrl = runtimeValue || envValue || DEFAULT_API_BASE_URL

  return baseUrl.replace(/\/+$/, '')
}

export function buildApiUrl(path: string): string {
  if (/^https?:\/\//i.test(path)) {
    return path
  }

  const normalizedPath = path.startsWith('/') ? path : `/${path}`
  return `${getApiBaseUrl()}${normalizedPath}`
}
