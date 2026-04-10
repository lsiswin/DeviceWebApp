<script setup lang="ts">
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { clearAuth, getCurrentUser } from '../api/client'

const route = useRoute()
const router = useRouter()
const userName = computed(() => getCurrentUser() || 'Admin')

async function logout() {
  clearAuth()
  await router.replace('/login')
}
</script>

<template>
  <div class="admin-layout">
    <aside class="sidebar">
      <h2>DeviceAdmin</h2>
      <router-link to="/dashboard" :class="{ active: route.path === '/dashboard' }">仪表盘</router-link>
      <router-link to="/devices" :class="{ active: route.path === '/devices' }">设备管理</router-link>
      <router-link to="/datapoints" :class="{ active: route.path === '/datapoints' }">数据点管理</router-link>
      <router-link to="/permissions" :class="{ active: route.path === '/permissions' }">权限中心</router-link>
      <router-link to="/operation-logs" :class="{ active: route.path === '/operation-logs' }">操作日志</router-link>
    </aside>
    <div class="main">
      <header class="topbar">
        <span>当前用户：{{ userName }}</span>
        <button class="secondary" @click="logout">退出登录</button>
      </header>
      <section class="content">
        <router-view />
      </section>
    </div>
  </div>
</template>
