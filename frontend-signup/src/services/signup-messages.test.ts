import { describe, expect, it } from 'vitest'
import { buildPendingSignupMessage } from '@/services/signup-messages'

describe('signup-messages', () => {
  it('uses a neutral pending-signup message for confirmation flows', () => {
    expect(buildPendingSignupMessage()).toContain('If this email is eligible for signup')
    expect(buildPendingSignupMessage()).toContain('check your email')
  })
})
