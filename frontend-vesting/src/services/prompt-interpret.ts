import { normalizeQuickStartDate } from '@/services/quick-vesting'
import type { AmountType, Grant, Period, PeriodType, QuickInterpretResponse } from '@/services/types'

export function buildPromptGrantPatch(response: QuickInterpretResponse): Partial<Grant> {
  const patch: Partial<Grant> = {}

  if (typeof response.sharesGranted === 'number' && Number.isFinite(response.sharesGranted)) {
    patch.SHARES = response.sharesGranted
  }

  if (response.vestingStart) {
    patch.VESTING_START = normalizeQuickStartDate(response.vestingStart)
  }

  return patch
}

export function getPromptPeriods(response: QuickInterpretResponse): Period[] {
  if (Array.isArray(response.Periods)) {
    return response.Periods
  }

  return Array.isArray(response.periods) ? response.periods : []
}

export function getPromptPeriodTypes(response: QuickInterpretResponse): PeriodType[] {
  if (Array.isArray(response.PeriodTypes)) {
    return response.PeriodTypes
  }

  return Array.isArray(response.periodTypes) ? response.periodTypes : []
}

export function getPromptAmountTypes(response: QuickInterpretResponse): AmountType[] {
  if (Array.isArray(response.AmountTypes)) {
    return response.AmountTypes
  }

  return Array.isArray(response.amountTypes) ? response.amountTypes : []
}
