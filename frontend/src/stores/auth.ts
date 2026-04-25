import { computed, ref } from 'vue'
import { defineStore } from 'pinia'

export type UserRole = 'admin' | 'optionee' | null

type SessionRecord = {
  uuid: string
  role: UserRole
}

const SESSION_KEY = 'inctrak.session'

export const useAuthStore = defineStore('auth', () => {
  const uuid = ref<string | null>(null)
  const role = ref<UserRole>(null)

  function loadSession(): void {
    const raw = window.localStorage.getItem(SESSION_KEY)
    if (!raw) {
      return
    }

    try {
      const parsed = JSON.parse(raw) as SessionRecord
      uuid.value = parsed.uuid || null
      role.value = parsed.role || null
    } catch {
      clearSession()
    }
  }

  function persistSession(): void {
    if (!uuid.value || !role.value) {
      window.localStorage.removeItem(SESSION_KEY)
      return
    }

    window.localStorage.setItem(
      SESSION_KEY,
      JSON.stringify({
        uuid: uuid.value,
        role: role.value
      } satisfies SessionRecord)
    )
  }

  function setSession(nextUuid: string, nextRole: UserRole): void {
    uuid.value = nextUuid
    role.value = nextRole
    persistSession()
  }

  function clearSession(): void {
    uuid.value = null
    role.value = null
    window.localStorage.removeItem(SESSION_KEY)
  }

  loadSession()

  return {
    uuid,
    role,
    isAuthenticated: computed(() => Boolean(uuid.value)),
    isAdmin: computed(() => role.value === 'admin'),
    isOptionee: computed(() => role.value === 'optionee'),
    setSession,
    clearSession
  }
})
