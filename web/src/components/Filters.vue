<script setup lang="ts">
import { computed } from 'vue';
import type { RedemptionStatus } from '../types';

// Props: current filter values from the parent, plus the available country codes.
const props = defineProps<{
  status: string;
  countryCode: string;
  availableCountries: string[];
}>();

// Events the parent listens to when a filter changes.
const emit = defineEmits<{
  (e: 'update:status', value: string): void;
  (e: 'update:countryCode', value: string): void;
}>();

// Status options: "All" plus the three real statuses.
const statusOptions = computed<{ label: string; value: string }[]>(() => [
  { label: 'All', value: 'All' },
  { label: 'Pending', value: 'Pending' satisfies RedemptionStatus },
  { label: 'Completed', value: 'Completed' satisfies RedemptionStatus },
  { label: 'Failed', value: 'Failed' satisfies RedemptionStatus },
]);

// Country options: "All" plus whatever countries came back from the API.
const countryOptions = computed(() => [
  { label: 'All countries', value: '' },
  ...props.availableCountries.map((code) => ({ label: code, value: code })),
]);

function onStatusChange(event: Event) {
  const value = (event.target as HTMLSelectElement).value;
  emit('update:status', value);
}

function onCountryChange(event: Event) {
  const value = (event.target as HTMLSelectElement).value;
  emit('update:countryCode', value);
}
</script>

<template>
  <div class="filters">
    <label>
      Status
      <select :value="status" @change="onStatusChange">
        <option v-for="option in statusOptions" :key="option.value" :value="option.value">
          {{ option.label }}
        </option>
      </select>
    </label>

    <label>
      Country
      <select :value="countryCode" @change="onCountryChange">
        <option v-for="option in countryOptions" :key="option.value" :value="option.value">
          {{ option.label }}
        </option>
      </select>
    </label>
  </div>
</template>

<style scoped>
.filters {
  display: flex;
  gap: 1rem;
  margin-bottom: 1rem;
}

label {
  display: flex;
  flex-direction: column;
  font-size: 0.875rem;
  color: #555;
}

select {
  margin-top: 0.25rem;
  padding: 0.35rem 0.5rem;
  font-size: 0.95rem;
  border: 1px solid #ccc;
  border-radius: 4px;
  background: #fff;
}
</style>