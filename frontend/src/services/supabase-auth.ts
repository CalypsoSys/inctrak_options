const SUPABASE_URL = import.meta.env.VITE_SUPABASE_URL?.trim()
const SUPABASE_PUBLISHABLE_KEY = import.meta.env.VITE_SUPABASE_PUBLISHABLE_KEY?.trim()

export type SupabaseSession = {
  access_token?: string
  refresh_token?: string
  expires_in?: number
  expires_at?: number
  user?: {
    email?: string
  }
}

function getSupabaseHeaders(): HeadersInit {
  if (!SUPABASE_URL || !SUPABASE_PUBLISHABLE_KEY) {
    throw new Error('Supabase frontend auth is not configured. Set VITE_SUPABASE_URL and VITE_SUPABASE_PUBLISHABLE_KEY.')
  }

  return {
    'Content-Type': 'application/json',
    apikey: SUPABASE_PUBLISHABLE_KEY
  }
}

async function supabaseRequest(path: string, payload: unknown): Promise<SupabaseSession> {
  if (!SUPABASE_URL) {
    throw new Error('Supabase frontend auth is not configured. Set VITE_SUPABASE_URL.')
  }

  const response = await fetch(`${SUPABASE_URL}/auth/v1/${path}`, {
    method: 'POST',
    headers: getSupabaseHeaders(),
    body: JSON.stringify(payload)
  })

  const body = await response.json()
  if (!response.ok) {
    throw new Error(body?.msg || body?.message || 'Supabase authentication failed.')
  }

  return body satisfies SupabaseSession
}

export function signInWithPassword(email: string, password: string): Promise<SupabaseSession> {
  return supabaseRequest('token?grant_type=password', { email, password })
}

export function signUpWithPassword(email: string, password: string): Promise<SupabaseSession> {
  return supabaseRequest('signup', { email, password })
}

export function refreshSupabaseSession(refreshToken: string): Promise<SupabaseSession> {
  return supabaseRequest('token?grant_type=refresh_token', { refresh_token: refreshToken })
}
