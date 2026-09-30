<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue';
import type { Participant, Reward, Redemption } from './types';
import { getParticipants, getRewards, getRedemptions, ApiRequestError } from './api';
import Filters from './components/Filters.vue';
import RedemptionsTable from './components/RedemptionsTable.vue';
import NewRedemptionForm from './components/NewRedemptionForm.vue';

// ---- State ----
const participants = ref<Participant[]>([]);
const rewards = ref<Reward[]>([]);
const redemptions = ref<Redemption[]>([]);

// Filter values (v-model'd to Filters.vue).
const statusFilter = ref<string>('All');
const countryFilter = ref<string>('');

// Page-level state for loading, error, empty.
const loading = ref<boolean>(false);
const listError = ref<string | null>(null);

// ---- Derived ----
// Unique country codes for the country dropdown, sorted alphabetically.
const availableCountries = computed<string[]>(() => {
  const set = new Set<string>();
  for (const r of redemptions.value) set.add(r.countryCode);
  return Array.from(set).sort();
});

// ---- Actions ----
async function loadRedemptions() {
  loading.value = true;
  listError.value = null;
  try {
    redemptions.value = await getRedemptions(statusFilter.value, countryFilter.value);
  } catch (err) {
    if (err instanceof ApiRequestError) {
      listError.value = err.message;
    } else {
      listError.value = 'Could not reach the API. Is it running?';
    }
    redemptions.value = [];
  } finally {
    loading.value = false;
  }
}

async function loadDropdownData() {
  try {
    const [p, r] = await Promise.all([getParticipants(), getRewards()]);
    participants.value = p;
    rewards.value = r;
  } catch (err) {
    // If this fails, the form dropdowns will be empty. Show the same list error.
    listError.value = 'Could not load participants/rewards from the API.';
  }
}

// ---- Lifecycle ----
onMounted(async () => {
  await loadDropdownData();
  await loadRedemptions();
});

// Refetch whenever a filter changes.
watch([statusFilter, countryFilter], loadRedemptions);
</script>

<template>
  <div class="app">
    <header>
      <h1>Rewards Redemptions</h1>
      <p class="subtitle">Junior Full-Stack take-home — Vue 3 + TypeScript</p>
    </header>

    <NewRedemptionForm
      :participants="participants"
      :rewards="rewards"
      @created="loadRedemptions"
    />

    <section class="list-section">
      <h2>Redemptions</h2>

      <Filters
        :status="statusFilter"
        :country-code="countryFilter"
        :available-countries="availableCountries"
        @update:status="statusFilter = $event"
        @update:country-code="countryFilter = $event"
      />

      <p v-if="loading" class="state">Loading…</p>

      <p v-else-if="listError" class="state error">
        {{ listError }}
      </p>

      <p v-else-if="redemptions.length === 0" class="state empty">
        No redemptions match the current filters.
      </p>

      <RedemptionsTable v-else :redemptions="redemptions" />
    </section>
  </div>
</template>

<style scoped>
.app {
  max-width: 1000px;
  margin: 0 auto;
  padding: 2rem 1.5rem;
}

header {
  margin-bottom: 1.5rem;
}

h1 {
  margin: 0 0 0.25rem;
  font-size: 1.6rem;
}

.subtitle {
  margin: 0;
  color: #666;
  font-size: 0.9rem;
}

.list-section {
  margin-top: 1.5rem;
}

.list-section h2 {
  font-size: 1.1rem;
  margin-bottom: 0.75rem;
}

.state {
  padding: 0.75rem 1rem;
  border-radius: 4px;
  background: #f4f4f4;
  color: #555;
  font-size: 0.95rem;
}

.state.error {
  background: #fbdcdc;
  color: #8a1f1f;
}

.state.empty {
  background: #fafafa;
  color: #777;
  font-style: italic;
}
</style>