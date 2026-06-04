import { describe, expect, it } from 'vitest'
import {
  createLoginForm,
  normalizeAuthEmail,
  resolvePendingProvisioningMetadata,
  validateAccountCreationForm,
  validateEmailAction,
  validatePasswordResetForm,
  validateSignInForm
} from '@/services/auth-service'

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

  it('normalizes auth email entry without changing casing', () => {
    expect(normalizeAuthEmail('  User@Example.com  ')).toBe('User@Example.com')
  })

  it('validates sign-in credentials', () => {
    expect(validateSignInForm('', 'secret')).toBe('Please enter an email address and password.')
    expect(validateSignInForm('user@example.com', '')).toBe('Please enter an email address and password.')
    expect(validateSignInForm('user@example.com', 'secret')).toBeNull()
  })

  it('validates account creation credentials', () => {
    expect(validateAccountCreationForm('', 'secret', 'secret')).toBe('Please enter an email address and password.')
    expect(validateAccountCreationForm('user@example.com', 'secret', '')).toBe('Please confirm your password.')
    expect(validateAccountCreationForm('user@example.com', 'secret', 'different')).toBe('Passwords do not match.')
    expect(validateAccountCreationForm('user@example.com', 'secret', 'secret')).toBeNull()
  })

  it('validates email-only auth actions', () => {
    expect(validateEmailAction('')).toBe('Please enter an email address first.')
    expect(validateEmailAction('user@example.com')).toBeNull()
  })

  it('validates password reset credentials', () => {
    expect(validatePasswordResetForm('', 'secret')).toBe('Please enter a new password.')
    expect(validatePasswordResetForm('secret', '')).toBe('Please confirm your new password.')
    expect(validatePasswordResetForm('secret', 'different')).toBe('Passwords do not match.')
    expect(validatePasswordResetForm('secret', 'secret')).toBeNull()
  })

  it('resolves pending provisioning metadata from supabase user metadata', () => {
    expect(resolvePendingProvisioningMetadata({
      company_name: 'Calypso Systems',
      tenant_slug: 'calypso-systems'
    })).toEqual({
      companyName: 'Calypso Systems',
      tenantSlug: 'calypso-systems'
    })
  })

  it('returns null when signup metadata is incomplete', () => {
    expect(resolvePendingProvisioningMetadata({
      company_name: 'Calypso Systems'
    })).toBeNull()
  })
})
