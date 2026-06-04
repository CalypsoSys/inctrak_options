import { normalizeTenantSlug } from '@/services/tenant-slug'
import type { LoginForm } from '@/services/types'

export function createLoginForm(): LoginForm {
  return {
    email: '',
    password: '',
    confirmPassword: '',
    companyName: '',
    tenantSlug: '',
    isRegistering: false,
    acceptTerms: false
  }
}

export function normalizeAuthEmail(email: string): string {
  return email.trim()
}

export function validateSignInForm(email: string, password: string): string | null {
  if (!normalizeAuthEmail(email) || !password.trim()) {
    return 'Please enter an email address and password.'
  }

  return null
}

export function validateAccountCreationForm(email: string, password: string, confirmPassword: string): string | null {
  if (!normalizeAuthEmail(email) || !password.trim()) {
    return 'Please enter an email address and password.'
  }

  if (!confirmPassword.trim()) {
    return 'Please confirm your password.'
  }

  if (password !== confirmPassword) {
    return 'Passwords do not match.'
  }

  return null
}

export function validatePasswordResetForm(password: string, confirmPassword: string): string | null {
  if (!password.trim()) {
    return 'Please enter a new password.'
  }

  if (!confirmPassword.trim()) {
    return 'Please confirm your new password.'
  }

  if (password !== confirmPassword) {
    return 'Passwords do not match.'
  }

  return null
}

export function validateEmailAction(email: string): string | null {
  if (!normalizeAuthEmail(email)) {
    return 'Please enter an email address first.'
  }

  return null
}

export type PendingProvisioningMetadata = {
  companyName: string
  tenantSlug: string
}

export function resolvePendingProvisioningMetadata(metadata?: {
  company_name?: string
  tenant_slug?: string
}): PendingProvisioningMetadata | null {
  const companyName = metadata?.company_name?.trim() ?? ''
  const tenantSlug = normalizeTenantSlug(metadata?.tenant_slug ?? '')
  if (!companyName || !tenantSlug) {
    return null
  }

  return {
    companyName,
    tenantSlug
  }
}
