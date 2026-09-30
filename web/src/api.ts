import type {
  Participant,
  Reward,
  Redemption,
  CreateRedemptionRequest,
  RedemptionCreated,
  ApiError,
} from './types';

// Base URL for the API. The HTTP port from `dotnet run`.
const API_BASE = 'http://localhost:5213/api';

// Thrown when the API returns a non-2xx status.
// Components catch this and show `error.message` to the user.
export class ApiRequestError extends Error {
  status: number;

  constructor(status: number, message: string) {
    super(message);
    this.status = status;
    this.name = 'ApiRequestError';
  }
}

// Small helper: sends a request, parses JSON, and throws ApiRequestError on failure.
async function handle<T>(response: Response): Promise<T> {
  let body: unknown = null;
  try {
    body = await response.json();
  } catch {
    // Some responses (like 500s without a body) may not have JSON.
  }

  if (!response.ok) {
    const message =
      body && typeof body === 'object' && 'message' in body
        ? String((body as ApiError).message)
        : `Request failed with status ${response.status}`;
    throw new ApiRequestError(response.status, message);
  }

  return body as T;
}

// GET /api/participants
export async function getParticipants(): Promise<Participant[]> {
  const res = await fetch(`${API_BASE}/participants`);
  return handle<Participant[]>(res);
}

// GET /api/rewards
export async function getRewards(): Promise<Reward[]> {
  const res = await fetch(`${API_BASE}/rewards`);
  return handle<Reward[]>(res);
}

// GET /api/redemptions — optional status and countryCode filters
export async function getRedemptions(
  status?: string,
  countryCode?: string,
): Promise<Redemption[]> {
  const params = new URLSearchParams();
  if (status && status !== 'All') params.set('status', status);
  if (countryCode) params.set('countryCode', countryCode);

  const query = params.toString();
  const url = query ? `${API_BASE}/redemptions?${query}` : `${API_BASE}/redemptions`;

  const res = await fetch(url);
  return handle<Redemption[]>(res);
}

// POST /api/redemptions
export async function createRedemption(
  payload: CreateRedemptionRequest,
): Promise<RedemptionCreated> {
  const res = await fetch(`${API_BASE}/redemptions`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  });
  return handle<RedemptionCreated>(res);
}