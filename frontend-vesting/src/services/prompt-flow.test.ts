import { describe, expect, it } from 'vitest'
import {
  canAutoCalculateFromPromptResult,
  isAiProvider,
  shouldShowStillNotRight,
  shouldShowTryAlternate,
  shouldShowUseAiInstead
} from '@/services/prompt-flow'

describe('prompt-flow', () => {
  it('auto-calculates only when grant fields and periods are present', () => {
    expect(canAutoCalculateFromPromptResult(50000, '2023-01-01', 2)).toBe(true)
    expect(canAutoCalculateFromPromptResult(0, '2023-01-01', 2)).toBe(false)
    expect(canAutoCalculateFromPromptResult(50000, '', 2)).toBe(false)
    expect(canAutoCalculateFromPromptResult(50000, '2023-01-01', 0)).toBe(false)
  })

  it('recognizes AI and non-AI providers for prompt actions', () => {
    expect(isAiProvider('llamasharp')).toBe(true)
    expect(isAiProvider('local-http')).toBe(true)
    expect(isAiProvider('parser')).toBe(false)
    expect(shouldShowStillNotRight('parser')).toBe(true)
    expect(shouldShowStillNotRight('llamasharp')).toBe(false)
  })

  it('shows alternate and AI escalation actions only when appropriate', () => {
    expect(shouldShowTryAlternate('parser')).toBe(true)
    expect(shouldShowTryAlternate('')).toBe(false)
    expect(shouldShowUseAiInstead(true, 'parser')).toBe(true)
    expect(shouldShowUseAiInstead(false, 'parser')).toBe(false)
    expect(shouldShowUseAiInstead(true, 'llamasharp')).toBe(false)
  })
})
