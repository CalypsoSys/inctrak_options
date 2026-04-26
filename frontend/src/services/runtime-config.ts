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
