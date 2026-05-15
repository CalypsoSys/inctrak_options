import { normalizeQuickStartDate } from '@/services/quick-vesting'
import type { QuickGrantResponse } from '@/services/vesting-service'
import type { AmountType, Grant, Period, PeriodType } from '@/services/types'

export type QuickGrantDefaults = {
  grantPatch: Partial<Grant>
  periods: Period[]
  periodTypes: PeriodType[]
  amountTypes: AmountType[]
}

export function buildQuickGrantDefaults(response: QuickGrantResponse): QuickGrantDefaults {
  const grantPatch = response.Grant
    ? {
        ...response.Grant,
        VESTING_START: normalizeQuickStartDate(response.Grant.VESTING_START)
      }
    : {}

  return {
    grantPatch,
    periods: Array.isArray(response.Periods) ? response.Periods : [],
    periodTypes: Array.isArray(response.PeriodTypes) ? response.PeriodTypes : [],
    amountTypes: Array.isArray(response.AmountTypes) ? response.AmountTypes : []
  }
}
