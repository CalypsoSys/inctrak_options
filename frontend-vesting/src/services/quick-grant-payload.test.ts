import { describe, expect, it } from 'vitest'
import { buildQuickGrantSavePayload } from '@/services/quick-grant-payload'
import type { Grant, Period } from '@/services/types'

describe('quick grant payload', () => {
  it('fills hidden dto fields that can otherwise produce api model-binding 400s', () => {
    const payload = buildQuickGrantSavePayload({
      GRANT_PK: '',
      PARTICIPANT_FK: '',
      PLAN_FK: '',
      VESTING_SCHEDULE_FK: '',
      TERMINATION_FK: '',
      SHARES: 100000,
      OPTION_PRICE: 0,
      DATE_OF_GRANT: '',
      VESTING_START: '2024-01-01'
    } as Grant, [
      {
        PERIOD_PK: '',
        PERIOD_AMOUNT: 1,
        PERIOD_TYPE_FK: 2,
        AMOUNT_TYPE_FK: 2,
        AMOUNT: 2.083333,
        INCREMENTS: 36,
        ORDER: 0,
        EVEN_OVER_N: 0
      } as Period
    ])

    expect(payload.Data.GRANT_PK).toBe('00000000-0000-0000-0000-000000000000')
    expect(payload.Data.GROUP_FK).toBe('00000000-0000-0000-0000-000000000000')
    expect(payload.Data.DATE_OF_GRANT).toBe('0001-01-01T00:00:00')
    expect(payload.Data.PARTICIPANT_FK).toBeNull()
    expect(payload.Children[0].PERIOD_PK).toBe('00000000-0000-0000-0000-000000000000')
    expect(payload.Children[0].SCHEDULE_FK).toBe('00000000-0000-0000-0000-000000000000')
    expect(payload.Children[0].CREATED).toBe('0001-01-01T00:00:00')
  })

  it('preserves populated dto fields from the api', () => {
    const payload = buildQuickGrantSavePayload({
      GRANT_PK: '11111111-1111-1111-1111-111111111111',
      GROUP_FK: '22222222-2222-2222-2222-222222222222',
      PARTICIPANT_FK: null,
      PLAN_FK: null,
      VESTING_SCHEDULE_FK: null,
      TERMINATION_FK: null,
      SHARES: 100000,
      OPTION_PRICE: 0,
      DATE_OF_GRANT: '2024-01-01T00:00:00',
      VESTING_START: '2024-01-01',
      CREATED: '2024-01-01T00:00:00',
      UPDATED: '2024-01-02T00:00:00'
    }, [])

    expect(payload.Data.GRANT_PK).toBe('11111111-1111-1111-1111-111111111111')
    expect(payload.Data.GROUP_FK).toBe('22222222-2222-2222-2222-222222222222')
    expect(payload.Data.DATE_OF_GRANT).toBe('2024-01-01T00:00:00')
    expect(payload.Data.UPDATED).toBe('2024-01-02T00:00:00')
  })
})
