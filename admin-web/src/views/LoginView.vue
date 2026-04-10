<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { API_BASE_URL, login, setAuth } from '../api/client'

const router = useRouter()
const userName = ref('admin')
const password = ref('Admin@123456')
const loading = ref(false)
const message = ref('')

async function onLogin() {
  loading.value = true
  message.value = ''
  try {
    const result = await login(userName.value, password.value)
    setAuth(result)
    await router.replace('/dashboard')
  } catch (error) {
    message.value = (error as Error).message
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="login-page">
    <div class="login-panel">
      <h1>工业设备管理后台</h1>
      <p class="tip">默认账号：admin / Admin@123456</p>
      <p class="tip">Operator：operator / Operator@123456，Viewer：viewer / Viewer@123456</p>
      <p class="tip">后端地址：{{ API_BASE_URL }}</p>
      <form @submit.prevent="onLogin">
        <label>
          用户名
          <input v-model="userName" required />
        </label>
        <label>
          密码
          <input v-model="password" type="password" required />
        </label>
        <button :disabled="loading" type="submit">{{ loading ? '登录中...' : '登录' }}</button>
      </form>
      <p v-if="message" class="error">{{ message }}</p>
    </div>
  </div>
</template>
