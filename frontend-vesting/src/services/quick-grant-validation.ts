import { shouldShowAmountPerStep, shouldShowNumberOfSteps } from '@/services/period-editor'
import type { Grant, Period } from '@/services/types'

export function getQuickGrantValidationMessage(grant: Grant, periods: Period[] | null | undefined): string {
  if (isPositiveNumber(grant.SHARES) === false) {
    return 'Enter a Shares Granted value greater than zero before calculating vesting.'
  }

  if (isValidDateInput(grant.VESTING_START) === false) {
    return 'Enter a Vesting Start date before calculating vesting.'
  }

  const safePeriods = Array.isArray(periods) ? periods : []

  if (safePeriods.length === 0) {
    return 'Add at least one vesting period before calculating vesting.'
  }

  for (const [index, period] of safePeriods.entries()) {
    const periodLabel = `Period ${index + 1}`

    if (isPositiveInteger(period.PERIOD_AMOUNT) === false) {
      return `${periodLabel}: enter a Length of Each Step greater than zero.`
    }

    if (period.PERIOD_TYPE_FK == null) {
      return `${periodLabel}: select a Step Unit.`
    }

    if (period.AMOUNT_TYPE_FK == null) {
      return `${periodLabel}: select what the period vests in.`
    }

    if (shouldShowAmountPerStep(period.EVEN_OVER_N) && isPositiveNumber(period.AMOUNT) === false) {
      return `${periodLabel}: enter an Amount Per Step greater than zero.`
    }

    if (shouldShowNumberOfSteps(period.EVEN_OVER_N) && isPositiveInteger(period.INCREMENTS) === false) {
      return `${periodLabel}: enter a Number of Steps greater than zero.`
    }
  }

  return ''
}

function isPositiveNumber(value: unknown): boolean {
  return typeof value === 'number' && Number.isFinite(value) && value > 0
}

function isPositiveInteger(value: unknown): boolean {
  return Number.isInteger(value) && isPositiveNumber(value)
}

function isValidDateInput(value: unknown): boolean {
  if (typeof value !== 'string' || /^\d{4}-\d{2}-\d{2}$/.test(value) === false) {
    return false
  }

  const parsed = new Date(`${value}T00:00:00Z`)
  return Number.isNaN(parsed.getTime()) === false
}
