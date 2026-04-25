import type { Grant, ParticipantDetail, Period, Schedule, StockClass, StockHolder, Termination, Plan } from '@/services/types'

export function validateStockClass(stockClass: StockClass): string[] {
  const errors: string[] = []
  if (!stockClass.NAME?.trim()) {
    errors.push('Stock class name is required.')
  }
  if (!stockClass.TOTAL_SHARES || stockClass.TOTAL_SHARES < 1) {
    errors.push('Total shares must be greater than zero.')
  }
  return errors
}

export function validatePlan(plan: Plan): string[] {
  const errors: string[] = []
  if (!plan.NAME?.trim()) {
    errors.push('Plan name is required.')
  }
  if (!plan.STOCK_CLASS_FK) {
    errors.push('Select a stock class.')
  }
  if (!plan.TOTAL_SHARES || plan.TOTAL_SHARES < 1) {
    errors.push('Total shares must be greater than zero.')
  }
  return errors
}

export function validatePeriods(periods: Period[]): string[] {
  const errors: string[] = []

  periods.forEach((period, index) => {
    period.ORDER = index
    const issues: string[] = []

    if (!period.PERIOD_AMOUNT) {
      issues.push('period amount')
    }
    if (!period.PERIOD_TYPE_FK) {
      issues.push('period duration')
    }
    if (!period.AMOUNT_TYPE_FK) {
      issues.push('amount type')
    }
    if (!period.AMOUNT && period.EVEN_OVER_N !== 1) {
      issues.push('amount')
    }
    if (!period.INCREMENTS && period.EVEN_OVER_N !== 2) {
      issues.push('number of periods')
    }

    if (issues.length > 0) {
      errors.push(`Period ${index + 1} is missing ${issues.join(', ')}.`)
    }
  })

  return errors
}

export function validateSchedule(schedule: Schedule, periods: Period[]): string[] {
  const errors: string[] = []
  if (!schedule.NAME?.trim()) {
    errors.push('Schedule name is required.')
  }
  return errors.concat(validatePeriods(periods))
}

export function validateParticipant(participant: ParticipantDetail): string[] {
  const errors: string[] = []
  if (!participant.NAME?.trim()) {
    errors.push('Participant name is required.')
  }
  if (!participant.PARTICIPANT_TYPE_FK) {
    errors.push('Participant type is required.')
  }

  if (participant.USER_ACTION === 'create_user' || participant.USER_ACTION === 'update_user') {
    if (!participant.EMAIL_ADDRESS?.trim()) {
      errors.push('Email address is required for user actions.')
    }
    if (!participant.GOOGLE_USER && !participant.USER_NAME?.trim()) {
      errors.push('Username is required for non-Google users.')
    }
  }

  return errors
}

export function validateGrant(grant: Grant): string[] {
  const errors: string[] = []
  if (!grant.PARTICIPANT_FK) {
    errors.push('Select a participant.')
  }
  if (!grant.PLAN_FK) {
    errors.push('Select a plan.')
  }
  if (!grant.VESTING_SCHEDULE_FK) {
    errors.push('Select a vesting schedule.')
  }
  if (!grant.DATE_OF_GRANT) {
    errors.push('Grant date is required.')
  }
  if (!grant.VESTING_START) {
    errors.push('Vesting start date is required.')
  }
  if (!grant.SHARES || grant.SHARES < 1) {
    errors.push('Granted shares must be greater than zero.')
  }
  return errors
}

export function validateStockHolder(stockHolder: StockHolder): string[] {
  const errors: string[] = []
  if (!stockHolder.PARTICIPANT_FK) {
    errors.push('Select a participant.')
  }
  if (!stockHolder.STOCK_CLASS_FK) {
    errors.push('Select a stock class.')
  }
  if (!stockHolder.DATE_OF_SALE) {
    errors.push('Sale date is required.')
  }
  if (!stockHolder.SHARES || stockHolder.SHARES < 1) {
    errors.push('Shares must be greater than zero.')
  }
  return errors
}

export function validateTermination(termination: Termination): string[] {
  const errors: string[] = []
  if (!termination.NAME?.trim()) {
    errors.push('Termination rule name is required.')
  }

  if (termination.IS_ABSOLUTE) {
    if (!termination.ABSOLUTE_DATE) {
      errors.push('Absolute date is required.')
    }
  } else {
    if (!termination.TERM_FROM_FK) {
      errors.push('Select the termination anchor.')
    }
  }

  return errors
}
