import { describe, expect, it } from 'vitest'
import { getQuickGrantValidationMessage } from '@/services/quick-grant-validation'
import type { Grant, Period } from '@/services/types'

describe('quick grant validation', () => {
  it('accepts a generated cliff plus monthly vesting schedule', () => {
    expect(getQuickGrantValidationMessage(validGrant(), [
      validPeriod({ PERIOD_TYPE_FK: 1, AMOUNT: 25, INCREMENTS: 1 }),
      validPeriod({ PERIOD_TYPE_FK: 2, AMOUNT: 2.083333, INCREMENTS: 36 })
    ])).toBe('')
  })

  it('rejects an empty vesting start before posting to the api', () => {
    expect(getQuickGrantValidationMessage(validGrant({ VESTING_START: '' }), [
      validPeriod()
    ])).toBe('Enter a Vesting Start date before calculating vesting.')
  })

  it('treats a missing periods collection as no vesting periods', () => {
    expect(getQuickGrantValidationMessage(validGrant(), undefined)).toBe('Add at least one vesting period before calculating vesting.')
  })

  it('rejects missing period selections before posting to the api', () => {
    expect(getQuickGrantValidationMessage(validGrant(), [
      validPeriod({ PERIOD_TYPE_FK: null })
    ])).toBe('Period 1: select a Step Unit.')

    expect(getQuickGrantValidationMessage(validGrant(), [
      validPeriod({ AMOUNT_TYPE_FK: null })
    ])).toBe('Period 1: select what the period vests in.')
  })

  it('rejects blank numeric fields before posting to the api', () => {
    expect(getQuickGrantValidationMessage(validGrant(), [
      validPeriod({ AMOUNT: '' as unknown as number })
    ])).toBe('Period 1: enter an Amount Per Step greater than zero.')

    expect(getQuickGrantValidationMessage(validGrant(), [
      validPeriod({ INCREMENTS: '' as unknown as number })
    ])).toBe('Period 1: enter a Number of Steps greater than zero.')
  })

  it('allows fields hidden by quick period shortcuts to remain zero', () => {
    expect(getQuickGrantValidationMessage(validGrant(), [
      validPeriod({ EVEN_OVER_N: 1, AMOUNT: 0 })
    ])).toBe('')

    expect(getQuickGrantValidationMessage(validGrant(), [
      validPeriod({ EVEN_OVER_N: 2, INCREMENTS: 0 })
    ])).toBe('')
  })
})

function validGrant(overrides: Partial<Grant> = {}): Grant {
  return {
    GRANT_PK: '00000000-0000-0000-0000-000000000000',
    PARTICIPANT_FK: null,
    PLAN_FK: null,
    VESTING_SCHEDULE_FK: null,
    TERMINATION_FK: null,
    SHARES: 100000,
    OPTION_PRICE: 0,
    DATE_OF_GRANT: '0001-01-01T00:00:00',
    VESTING_START: '2026-05-15',
    ...overrides
  }
}

function validPeriod(overrides: Partial<Period> = {}): Period {
  return {
    PERIOD_PK: '00000000-0000-0000-0000-000000000000',
    PERIOD_AMOUNT: 1,
    PERIOD_TYPE_FK: 2,
    AMOUNT_TYPE_FK: 2,
    AMOUNT: 25,
    INCREMENTS: 4,
    ORDER: 0,
    EVEN_OVER_N: 0,
    ...overrides
  }
}
