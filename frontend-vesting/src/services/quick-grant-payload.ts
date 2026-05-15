import type { Grant, Period } from '@/services/types'

const EMPTY_GUID = '00000000-0000-0000-0000-000000000000'
const EMPTY_DATE = '0001-01-01T00:00:00'

export function buildQuickGrantSavePayload(grant: Grant, periods: Period[]): { Data: Grant; Children: Period[] } {
  return {
    Data: {
      ...grant,
      GRANT_PK: defaultGuid(grant.GRANT_PK),
      GROUP_FK: defaultGuid(grant.GROUP_FK),
      PARTICIPANT_FK: nullableGuid(grant.PARTICIPANT_FK),
      VESTING_SCHEDULE_FK: nullableGuid(grant.VESTING_SCHEDULE_FK),
      TERMINATION_FK: nullableGuid(grant.TERMINATION_FK),
      PLAN_FK: nullableGuid(grant.PLAN_FK),
      CREATED: defaultDate(grant.CREATED),
      UPDATED: defaultDate(grant.UPDATED),
      DATE_OF_GRANT: defaultDate(grant.DATE_OF_GRANT),
      GroupKeyCheck: defaultGuid(grant.GroupKeyCheck)
    },
    Children: periods.map((period) => ({
      ...period,
      PERIOD_PK: defaultGuid(period.PERIOD_PK),
      GROUP_FK: defaultGuid(period.GROUP_FK),
      SCHEDULE_FK: defaultGuid(period.SCHEDULE_FK),
      CREATED: defaultDate(period.CREATED),
      UPDATED: defaultDate(period.UPDATED),
      GroupKeyCheck: defaultGuid(period.GroupKeyCheck)
    }))
  }
}

function defaultGuid(value: string | undefined): string {
  return value && value.trim() ? value : EMPTY_GUID
}

function nullableGuid(value: string | null | undefined): string | null {
  return value && value.trim() ? value : null
}

function defaultDate(value: string | undefined): string {
  return value && value.trim() ? value : EMPTY_DATE
}
