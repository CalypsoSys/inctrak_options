import { afterEach, describe, expect, it, vi } from 'vitest'

function mockSupabaseFetch(body: unknown = {}) {
  const fetchMock = vi.fn().mockResolvedValue(new Response(JSON.stringify(body), {
    status: 200,
    headers: {
      'Content-Type': 'application/json'
    }
  }))

  vi.stubGlobal('fetch', fetchMock)
  return fetchMock
}

async function importAuthService() {
  vi.resetModules()
  vi.stubEnv('VITE_SUPABASE_URL', 'https://project.supabase.co')
  vi.stubEnv('VITE_SUPABASE_PUBLISHABLE_KEY', 'sb_publishable_test')
  return import('@/services/supabase-auth')
}

function getRequestBody(fetchMock: ReturnType<typeof mockSupabaseFetch>): unknown {
  const init = fetchMock.mock.calls[0][1] as RequestInit
  return JSON.parse(init.body as string)
}

describe('supabase-auth', () => {
  afterEach(() => {
    vi.restoreAllMocks()
    vi.unstubAllGlobals()
    vi.unstubAllEnvs()
    vi.resetModules()
  })

  it('posts credential-only account signup without tenant metadata', async () => {
    const fetchMock = mockSupabaseFetch({ user: { email: 'user@example.com' } })
    const { signUpForAccount } = await importAuthService()

    await signUpForAccount('user@example.com', 'secret', 'https://app.inctrak.com/#/auth/login')

    expect(fetchMock).toHaveBeenCalledWith(
      'https://project.supabase.co/auth/v1/signup?redirect_to=https%3A%2F%2Fapp.inctrak.com%2F%23%2Fauth%2Flogin',
      expect.objectContaining({
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          apikey: 'sb_publishable_test'
        }
      })
    )
    expect(getRequestBody(fetchMock)).toEqual({
      email: 'user@example.com',
      password: 'secret'
    })
  })

  it('requests magic links without creating unknown Supabase users', async () => {
    const fetchMock = mockSupabaseFetch()
    const { sendMagicLink } = await importAuthService()

    await sendMagicLink('user@example.com', 'https://app.inctrak.com/#/auth/login')

    expect(fetchMock).toHaveBeenCalledWith(
      'https://project.supabase.co/auth/v1/otp?redirect_to=https%3A%2F%2Fapp.inctrak.com%2F%23%2Fauth%2Flogin',
      expect.objectContaining({
        method: 'POST'
      })
    )
    expect(getRequestBody(fetchMock)).toEqual({
      email: 'user@example.com',
      create_user: false
    })
  })

  it('requests password recovery email delivery', async () => {
    const fetchMock = mockSupabaseFetch()
    const { sendPasswordRecoveryEmail } = await importAuthService()

    await sendPasswordRecoveryEmail('user@example.com', 'https://app.inctrak.com/#/auth/login')

    expect(fetchMock).toHaveBeenCalledWith(
      'https://project.supabase.co/auth/v1/recover?redirect_to=https%3A%2F%2Fapp.inctrak.com%2F%23%2Fauth%2Flogin',
      expect.objectContaining({
        method: 'POST'
      })
    )
    expect(getRequestBody(fetchMock)).toEqual({
      email: 'user@example.com'
    })
  })

  it('updates a password with the recovery access token', async () => {
    const fetchMock = mockSupabaseFetch()
    const { updateSupabasePassword } = await importAuthService()

    await updateSupabasePassword('access-token', 'new-secret')

    expect(fetchMock).toHaveBeenCalledWith(
      'https://project.supabase.co/auth/v1/user',
      expect.objectContaining({
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          apikey: 'sb_publishable_test',
          Authorization: 'Bearer access-token'
        }
      })
    )
    expect(getRequestBody(fetchMock)).toEqual({
      password: 'new-secret'
    })
  })
})
