import { describe, expect, it } from 'vitest'
import { buildPromptGrantPatch, getPromptAmountTypes, getPromptPeriods, getPromptPeriodTypes } from '@/services/prompt-interpret'

describe('buildPromptGrantPatch', () => {
  it('applies shares and vesting start when present', () => {
    const patch = buildPromptGrantPatch({
      success: true,
      sharesGranted: 4800,
      vestingStart: '2026-01-01',
      Periods: [],
      PeriodTypes: [],
      AmountTypes: []
    })

    expect(patch.SHARES).toBe(4800)
    expect(patch.VESTING_START).toBe('2026-01-01')
  })

  it('leaves manual fields alone when prompt values are missing', () => {
    const patch = buildPromptGrantPatch({
      success: true,
      Periods: [],
      PeriodTypes: [],
      AmountTypes: []
    })

    expect(patch).toEqual({})
  })

  it('reads prompt collections from PascalCase or camelCase API responses', () => {
    const period = {
      PERIOD_AMOUNT: 1,
      PERIOD_TYPE_FK: 2,
      AMOUNT_TYPE_FK: 2,
      AMOUNT: 2.083333,
      INCREMENTS: 36,
      ORDER: 1,
      EVEN_OVER_N: 0
    }
    const periodType = { PERIOD_TYPE_PK: 2, NAME: 'Months' }
    const amountType = { AMOUNT_TYPE_PK: 2, NAME: 'Percentage' }

    expect(getPromptPeriods({ success: true, Periods: [period] })).toEqual([period])
    expect(getPromptPeriods({ success: true, periods: [period] })).toEqual([period])
    expect(getPromptPeriodTypes({ success: true, periodTypes: [periodType] })).toEqual([periodType])
    expect(getPromptAmountTypes({ success: true, amountTypes: [amountType] })).toEqual([amountType])
  })
})
