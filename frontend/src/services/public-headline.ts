export function getPublicHeadline(routeName?: string, isAdmin = false, isOptionee = false): string {
  if (isAdmin) {
    return 'Modern equity operations for administrators'
  }

  if (isOptionee) {
    return 'Clear participant access to grants and vesting'
  }

  switch (routeName) {
    case 'home':
      return 'Equity administration for growing companies'
    case 'login':
      return 'Equity management for growing companies'
    case 'contact':
      return 'Talk with the IncTrak team'
    case 'privacy':
      return 'Privacy details for your company and participants'
    case 'about':
      return 'Stock option administration without the spreadsheet chaos'
    default:
      return 'Equity administration built for modern teams'
  }
}
