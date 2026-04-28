import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import { fetchAppSession } from '@/services/auth-session'
import { refreshSupabaseSession } from '@/services/supabase-auth'

export type UserRole = 'admin' | 'optionee' | null

type SessionRecord = {
  accessToken: string
  refreshToken: string
  expiresAt: number
  role: UserRole
  email: string | null
}

const SESSION_KEY = 'inctrak.session'

export const useAuthStore = defineStore('auth', () => {
  const accessToken = ref<string | null>(null)
  const refreshToken = ref<string | null>(null)
  const expiresAt = ref<number | null>(null)
  const role = ref<UserRole>(null)
  const email = ref<string | null>(null)
  const isReady = ref(false)

  function loadSession(): void {
    const raw = window.localStorage.getItem(SESSION_KEY)
    if (!raw) {
      isReady.value = true
      return
    }

    try {
      const parsed = JSON.parse(raw) as SessionRecord
      accessToken.value = parsed.accessToken || null
      refreshToken.value = parsed.refreshToken || null
      expiresAt.value = parsed.expiresAt || null
      role.value = parsed.role || null
      email.value = parsed.email || null
    } catch {
      clearSession()
    }

    isReady.value = true
  }

  function persistSession(): void {
    if (!accessToken.value || !refreshToken.value || !expiresAt.value || !role.value) {
      window.localStorage.removeItem(SESSION_KEY)
      return
    }

    window.localStorage.setItem(
      SESSION_KEY,
      JSON.stringify({
        accessToken: accessToken.value,
        refreshToken: refreshToken.value,
        expiresAt: expiresAt.value,
        role: role.value,
        email: email.value
      } satisfies SessionRecord)
    )
  }

  async function initialize(): Promise<void> {
    if (!isReady.value) {
      loadSession()
    }

    const nowSeconds = Math.floor(Date.now() / 1000)
    if (accessToken.value && role.value && expiresAt.value && expiresAt.value > nowSeconds + 30) {
      return
    }

    if (!refreshToken.value) {
      return
    }

    if (!accessToken.value || !expiresAt.value || expiresAt.value <= nowSeconds + 30) {
      try {
        const session = await refreshSupabaseSession(refreshToken.value)
        accessToken.value = session.access_token
        refreshToken.value = session.refresh_token
        expiresAt.value = session.expires_at ?? nowSeconds + session.expires_in
        email.value = session.user?.email ?? email.value
      } catch {
        clearSession()
        return
      }
    }

    try {
      const appSession = await fetchAppSession()
      role.value = appSession.Role
      persistSession()
    } catch {
      clearSession()
    }
  }

  function setSession(nextAccessToken: string, nextRefreshToken: string, nextExpiresAt: number, nextRole: UserRole, nextEmail: string | null): void {
    accessToken.value = nextAccessToken
    refreshToken.value = nextRefreshToken
    expiresAt.value = nextExpiresAt
    role.value = nextRole
    email.value = nextEmail
    persistSession()
  }

  function clearSession(): void {
    accessToken.value = null
    refreshToken.value = null
    expiresAt.value = null
    role.value = null
    email.value = null
    window.localStorage.removeItem(SESSION_KEY)
  }

  loadSession()

  return {
    accessToken,
    refreshToken,
    expiresAt,
    role,
    email,
    isReady,
    isAuthenticated: computed(() => Boolean(accessToken.value && role.value)),
    isAdmin: computed(() => role.value === 'admin'),
    isOptionee: computed(() => role.value === 'optionee'),
    initialize,
    setSession,
    clearSession
  }
})
