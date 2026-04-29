import { toDateInputValue } from '@/utils/formatters'

export function normalizeQuickStartDate(value: string | null | undefined, today = new Date()): string {
  const normalized = toDateInputValue(value)
  if (!normalized || normalized === '0001-01-01') {
    return toDateInputValue(today.toISOString())
  }

  return normalized
}
