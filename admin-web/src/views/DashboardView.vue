<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { getDashboardStats } from '../api/client'
import type { DashboardStats } from '../types'

const stats = ref<DashboardStats | null>(null)
const loading = ref(false)
const message = ref('')

async function loadStats() {
  loading.value = true
  message.value = ''
  try {
    stats.value = await getDashboardStats()
  } catch (error) {
    message.value = (error as Error).message
  } finally {
    loading.value = false
  }
}

onMounted(loadStats)
</script>

<template>
  <div>
    <h1>仪表盘</h1>
    <p class="subtitle">查看设备与数据点整体状态</p>
    <p v-if="message" class="error">{{ message }}</p>
    <p v-if="loading">加载中...</p>
    <div v-if="stats" class="stats-grid">
      <article class="card">
        <h3>设备总数</h3>
        <strong>{{ stats.deviceCount }}</strong>
      </article>
      <article class="card">
        <h3>传感器数量</h3>
        <strong>{{ stats.sensorCount }}</strong>
      </article>
      <article class="card">
        <h3>PLC数量</h3>
        <strong>{{ stats.plcCount }}</strong>
      </article>
      <article class="card">
        <h3>数据点总数</h3>
        <strong>{{ stats.dataPointCount }}</strong>
      </article>
      <article class="card">
        <h3>在线设备</h3>
        <strong>{{ stats.onlineDeviceCount }}</strong>
      </article>
    </div>
  </div>
</template>
