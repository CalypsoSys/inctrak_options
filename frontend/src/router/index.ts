import { createRouter, createWebHashHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { resolveLegacyPath } from '@/router/legacy-paths'

import DashboardLayout from '@/layouts/DashboardLayout.vue'
import HomePage from '@/pages/HomePage.vue'
import AboutPage from '@/pages/AboutPage.vue'
import ContactPage from '@/pages/ContactPage.vue'
import PrivacyPage from '@/pages/PrivacyPage.vue'
import LoginPage from '@/pages/LoginPage.vue'
import ActivateAccountPage from '@/pages/ActivateAccountPage.vue'
import ResetPasswordRequestPage from '@/pages/ResetPasswordRequestPage.vue'
import ResetPasswordPage from '@/pages/ResetPasswordPage.vue'
import AcceptTermsPage from '@/pages/AcceptTermsPage.vue'
import StockClassesPage from '@/pages/StockClassesPage.vue'
import StockHoldersPage from '@/pages/StockHoldersPage.vue'
import PlansPage from '@/pages/PlansPage.vue'
import SchedulesPage from '@/pages/SchedulesPage.vue'
import TerminationsPage from '@/pages/TerminationsPage.vue'
import ParticipantsPage from '@/pages/ParticipantsPage.vue'
import GrantsPage from '@/pages/GrantsPage.vue'
import ParticipantStocksPage from '@/pages/ParticipantStocksPage.vue'
import ParticipantSummaryPage from '@/pages/ParticipantSummaryPage.vue'
import ParticipantGrantsPage from '@/pages/ParticipantGrantsPage.vue'

declare module 'vue-router' {
  interface RouteMeta {
    title: string
    requiresAuth?: boolean
    requiresAdmin?: boolean
    requiresOptionee?: boolean
    publicOnly?: boolean
  }
}

const router = createRouter({
  history: createWebHashHistory(),
  routes: [
    {
      path: '/',
      component: DashboardLayout,
      children: [
        { path: '', name: 'home', component: HomePage, meta: { title: 'Dashboard' } },
        { path: 'about', name: 'about', component: AboutPage, meta: { title: 'About' } },
        { path: 'support/contact', name: 'contact', component: ContactPage, meta: { title: 'Contact' } },
        { path: 'legal/privacy', name: 'privacy', component: PrivacyPage, meta: { title: 'Privacy Policy' } },
        { path: 'auth/login', name: 'login', component: LoginPage, meta: { title: 'Login', publicOnly: true } },
        { path: 'auth/activate/:key', name: 'activate-account', component: ActivateAccountPage, meta: { title: 'Activate Account' } },
        { path: 'auth/reset-password', name: 'reset-password-request', component: ResetPasswordRequestPage, meta: { title: 'Reset Password', publicOnly: true } },
        { path: 'auth/reset-password/:key', name: 'reset-password', component: ResetPasswordPage, meta: { title: 'Set Password' } },
        { path: 'auth/accept-terms/:key', name: 'accept-terms', component: AcceptTermsPage, meta: { title: 'Accept Terms' } },
        { path: 'admin/stock-classes/:id?', name: 'stock-classes', component: StockClassesPage, meta: { title: 'Stock Classes', requiresAuth: true, requiresAdmin: true } },
        { path: 'admin/stock-holders/:id?', name: 'stock-holders', component: StockHoldersPage, meta: { title: 'Stock Holders', requiresAuth: true, requiresAdmin: true } },
        { path: 'admin/plans/:id?', name: 'plans', component: PlansPage, meta: { title: 'Plans', requiresAuth: true, requiresAdmin: true } },
        { path: 'admin/vesting-schedules/:id?', name: 'vesting-schedules', component: SchedulesPage, meta: { title: 'Vesting Schedules', requiresAuth: true, requiresAdmin: true } },
        { path: 'admin/termination-rules/:id?', name: 'termination-rules', component: TerminationsPage, meta: { title: 'Termination Rules', requiresAuth: true, requiresAdmin: true } },
        { path: 'admin/participants/:id?', name: 'participants', component: ParticipantsPage, meta: { title: 'Participants', requiresAuth: true, requiresAdmin: true } },
        { path: 'admin/grants/:id?', name: 'grants', component: GrantsPage, meta: { title: 'Grants', requiresAuth: true, requiresAdmin: true } },
        { path: 'participant/stocks', name: 'participant-stocks', component: ParticipantStocksPage, meta: { title: 'Stock Summary', requiresAuth: true, requiresOptionee: true } },
        { path: 'participant/options', name: 'participant-options', component: ParticipantSummaryPage, meta: { title: 'Option Summary', requiresAuth: true, requiresOptionee: true } },
        { path: 'participant/grants/:id?', name: 'participant-grants', component: ParticipantGrantsPage, meta: { title: 'Grant Details', requiresAuth: true, requiresOptionee: true } }
      ]
    },
    {
      path: '/:pathMatch(.*)*',
      redirect: (to) => {
        const joinedPath = `/${(to.params.pathMatch as string[]).join('/')}`
        const resolved = resolveLegacyPath(joinedPath)
        return resolved ?? '/'
      }
    }
  ]
})

router.beforeEach(async (to) => {
  const authStore = useAuthStore()
  await authStore.initialize()

  if (to.meta.publicOnly && authStore.isAuthenticated) {
    return authStore.isAdmin ? { name: 'stock-classes' } : { name: 'participant-stocks' }
  }

  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    return { name: 'login', query: { next: to.fullPath } }
  }

  if (to.meta.requiresAdmin && !authStore.isAdmin) {
    return { name: authStore.isOptionee ? 'participant-stocks' : 'home' }
  }

  if (to.meta.requiresOptionee && !authStore.isOptionee) {
    return { name: authStore.isAdmin ? 'stock-classes' : 'home' }
  }

  return true
})

export default router
