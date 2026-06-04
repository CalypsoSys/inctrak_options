const SUPABASE_URL = import.meta.env.VITE_SUPABASE_URL?.trim()
const SUPABASE_PUBLISHABLE_KEY = import.meta.env.VITE_SUPABASE_PUBLISHABLE_KEY?.trim()

export type SupabaseSession = {
  access_token?: string
  refresh_token?: string
  expires_in?: number
  expires_at?: number
  type?: string
  user?: {
    id?: string
    email?: string
    user_metadata?: {
      company_name?: string
      tenant_slug?: string
    }
  }
}

function getSupabaseHeaders(accessToken?: string): HeadersInit {
  if (!SUPABASE_URL || !SUPABASE_PUBLISHABLE_KEY) {
    throw new Error('Supabase frontend auth is not configured. Set VITE_SUPABASE_URL and VITE_SUPABASE_PUBLISHABLE_KEY.')
  }

  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    apikey: SUPABASE_PUBLISHABLE_KEY
  }

  if (accessToken) {
    headers.Authorization = `Bearer ${accessToken}`
  }

  return headers
}

function appendRedirectTo(path: string, redirectTo?: string): string {
  if (!redirectTo) {
    return path
  }

  const separator = path.includes('?') ? '&' : '?'
  return `${path}${separator}redirect_to=${encodeURIComponent(redirectTo)}`
}

async function supabaseRequest<T>(
  path: string,
  payload: unknown,
  method = 'POST',
  accessToken?: string
): Promise<T> {
  if (!SUPABASE_URL) {
    throw new Error('Supabase frontend auth is not configured. Set VITE_SUPABASE_URL.')
  }

  const response = await fetch(`${SUPABASE_URL}/auth/v1/${path}`, {
    method,
    headers: getSupabaseHeaders(accessToken),
    body: JSON.stringify(payload)
  })

  const responseText = await response.text()
  let body: unknown = {}
  if (responseText) {
    try {
      body = JSON.parse(responseText)
    } catch {
      body = { message: responseText }
    }
  }

  if (!response.ok) {
    const errorBody = body && typeof body === 'object'
      ? body as { msg?: string; message?: string }
      : {}
    throw new Error(errorBody.msg || errorBody.message || 'Supabase authentication failed.')
  }

  return body as T
}

export function signInWithPassword(email: string, password: string): Promise<SupabaseSession> {
  return supabaseRequest<SupabaseSession>('token?grant_type=password', { email, password })
}

export function signUpForAccount(email: string, password: string, redirectTo?: string): Promise<SupabaseSession> {
  return supabaseRequest<SupabaseSession>(appendRedirectTo('signup', redirectTo), { email, password })
}

export function signUpWithPassword(
  email: string,
  password: string,
  companyName: string,
  tenantSlug: string
): Promise<SupabaseSession> {
  return supabaseRequest<SupabaseSession>('signup', {
    email,
    password,
    data: {
      company_name: companyName,
      tenant_slug: tenantSlug
    }
  })
}

export async function sendMagicLink(email: string, redirectTo?: string): Promise<void> {
  await supabaseRequest<Record<string, never>>(
    appendRedirectTo('otp', redirectTo),
    { email, create_user: false }
  )
}

export async function sendPasswordRecoveryEmail(email: string, redirectTo?: string): Promise<void> {
  await supabaseRequest<Record<string, never>>(appendRedirectTo('recover', redirectTo), { email })
}

export async function updateSupabasePassword(accessToken: string, password: string): Promise<void> {
  await supabaseRequest<Record<string, never>>('user', { password }, 'PUT', accessToken)
}

export function refreshSupabaseSession(refreshToken: string): Promise<SupabaseSession> {
  return supabaseRequest<SupabaseSession>('token?grant_type=refresh_token', { refresh_token: refreshToken })
}
