export type ApiResponse = {
  success?: boolean
  login?: boolean
  message?: string
}

export type LoginForm = {
  USER_NAME: string
  PASSWORD: string
  PASSWORD2: string
  EMAIL_ADDRESS: string
  GROUP_NAME: string
  IS_REGISTERING: boolean
  ACCEPT_TERMS: boolean
}

export type LoginResponse = ApiResponse & {
  uuid?: string
  Role?: 'admin' | 'optionee'
}

export type FeedbackForm = {
  EmailAddress: string
  Name: string
  MessageTypeFk: number
  Subject: string
  Message: string
}

export type MessageType = {
  Key: number
  Name: string
}

export type EntitySaveEnvelope<T> = {
  Key: string
  UUID: string
  Data: T
}

export type EntitySaveWithChildrenEnvelope<T, C> = EntitySaveEnvelope<T> & {
  Children: C[]
}

export type SearchType = 'all' | 'any' | 'exact' | 'inline'

export type ParticipantSummary = {
  PARTICIPANT_PK: string
  NAME: string
  TOTAL_GRANTS: number
  GRANTED_SHARES: number
  HAS_USER: boolean
}

export type ParticipantType = {
  PARTICIPANT_TYPE_PK: number
  NAME: string
}

export type ParticipantDetail = {
  PARTICIPANT_PK: string
  PARTICIPANT_TYPE_FK: number
  NAME: string
  USER_NAME: string
  EMAIL_ADDRESS: string
  USER_ACTION: string
  SEND_EMAIL: boolean
}

export type StockClass = {
  STOCK_CLASS_PK: string
  NAME: string
  TOTAL_SHARES: number
}

export type Plan = {
  PLAN_PK: string
  NAME: string
  STOCK_CLASS_FK: string
  STOCK_CLASS_NAME?: string
  TOTAL_SHARES: number
  GRANTED_SHARES?: number
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

export type Schedule = {
  SCHEDULE_PK: string
  NAME: string
  PERIODS?: number
}

export type TermFromType = {
  TERM_FROM_PK: number
  NAME: string
}

export type Termination = {
  TERMINATION_PK: string
  NAME: string
  IS_ABSOLUTE: boolean
  ABSOLUTE_DATE: string
  TERM_FROM_FK: number | null
  SPECIFIC_DATE: string
  YEARS: number
  MONTHS: number
  DAYS: number
}

export type Grant = {
  GRANT_PK: string
  PARTICIPANT_FK: string | null
  PARTICIPANT_NAME?: string
  PLAN_FK: string | null
  PLAN_NAME?: string
  VESTING_SCHEDULE_FK: string | null
  TERMINATION_FK: string | null
  SHARES: number
  OPTION_PRICE: number
  DATE_OF_GRANT: string
  VESTING_START: string
  VEST_NAME?: string
  TerminationDate?: string
  VestingEnd?: string
}

export type StockHolder = {
  STOCK_HOLDER_PK: string
  PARTICIPANT_FK: string | null
  PARTICIPANT_NAME?: string
  STOCK_CLASS_FK: string | null
  STOCK_CLASS_NAME?: string
  SHARES: number
  PRICE: number
  DATE_OF_SALE: string
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
