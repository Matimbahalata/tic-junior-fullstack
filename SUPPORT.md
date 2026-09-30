 # Support ticket — Timo Negonga

> "Hi, Timo in Namibia says he's trying to redeem the R100 voucher again but the
> system keeps giving him an error. He's also asking why the voucher he requested
> on the 8th still isn't showing as completed. He's getting frustrated. Please
> sort this out ASAP."


 ## What I checked

I opened the database and looked up Timo. He's ParticipantId 2, based in NA, with
600 points and an active account.

Then I pulled his redemptions:

- 101 — Voucher R100 — Pending— requested 8 Sept
- 102 — Airtime R50 — Failed (provider timeout) — requested 7 Sept

The R100 voucher he's trying to redeem is reward 11. He already has redemption
101 in Pending status for that same reward. So when he submits a new request, our
POST /api/redemptions rules go through in order and stop at **Rule 6** — no
duplicate Pending redemptions for the same participant + reward. The API returns
409 Conflict with "You already have a pending redemption for this reward."

Rule 7 (enough points) never even gets checked. But it's worth noting that if it
did, Timo only has 600 points and the voucher costs 1000 — so that would also fail.

As for why the voucher from the 8th isn't Completed — it's still Pending. Nobody
has moved it forward. Our API doesn't auto-complete pending redemptions; someone
has to action it (or the stretch PATCH endpoint would need to exist).

## Reply to ops

Hi,

I've looked into Timo's account. Here's what's going on.

The error he's getting when he tries to redeem the voucher is actually the system
working as intended. He already has a voucher request from the 8th that's still
open, and we block duplicate requests so someone doesn't get charged twice by
mistake. So the error isn't a bug.

The reason the voucher from the 8th isn't showing as completed yet is that it
hasn't been processed — it's still sitting in a Pending state. That's the thing
to follow up on.

One other thing worth mentioning: Timo has 600 points at the moment and the
voucher costs 1000, so he wouldn't have enough points for a new request anyway
right now.

Next steps I'd suggest:
1. Get his existing request from the 8th actioned — that should sort out his
   immediate frustration.
2. Let him know he needs 1000 points and currently has 600.




## Escalation to senior dev

**Subject:** Pending redemptions don't seem to have a path to completion

Timo Negonga (ParticipantId 2) requested a Voucher R100 on 8 Sept (RedemptionId
101). It's still Pending. He's now blocked from re-requesting the same reward by
Rule 6 — which is correct behaviour — but he has no idea when or how his original
one will be actioned.

Looking at the codebase, I can't find anything that moves a Pending redemption to
Completed. The stretch PATCH endpoint would do it but isn't implemented in the
current build.

Question: is there supposed to be a background job or admin tool for this? If yes,
it doesn't seem to be running. If no, we should document the manual process so
support knows what to tell people.

Impact: any participant in this state will hit the same problem and generate repeat
tickets.