import { normalizeQuickStartDate } from '@/services/quick-vesting'
import type { Grant, QuickInterpretResponse } from '@/services/types'

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
