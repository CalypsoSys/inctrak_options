<template>
  <div class="min-h-screen px-4 py-4 md:px-6 md:py-6">
    <div class="mx-auto flex max-w-[1600px] gap-6">
      <AppSidebar />
      <div class="flex min-w-0 flex-1 flex-col gap-6">
        <AppTopbar />
        <main class="min-w-0">
          <RouterView />
        </main>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import AppSidebar from '@/components/AppSidebar.vue'
import AppTopbar from '@/components/AppTopbar.vue'
</script>
