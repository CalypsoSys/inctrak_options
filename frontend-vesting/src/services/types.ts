export type ApiResponse = {
  success?: boolean
  message?: string
}

export type FeedbackForm = {
  EmailAddress: string
  Name: string
  MessageTypeFk: number
  Subject: string
  Message: string
}

export type QuickInterpretResponse = ApiResponse & {
  summary?: string
  provider?: string
  alternateProvider?: string
  confidence?: number
  requiresAi?: boolean
  sharesGranted?: number
  vestingStart?: string
  Periods?: Period[]
  PeriodTypes?: PeriodType[]
  AmountTypes?: AmountType[]
  periods?: Period[]
  periodTypes?: PeriodType[]
  amountTypes?: AmountType[]
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
  GROUP_FK?: string
  SCHEDULE_FK?: string
  PERIOD_AMOUNT: number
  PERIOD_TYPE_FK: number | null
  AMOUNT_TYPE_FK: number | null
  AMOUNT: number
  INCREMENTS: number
  ORDER: number
  EVEN_OVER_N: number
  CREATED?: string
  UPDATED?: string
  GroupKeyCheck?: string
}

export type Grant = {
  GRANT_PK: string
  GROUP_FK?: string
  PARTICIPANT_FK: string | null
  PLAN_FK: string | null
  VESTING_SCHEDULE_FK: string | null
  TERMINATION_FK: string | null
  SHARES: number
  OPTION_PRICE: number
  DATE_OF_GRANT: string
  VESTING_START: string
  CREATED?: string
  UPDATED?: string
  GroupKeyCheck?: string
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
