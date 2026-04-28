import type { LoginForm } from '@/services/types'

export function createLoginForm(): LoginForm {
  return {
    email: '',
    password: '',
    confirmPassword: '',
    isRegistering: false,
    acceptTerms: false
  }
}
