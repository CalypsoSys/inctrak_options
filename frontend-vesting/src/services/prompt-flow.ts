export function canAutoCalculateFromPromptResult(sharesGranted: number, vestingStart: string, periodCount: number): boolean {
  return sharesGranted > 0 && vestingStart !== '' && periodCount > 0
}

export function isAiProvider(provider?: string): boolean {
  return provider === 'llamasharp' || provider === 'local-http'
}

export function shouldShowTryAlternate(alternateProvider?: string): boolean {
  return Boolean(alternateProvider)
}

export function shouldShowStillNotRight(provider?: string): boolean {
  return provider !== '' && provider !== undefined && isAiProvider(provider) === false
}

export function shouldShowUseAiInstead(revealAiChoice: boolean, provider?: string): boolean {
  return revealAiChoice && shouldShowStillNotRight(provider)
}
