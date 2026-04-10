import { createRouter, createWebHistory } from 'vue-router'
import { getToken } from '../api/client'
import DashboardView from '../views/DashboardView.vue'
import DataPointsView from '../views/DataPointsView.vue'
import DevicesView from '../views/DevicesView.vue'
import LayoutView from '../views/LayoutView.vue'
import LoginView from '../views/LoginView.vue'
import OperationLogsView from '../views/OperationLogsView.vue'
import PermissionsView from '../views/PermissionsView.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/login', name: 'login', component: LoginView },
    {
      path: '/',
      component: LayoutView,
      children: [
        { path: '', redirect: '/dashboard' },
        { path: '/dashboard', name: 'dashboard', component: DashboardView, meta: { requiresAuth: true } },
        { path: '/devices', name: 'devices', component: DevicesView, meta: { requiresAuth: true } },
        { path: '/datapoints', name: 'datapoints', component: DataPointsView, meta: { requiresAuth: true } },
        { path: '/permissions', name: 'permissions', component: PermissionsView, meta: { requiresAuth: true } },
        { path: '/operation-logs', name: 'operation-logs', component: OperationLogsView, meta: { requiresAuth: true } }
      ]
    }
  ]
})

router.beforeEach((to) => {
  if (to.path === '/login') {
    return true
  }

  const token = getToken()
  if (!token) {
    return '/login'
  }

  return true
})

export default router
