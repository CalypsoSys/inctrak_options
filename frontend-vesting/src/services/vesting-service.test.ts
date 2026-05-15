import { describe, expect, it } from 'vitest'
import { buildQuickGrantDefaults } from '@/services/quick-grant-defaults'
import type { QuickGrantResponse } from '@/services/vesting-service'
import type { Grant } from '@/services/types'

describe('QuickGrantResponse', () => {
  it('allows startup to handle an API response without defaults', () => {
    const response: QuickGrantResponse = {
      success: false,
      message: 'Unavailable'
    }

    expect(buildQuickGrantDefaults(response)).toEqual({
      grantPatch: {},
      periods: [],
      periodTypes: [],
      amountTypes: []
    })
  })

  it('normalizes defaults when the API returns a complete quick grant response', () => {
    const grant: Grant = {
      GRANT_PK: '',
      PARTICIPANT_FK: null,
      PLAN_FK: null,
      VESTING_SCHEDULE_FK: null,
      TERMINATION_FK: null,
      SHARES: 100000,
      OPTION_PRICE: 0,
      DATE_OF_GRANT: '',
      VESTING_START: '2024-01-01T00:00:00'
    }

    const defaults = buildQuickGrantDefaults({
      success: true,
      Grant: grant,
      Periods: [{
        PERIOD_AMOUNT: 1,
        PERIOD_TYPE_FK: 2,
        AMOUNT_TYPE_FK: 2,
        AMOUNT: 2.083333,
        INCREMENTS: 48,
        ORDER: 0,
        EVEN_OVER_N: 1
      }],
      PeriodTypes: [{ PERIOD_TYPE_PK: 2, NAME: 'Months' }],
      AmountTypes: [{ AMOUNT_TYPE_PK: 2, NAME: 'Percentage' }]
    })

    expect(defaults.grantPatch.VESTING_START).toBe('2024-01-01')
    expect(defaults.periods).toHaveLength(1)
    expect(defaults.periodTypes).toHaveLength(1)
    expect(defaults.amountTypes).toHaveLength(1)
  })
})
