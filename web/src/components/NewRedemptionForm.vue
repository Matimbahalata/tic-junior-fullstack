<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import type { Participant, Reward } from '../types';
import { createRedemption } from '../api';
import { ApiRequestError } from '../api';

// Props: participants and rewards come from the parent (App.vue fetches them once).
const props = defineProps<{
  participants: Participant[];
  rewards: Reward[];
}>();

// Emitted when a redemption is created, so the parent can refresh the list.
const emit = defineEmits<{
  (e: 'created'): void;
}>();

// Form state.
const selectedParticipantId = ref<number | null>(null);
const selectedRewardId = ref<number | null>(null);

// UI state.
const submitting = ref(false);
const errorMessage = ref<string | null>(null);
const successMessage = ref<string | null>(null);

// The currently selected participant object (for showing points balance).
const selectedParticipant = computed(() =>
  props.participants.find((p) => p.participantId === selectedParticipantId.value) ?? null,
);

// Clear status messages whenever the user changes a dropdown.
watch([selectedParticipantId, selectedRewardId], () => {
  errorMessage.value = null;
  successMessage.value = null;
});

async function onSubmit() {
  errorMessage.value = null;
  successMessage.value = null;

  if (selectedParticipantId.value === null || selectedRewardId.value === null) {
    errorMessage.value = 'Please select both a participant and a reward.';
    return;
  }

  submitting.value = true;
  try {
    const result = await createRedemption({
      participantId: selectedParticipantId.value,
      rewardId: selectedRewardId.value,
    });

    successMessage.value = `Redemption #${result.redemptionId} created. Remaining balance: ${result.remainingPointsBalance} points.`;

    // Reset the form.
    selectedParticipantId.value = null;
    selectedRewardId.value = null;

    // Tell the parent to refresh the table.
    emit('created');
  } catch (err) {
    if (err instanceof ApiRequestError) {
      // Show the API's message (e.g., "You do not have enough points...").
      errorMessage.value = err.message;
    } else {
      errorMessage.value = 'Something went wrong. Please try again.';
    }
  } finally {
    submitting.value = false;
  }
}
</script>

<template>
  <section class="new-redemption">
    <h2>New redemption</h2>

    <form @submit.prevent="onSubmit">
      <label>
        Participant
        <select v-model="selectedParticipantId">
          <option :value="null" disabled>Choose a participant</option>
          <option
            v-for="p in props.participants"
            :key="p.participantId"
            :value="p.participantId"
          >
            {{ p.fullName }} ({{ p.countryCode }})
            {{ p.isActive ? '' : '— inactive' }}
          </option>
        </select>
      </label>

      <label>
        Reward
        <select v-model="selectedRewardId">
          <option :value="null" disabled>Choose a reward</option>
          <option
            v-for="r in props.rewards"
            :key="r.rewardId"
            :value="r.rewardId"
            :disabled="!r.isActive"
          >
            {{ r.name }} — {{ r.pointsCost }} pts
            {{ r.isActive ? '' : '— inactive' }}
          </option>
        </select>
      </label>

      <p v-if="selectedParticipant" class="balance">
        Current balance: <strong>{{ selectedParticipant.pointsBalance }} points</strong>
      </p>

      <button type="submit" :disabled="submitting">
        {{ submitting ? 'Submitting…' : 'Redeem' }}
      </button>
    </form>

    <p v-if="errorMessage" class="message error">{{ errorMessage }}</p>
    <p v-if="successMessage" class="message success">{{ successMessage }}</p>
  </section>
</template>

<style scoped>
.new-redemption {
  border: 1px solid #ddd;
  border-radius: 6px;
  padding: 1rem 1.25rem;
  margin-bottom: 1.5rem;
  background: #fafafa;
}

h2 {
  margin-top: 0;
  font-size: 1.1rem;
}

form {
  display: grid;
  grid-template-columns: 1fr 1fr auto;
  gap: 1rem;
  align-items: end;
}

label {
  display: flex;
  flex-direction: column;
  font-size: 0.85rem;
  color: #555;
}

select {
  margin-top: 0.25rem;
  padding: 0.4rem 0.5rem;
  font-size: 0.95rem;
  border: 1px solid #ccc;
  border-radius: 4px;
  background: #fff;
}

.balance {
  grid-column: 1 / -1;
  margin: 0;
  font-size: 0.9rem;
  color: #333;
}

button {
  padding: 0.5rem 1rem;
  font-size: 0.95rem;
  background: #2c5cc5;
  color: #fff;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}

button:hover:not(:disabled) {
  background: #244ea8;
}

button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.message {
  margin-top: 0.75rem;
  padding: 0.5rem 0.75rem;
  border-radius: 4px;
  font-size: 0.9rem;
}

.message.error {
  background: #fbdcdc;
  color: #8a1f1f;
}

.message.success {
  background: #d9f2e0;
  color: #176b2f;
}
</style>