import { apiPost } from '@/services/api'
import type { ApiResponse, LoginForm, LoginResponse } from '@/services/types'

export function createLoginForm(): LoginForm {
  return {
    email: '',
    password: '',
    confirmPassword: '',
    isRegistering: false,
    acceptTerms: false
  }
}

export function activateAccount(key: string): Promise<ApiResponse> {
  return apiPost<ApiResponse>('/api/login/activateaccount/', { ActivateKey: key })
}

export function requestPasswordReset(userNameEmail: string): Promise<ApiResponse> {
  return apiPost<ApiResponse>('/api/login/resetpassword/', { UserNameEmail: userNameEmail })
}

export function resetPassword(key: string, password1: string, password2: string, acceptTerms: boolean): Promise<ApiResponse> {
  return apiPost<ApiResponse>('/api/login/resetpasswordlink/', {
    ResetPasswordKey: key,
    Password1: password1,
    Password2: password2,
    AcceptTerms: acceptTerms
  })
}

export function acceptTerms(key: string, accepted: boolean): Promise<LoginResponse> {
  return apiPost<LoginResponse>('/api/login/accept_terms/', {
    AcceptTermsKey: key,
    AcceptTerms: accepted
  })
}
