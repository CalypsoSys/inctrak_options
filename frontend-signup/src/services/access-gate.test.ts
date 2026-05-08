import { describe, expect, it } from 'vitest'
import { accessGate } from '@/services/access-gate'

describe('accessGate', () => {
  it('keeps public signup temporarily disabled', () => {
    expect(accessGate.disabled).toBe(true)
    expect(accessGate.title).toContain('updated')
    expect(accessGate.message.length).toBeGreaterThan(20)
  })
})
