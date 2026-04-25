import { apiGet, apiPost } from '@/services/api'
import type { ApiResponse, LoginForm, LoginResponse } from '@/services/types'

export function createLoginForm(): LoginForm {
  return {
    USER_NAME: '',
    PASSWORD: '',
    PASSWORD2: '',
    EMAIL_ADDRESS: '',
    GROUP_NAME: '',
    IS_REGISTERING: false,
    ACCEPT_TERMS: false,
    GOOGLE_LOGON: false
  }
}

export function fetchLoginDefaults(): Promise<LoginForm> {
  return apiGet<LoginForm>('/api/login/get_creds/')
}

export function submitLogin(form: LoginForm): Promise<LoginResponse> {
  let endpoint = 'login_internal/'

  if (form.GOOGLE_LOGON) {
    endpoint = form.IS_REGISTERING ? 'register_google/' : 'login_google/'
  } else if (form.IS_REGISTERING) {
    endpoint = 'register_internal/'
  }

  return apiPost<LoginResponse>(`/api/login/${endpoint}`, form)
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
