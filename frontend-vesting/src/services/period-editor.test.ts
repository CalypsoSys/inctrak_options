import { describe, expect, it } from 'vitest'
import { shouldShowAmountPerStep, shouldShowNumberOfSteps } from '@/services/period-editor'

describe('period-editor visibility', () => {
  it('hides amount per step when total is split evenly across steps', () => {
    expect(shouldShowAmountPerStep(1)).toBe(false)
    expect(shouldShowAmountPerStep(0)).toBe(true)
  })

  it('hides number of steps when steps are calculated from the amount', () => {
    expect(shouldShowNumberOfSteps(2)).toBe(false)
    expect(shouldShowNumberOfSteps(0)).toBe(true)
  })
})
