import { describe, expect, it } from 'vitest'
import { createLoginForm } from '@/services/auth-service'

describe('auth-service', () => {
  it('creates a login form without legacy server defaults', () => {
    expect(createLoginForm()).toEqual({
      email: '',
      password: '',
      confirmPassword: '',
      companyName: '',
      tenantSlug: '',
      isRegistering: false,
      acceptTerms: false
    })
  })
})
