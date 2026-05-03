import { describe, expect, it } from 'vitest'
import { getPublicHeadline } from '@/services/public-headline'

describe('getPublicHeadline', () => {
  it('returns a tailored home headline for public users', () => {
    expect(getPublicHeadline('home', false, false)).toBe('Equity administration for growing companies')
  })

  it('returns a tailored login headline for public users', () => {
    expect(getPublicHeadline('login', false, false)).toBe('Equity management for growing companies')
  })

  it('keeps admin and optionee headlines intact', () => {
    expect(getPublicHeadline('login', true, false)).toBe('Modern equity operations for administrators')
    expect(getPublicHeadline('login', false, true)).toBe('Clear participant access to grants and vesting')
  })
})
