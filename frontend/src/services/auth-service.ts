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
