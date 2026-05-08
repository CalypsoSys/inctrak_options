import { describe, expect, it } from 'vitest'
import { normalizeQuickStartDate } from '@/services/quick-vesting'

describe('normalizeQuickStartDate', () => {
  it('uses today when the API returns the .NET min date', () => {
    expect(normalizeQuickStartDate('0001-01-01T00:00:00', new Date('2026-04-29T12:00:00Z'))).toBe('2026-04-29')
  })

  it('keeps a real vesting start date unchanged', () => {
    expect(normalizeQuickStartDate('2026-09-15T00:00:00')).toBe('2026-09-15')
  })
})
