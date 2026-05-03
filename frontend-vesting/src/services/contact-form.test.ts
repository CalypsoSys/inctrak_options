import { describe, expect, it } from 'vitest'
import { canSubmitContactForm } from '@/services/contact-form'

describe('canSubmitContactForm', () => {
  it('returns true when all required fields are present', () => {
    expect(canSubmitContactForm({
      EmailAddress: 'contact@inctrak.com',
      Name: 'Founder',
      MessageTypeFk: 8,
      Subject: 'Question about vesting',
      Message: 'Can you help me review this schedule?'
    })).toBe(true)
  })

  it('returns false when a required field is missing', () => {
    expect(canSubmitContactForm({
      EmailAddress: 'contact@inctrak.com',
      Name: '',
      MessageTypeFk: 8,
      Subject: 'Question about vesting',
      Message: 'Can you help me review this schedule?'
    })).toBe(false)
  })
})
