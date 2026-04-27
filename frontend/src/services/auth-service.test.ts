import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createLoginForm, submitLogin } from '@/services/auth-service'

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

  it('posts login requests to the internal login endpoint', async () => {
    apiPostMock.mockResolvedValue({ success: true })

    await submitLogin({
      ...createLoginForm(),
      USER_NAME: 'founder',
      PASSWORD: 'secret'
    })

    expect(apiPostMock).toHaveBeenCalledWith('/api/login/login_internal/', expect.any(Object))
  })

  it('posts registration requests to the internal registration endpoint', async () => {
    apiPostMock.mockResolvedValue({ success: true })

    await submitLogin({
      ...createLoginForm(),
      USER_NAME: 'founder',
      PASSWORD: 'secret',
      PASSWORD2: 'secret',
      EMAIL_ADDRESS: 'founder@calypsosys.com',
      GROUP_NAME: 'Calypso Systems',
      IS_REGISTERING: true,
      ACCEPT_TERMS: true
    })

    expect(apiPostMock).toHaveBeenCalledWith('/api/login/register_internal/', expect.any(Object))
  })
})
