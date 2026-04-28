import { beforeEach, describe, expect, it, vi } from 'vitest'
import { acceptTerms, createLoginForm } from '@/services/auth-service'

const { apiPostMock } = vi.hoisted(() => ({
  apiPostMock: vi.fn()
}))

vi.mock('@/services/api', () => ({
  apiPost: apiPostMock
}))

describe('auth-service', () => {
  beforeEach(() => {
    apiPostMock.mockReset()
  })

  it('creates a login form without legacy server defaults', () => {
    expect(createLoginForm()).toEqual({
      email: '',
      password: '',
      confirmPassword: '',
      isRegistering: false,
      acceptTerms: false
    })
  })

  it('posts accept-terms requests to the legacy endpoint', async () => {
    apiPostMock.mockResolvedValue({ success: true })

    await acceptTerms('terms-key', true)

    expect(apiPostMock).toHaveBeenCalledWith('/api/login/accept_terms/', {
      AcceptTermsKey: 'terms-key',
      AcceptTerms: true
    })
  })
})
