import { beforeEach, describe, expect, it, vi } from 'vitest'
import { acceptTerms, createLoginForm, fetchLoginDefaults } from '@/services/auth-service'

const { apiGetMock, apiPostMock } = vi.hoisted(() => ({
  apiGetMock: vi.fn(),
  apiPostMock: vi.fn()
}))

vi.mock('@/services/api', () => ({
  apiGet: apiGetMock,
  apiPost: apiPostMock
}))

describe('auth-service', () => {
  beforeEach(() => {
    apiGetMock.mockReset()
    apiPostMock.mockReset()
  })

  it('creates a login form without legacy Google state', () => {
    expect(createLoginForm()).toEqual({
      USER_NAME: '',
      PASSWORD: '',
      PASSWORD2: '',
      EMAIL_ADDRESS: '',
      GROUP_NAME: '',
      IS_REGISTERING: false,
      ACCEPT_TERMS: false
    })
  })

  it('fetches login defaults from the API', async () => {
    apiGetMock.mockResolvedValue({ USER_NAME: '' })

    await fetchLoginDefaults()

    expect(apiGetMock).toHaveBeenCalledWith('/api/login/get_creds/')
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
