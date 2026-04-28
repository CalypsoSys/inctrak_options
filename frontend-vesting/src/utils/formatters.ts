export function formatNumber(value: number | null | undefined, maximumFractionDigits = 0): string {
  return new Intl.NumberFormat('en-US', {
    maximumFractionDigits,
    minimumFractionDigits: maximumFractionDigits > 0 ? maximumFractionDigits : 0
  }).format(value ?? 0)
}

export function formatDate(value: string | null | undefined): string {
  if (!value) {
    return '—'
  }

  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return value
  }

  return new Intl.DateTimeFormat('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  }).format(date)
}

export function toDateInputValue(value: string | null | undefined): string {
  if (!value) {
    return ''
  }

  return value.includes('T') ? value.split('T')[0] : value
}
