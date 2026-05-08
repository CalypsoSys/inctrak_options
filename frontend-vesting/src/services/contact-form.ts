import type { FeedbackForm } from '@/services/types'

export function canSubmitContactForm(form: FeedbackForm): boolean {
  return hasValue(form.EmailAddress) &&
    hasValue(form.Name) &&
    hasValue(form.Subject) &&
    hasValue(form.Message)
}

function hasValue(value: string | null | undefined): boolean {
  return typeof value === 'string' && value.trim().length > 0
}
