<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { getCurrentRoles, getMyPermissions, register } from '../api/client'
import type { PermissionSummary } from '../types'

const summary = ref<PermissionSummary | null>(null)
const loading = ref(false)
const message = ref('')
const roles = getCurrentRoles()

const form = reactive({
  userName: '',
  password: '',
  role: 'Viewer'
})

const canManageUser = roles.includes('Admin')

async function loadPermissions() {
  loading.value = true
  message.value = ''
  try {
    summary.value = await getMyPermissions()
  } catch (error) {
    message.value = (error as Error).message
  } finally {
    loading.value = false
  }
}

async function createUser() {
  if (!canManageUser) {
    return
  }

  try {
    await register(form.userName, form.password, form.role)
    message.value = '用户创建成功'
    form.userName = ''
    form.password = ''
    form.role = 'Viewer'
  } catch (error) {
    message.value = (error as Error).message
  }
}

onMounted(loadPermissions)
</script>

<template>
  <div>
    <h1>权限中心</h1>
    <p class="subtitle">查看当前账号角色与权限，并按角色创建用户</p>
    <p v-if="message" :class="['message', message.includes('成功') ? 'success' : 'error']">{{ message }}</p>
    <section class="panel" v-if="summary">
      <h3>当前身份</h3>
      <p>用户名：{{ summary.userName }}</p>
      <p>角色：{{ summary.roles.join('、') }}</p>
      <p>权限：</p>
      <ul>
        <li v-for="item in summary.permissions" :key="item">{{ item }}</li>
      </ul>
    </section>
    <section v-if="canManageUser" class="panel">
      <h3>创建用户</h3>
      <form class="form-grid" @submit.prevent="createUser">
        <label>
          用户名
          <input v-model="form.userName" required />
        </label>
        <label>
          密码
          <input v-model="form.password" type="password" required />
        </label>
        <label>
          角色
          <select v-model="form.role">
            <option value="Admin">Admin</option>
            <option value="Operator">Operator</option>
            <option value="Viewer">Viewer</option>
          </select>
        </label>
        <div class="form-buttons">
          <button type="submit">创建用户</button>
        </div>
      </form>
    </section>
    <p v-if="loading">加载中...</p>
  </div>
</template>
