import { describe, expect, it } from 'vitest'
import { buildPromptGrantPatch } from '@/services/prompt-interpret'

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
})
