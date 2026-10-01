# Test cases — POST /api/redemptions

These test cases exercise the eight business rules applied in order by POST /api/redemptions.

## Seed data reference

Participants:
- Amina Jacobs (id 1, ZA, 2000 pts, active)
- Timo Negonga (id 2, NA, 600 pts, active)
- Sam Greene (id 3, GB, 1500 pts, INACTIVE)
- Lerato Mokoena (id 4, ZA, 100 pts, active)

Rewards:
- Airtime R50 (id 10, 500 pts, active)
- Voucher R100 (id 11, 1000 pts, active)
- Legacy Gift (id 12, 200 pts, INACTIVE)
- Data Bundle 1GB (id 13, 300 pts, active)

Existing redemptions:
- 100: Amina → Airtime R50 → Completed
- 101: Timo → Voucher R100 → Pending
- 102: Timo → Airtime R50 → Failed
- 103: Amina → Voucher R100 → Completed

---

## TC-01 — Happy path

**Do:** POST a valid redemption for an active participant who can afford the reward and has no existing Pending redemption for that reward.

**Input:** participantId = 2, rewardId = 13  (Timo, Data Bundle 1GB, 300 pts — Timo has 600 and no Pending for reward 13)

**Expected:** HTTP 201 Created. Response includes a new redemptionId, status "Pending", requestedAt in UTC, and remainingPointsBalance = 300. In the DB, Timo's PointsBalance drops by 300, and a new Redemptions row exists with Status = 'Pending'.

**Why:** Confirms the happy path end-to-end, including atomic write and points deduction.

---

## TC-02 — Rule 1: IDs must be present and > 0

**Do:** POST with participantId = 0.

**Input:** participantId = 0, rewardId = 10

**Expected:** HTTP 400 Bad Request. Message: "participantId and rewardId must be present and greater than 0." No DB changes.

---

## TC-03 — Rule 2: Participant must exist

**Do:** POST with a participantId that does not exist.

**Input:** participantId = 999, rewardId = 10

**Expected:** HTTP 404 Not Found. Message: "Participant 999 was not found." No DB changes.

---

## TC-04 — Rule 3: Participant must be active

**Do:** POST for Sam Greene (id 3, IsActive = false).

**Input:** participantId = 3, rewardId = 10

**Expected:** HTTP 400 Bad Request. Message: "This participant is not active and cannot redeem rewards." No DB changes.

---

## TC-05 — Rule 4: Reward must exist

**Do:** POST with a rewardId that does not exist.

**Input:** participantId = 1, rewardId = 999

**Expected:** HTTP 404 Not Found. Message: "Reward 999 was not found." No DB changes.

---

## TC-06 — Rule 5: Reward must be active

**Do:** POST for the inactive Legacy Gift (id 12).

**Input:** participantId = 1, rewardId = 12

**Expected:** HTTP 400 Bad Request. Message: "This reward is not currently available." No DB changes.

---

## TC-07 — Rule 6: No duplicate Pending for same participant + reward

**Do:** POST for Timo (id 2) redeeming Voucher R100 (id 11). Timo already has redemption 101 in Pending for the same reward.

**Input:** participantId = 2, rewardId = 11

**Expected:** HTTP 409 Conflict. Message: "You already have a pending redemption for this reward. Please wait for it to be processed." No DB changes.

**Why 409 not 400:** The request is valid, but conflicts with existing state. 409 is the correct semantic.

---

## TC-08 — Rule 7: Participant must have enough points

**Do:** POST for Lerato (id 4, 100 pts) redeeming Voucher R100 (id 11, costs 1000).

**Input:** participantId = 4, rewardId = 11

**Expected:** HTTP 400 Bad Request. Message: "You do not have enough points. This reward costs 1000 points and your balance is 100." No DB changes.

---

## TC-09 — Edge case: exactly enough points (boundary)

**Do:** POST a redemption where PointsBalance equals PointsCost exactly. The rule uses >= so this must succeed.

**Setup:** Set Timo's balance to exactly 300: UPDATE Participants SET PointsBalance = 300 WHERE ParticipantId = 2;

**Input:** participantId = 2, rewardId = 13

**Expected:** HTTP 201 Created with remainingPointsBalance = 0.

**Why:** Proves the boundary >= is inclusive (off-by-one bugs are common here).

**Teardown:** UPDATE Participants SET PointsBalance = 600 WHERE ParticipantId = 2; DELETE FROM Redemptions WHERE ParticipantId = 2 AND RewardId = 13;

---

## TC-10 — Edge case: rule order matters

**Do:** POST a request that violates two rules at once — an inactive participant with insufficient points. Rule 3 should fire before Rule 7.

**Setup:** UPDATE Participants SET PointsBalance = 50 WHERE ParticipantId = 3;

**Input:** participantId = 3, rewardId = 11  (Sam is inactive AND has 50 pts < 1000 cost)

**Expected:** HTTP 400 Bad Request. Message: "This participant is not active and cannot redeem rewards." NOT the "not enough points" message — proving Rule 3 fires first.

**Teardown:** UPDATE Participants SET PointsBalance = 1500 WHERE ParticipantId = 3;

---

## TC-11 — Edge case: different rewards are independent

**Do:** Timo (id 2) already has a Pending redemption for Voucher R100 (id 11). Confirm he can still redeem Data Bundle 1GB (id 13) — a different reward.

**Input:** participantId = 2, rewardId = 13

**Expected:** HTTP 201 Created. Rule 6 only blocks duplicates for the same reward, not any redemption.

**Why:** Confirms Rule 6's exact semantics — "same participant AND same reward AND Pending". A looser interpretation would incorrectly reject this.

**Teardown:** UPDATE Participants SET PointsBalance = 600 WHERE ParticipantId = 2; DELETE FROM Redemptions WHERE ParticipantId = 2 AND RewardId = 13;

---

## TC-12 — Edge case: missing request body

**Do:** POST with an empty body.

**Input:** (nothing)

**Expected:** HTTP 400 Bad Request (returned automatically by ApiController model binding).

**Why:** Confirms the API doesn't crash on malformed input.

---

## Summary

| Test | Covers | Expected |
|------|--------|----------|
| TC-01 | Happy path | 201 |
| TC-02 | Rule 1 — IDs > 0 | 400 |
| TC-03 | Rule 2 — Participant exists | 404 |
| TC-04 | Rule 3 — Participant active | 400 |
| TC-05 | Rule 4 — Reward exists | 404 |
| TC-06 | Rule 5 — Reward active | 400 |
| TC-07 | Rule 6 — No duplicate Pending | 409 |
| TC-08 | Rule 7 — Enough points | 400 |
| TC-09 | Edge — boundary (>=) | 201 |
| TC-10 | Edge — rule order | 400 (inactive msg) |
| TC-11 | Edge — independent rewards | 201 |
| TC-12 | Edge — missing body | 400 |

All cases assume the seed data from sample-data.sql is loaded.