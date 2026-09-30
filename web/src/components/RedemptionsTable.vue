<script setup lang="ts">
import type { Redemption } from '../types';

// Props: the rows to display. The parent fetches and passes them in.
defineProps<{
  redemptions: Redemption[];
}>();

// Small helper to format the ISO date string the API returns.
function formatDate(iso: string): string {
  const d = new Date(iso);
  // Display as "2026-09-30 06:44" — local time, readable.
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}`;
}

// Map a status to a CSS class for colour-coding.
function statusClass(status: string): string {
  return `status status-${status.toLowerCase()}`;
}
</script>

<template>
  <table class="redemptions-table">
    <thead>
      <tr>
        <th>ID</th>
        <th>Participant</th>
        <th>Country</th>
        <th>Reward</th>
        <th>Points</th>
        <th>Status</th>
        <th>Requested</th>
      </tr>
    </thead>
    <tbody>
      <tr v-for="r in redemptions" :key="r.redemptionId">
        <td>{{ r.redemptionId }}</td>
        <td>{{ r.participantName }}</td>
        <td>{{ r.countryCode }}</td>
        <td>{{ r.rewardName }}</td>
        <td class="number">{{ r.pointsCost }}</td>
        <td>
          <span :class="statusClass(r.status)">{{ r.status }}</span>
        </td>
        <td>{{ formatDate(r.requestedAt) }}</td>
      </tr>
    </tbody>
  </table>
</template>

<style scoped>
.redemptions-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.95rem;
}

th,
td {
  text-align: left;
  padding: 0.5rem 0.75rem;
  border-bottom: 1px solid #eee;
}

th {
  background: #f7f7f7;
  font-weight: 600;
  font-size: 0.85rem;
  text-transform: uppercase;
  letter-spacing: 0.03em;
  color: #555;
}

tbody tr:hover {
  background: #fafafa;
}

.number {
  text-align: right;
}

.status {
  display: inline-block;
  padding: 0.15rem 0.5rem;
  border-radius: 12px;
  font-size: 0.8rem;
  font-weight: 500;
}

.status-pending {
  background: #fff4d6;
  color: #8a5a00;
}

.status-completed {
  background: #d9f2e0;
  color: #176b2f;
}

.status-failed {
  background: #fbdcdc;
  color: #8a1f1f;
}
</style>