import { describe, expect, it } from 'vitest'
import { accessGate } from '@/services/access-gate'

describe('accessGate', () => {
  it('keeps the main app temporarily disabled', () => {
    expect(accessGate.disabled).toBe(true)
    expect(accessGate.title).toContain('Temporarily')
    expect(accessGate.message.length).toBeGreaterThan(20)
  })
})
