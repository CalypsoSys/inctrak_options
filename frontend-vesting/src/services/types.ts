export type ApiResponse = {
  success?: boolean
  message?: string
}

export type QuickInterpretResponse = ApiResponse & {
  summary?: string
  Periods: Period[]
  PeriodTypes: PeriodType[]
  AmountTypes: AmountType[]
}

export type PeriodType = {
  PERIOD_TYPE_PK: number
  NAME: string
}

export type AmountType = {
  AMOUNT_TYPE_PK: number
  NAME: string
}

export type Period = {
  PERIOD_PK?: string
  PERIOD_AMOUNT: number
  PERIOD_TYPE_FK: number | null
  AMOUNT_TYPE_FK: number | null
  AMOUNT: number
  INCREMENTS: number
  ORDER: number
  EVEN_OVER_N: number
}

export type Grant = {
  GRANT_PK: string
  PARTICIPANT_FK: string | null
  PLAN_FK: string | null
  VESTING_SCHEDULE_FK: string | null
  TERMINATION_FK: string | null
  SHARES: number
  OPTION_PRICE: number
  DATE_OF_GRANT: string
  VESTING_START: string
}

export type VestScheduleEntry = {
  Order: number
  VestDate: string
  Percent: number
  Shares: number
  TotalPercent: number
  TotalShares: number
  IsVested: boolean
}
