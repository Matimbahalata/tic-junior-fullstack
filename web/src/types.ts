// Status values that the API returns for a redemption.
// Union type instead of plain string so TypeScript catches typos.
export type RedemptionStatus = 'Pending' | 'Completed' | 'Failed';

export interface Participant {
  participantId: number;
  fullName: string;
  countryCode: string;
  pointsBalance: number;
  isActive: boolean;
}

export interface Reward {
  rewardId: number;
  name: string;
  pointsCost: number;
  isActive: boolean;
}

export interface Redemption {
  redemptionId: number;
  participantName: string;
  countryCode: string;
  rewardName: string;
  pointsCost: number;
  status: RedemptionStatus;
  requestedAt: string; // ISO 8601 date string from the API
}

// Shape of the POST /api/redemptions request body.
export interface CreateRedemptionRequest {
  participantId: number;
  rewardId: number;
}

// Shape of the response we get back on a successful create.
export interface RedemptionCreated {
  redemptionId: number;
  participantId: number;
  rewardId: number;
  status: RedemptionStatus;
  requestedAt: string;
  remainingPointsBalance: number;
}

// Shape of an error response from the API (400/404/409/500).
export interface ApiError {
  message: string;
}