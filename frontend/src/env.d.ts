/// <reference types="vite/client" />

declare global {
  interface Window {
    IncTrakSiteConfig?: {
      apiBaseUrl?: string
    }
  }
}

export {}
