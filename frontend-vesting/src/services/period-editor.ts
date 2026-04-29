export function shouldShowAmountPerStep(evenOverN: number): boolean {
  return evenOverN !== 1
}

export function shouldShowNumberOfSteps(evenOverN: number): boolean {
  return evenOverN !== 2
}
