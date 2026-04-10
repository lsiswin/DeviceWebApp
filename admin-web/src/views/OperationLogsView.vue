<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { getOperationLogs } from '../api/client'
import type { OperationLog } from '../types'

const logs = ref<OperationLog[]>([])
const loading = ref(false)
const message = ref('')

async function loadLogs() {
  loading.value = true
  message.value = ''
  try {
    logs.value = await getOperationLogs(200)
  } catch (error) {
    message.value = (error as Error).message
  } finally {
    loading.value = false
  }
}

onMounted(loadLogs)
</script>

<template>
  <div>
    <h1>操作日志</h1>
    <p class="subtitle">展示最近200条后台操作记录</p>
    <p v-if="message" class="error">{{ message }}</p>
    <section class="panel">
      <button @click="loadLogs">刷新日志</button>
      <p v-if="loading">加载中...</p>
      <table v-else>
        <thead>
          <tr>
            <th>时间</th>
            <th>用户</th>
            <th>动作</th>
            <th>资源类型</th>
            <th>资源ID</th>
            <th>详情</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in logs" :key="item.id">
            <td>{{ new Date(item.createdAtUtc).toLocaleString() }}</td>
            <td>{{ item.userName }}</td>
            <td>{{ item.action }}</td>
            <td>{{ item.resourceType }}</td>
            <td>{{ item.resourceId }}</td>
            <td>{{ item.detail }}</td>
          </tr>
        </tbody>
      </table>
    </section>
  </div>
</template>
