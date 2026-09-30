# Notes

## Time spent

- Environment setup: ~3–4 hours across a couple of evenings (npm kept failing on the work network, and I hit an EF Core version mismatch early on)
- Part 1 — SQL: ~45 min
- Part 2 — Backend: ~2 hours
- Part 3 — Frontend: ~1.5 hours
- Part 4a — TESTING.md: ~20 min
- Part 4b — Automated tests: ~40 min
- Part 4c — SUPPORT.md: ~20 min
- README + NOTES: ~30 min

Roughly 6 hours total on the assessment itself.

## Tools used

- VS Code
- SSMS
- Postman
- Git Bash
- DeepSeek  and ChatGPT as a coding assistant,

What I used DeepSeek for:

- Setup guidance (.NET/EF Core versions, connection strings, why certain commands were failing)
- Drafting the SQL queries and talking through NOT EXISTS vs NOT IN for query 3
- Drafting controller skeletons and the rule order for POST /api/redemptions
- Initial shapes for TESTING.md and SUPPORT.md
- Looking up errors (Swagger mismatch, EF in-memory transaction warning, CORS)

I ran and tested everything myself against the seed data. The API responses in TESTING.md are from real requests I sent with Postman.

## Assumptions

- Column names in sample-data.sql use `Id` rather than `ID`. I used those names everywhere.
- RequestedAt is stored in UTC. The POST endpoint uses DateTime.UtcNow.
- Status only ever contains Pending, Completed, or Failed. No check constraint (brief didn't ask for one).
- Query 5 includes inactive participants — the brief said "every participant" so I took it literally. If it should only be active ones, add WHERE p.IsActive = 1.

## Decisions

**EF Core: database-first.** The brief allowed either. I already had sample-data.sql loaded, so scaffolding the entities from the existing tables was the shortest path. No risk of migrations drifting from the actual DB.

**Index:** I added

    CREATE INDEX IX_Redemptions_ParticipantId_Status
        ON Redemptions (ParticipantId, Status);

Two reasons. First, the POST endpoint checks for existing Pending redemptions by participant + reward (Rule 6), and this index covers the ParticipantId + Status part of that lookup. Second, queries 3 and 5 both join redemptions by participant and filter/group by status. ParticipantId goes first because it's more selective than Status in these queries.

**Removed Swagger.** The default Swashbuckle package pulled in a Microsoft.OpenApi version that wasn't compatible with .NET 9 in this environment — startup was throwing TypeLoadExceptions. Swagger isn't required in the brief, so I removed it and tested with Postman. Endpoints work the same.

**Connection strings.** Real connection string (with the password) lives in appsettings.Development.json, which is gitignored. appsettings.json has a placeholder, and there's an .example template for reviewers.

**POST /api/redemptions.** Rules run in the order from the brief and stop at the first failure. Create + points deduction are wrapped in a transaction. EF Core would give atomicity for these two writes with a single SaveChangesAsync anyway, but the explicit transaction makes the intent clear and covers any additional writes if I add them later. Errors return 400/404/409/500 with a JSON message. 500s log the full exception server-side and return a generic message to the client.

## Not finished / would improve with time

- **Concurrency on Rule 6.** Two POSTs for the same participant + reward fired close together could both pass the duplicate-Pending check before either commits. A unique filtered index on (ParticipantId, RewardId) WHERE Status = 'Pending' would fix this at the DB level. I noticed it and left it as a future improvement.
- **Stretch endpoint.** PATCH /api/redemptions/{id}/status isn't implemented. Optional in the brief, so I left it out. First thing I'd add.
- **Frontend stretch.** The Complete/Fail buttons on pending redemptions aren't there. Inactive rewards are already disabled in the dropdown.
- **Swagger.** Would be nice with a compatible package version. Not required.
- **Validation attributes.** I don't have [Range] or [Required] on the request DTO. Rule 1 handles the "greater than 0" case manually, which works, but attributes would be cleaner.

## Difficult / unsure

- npm install failed several times on the work network — ERR_SOCKET_TIMEOUT, then EPERM / spawnSync errors. Looked like antivirus plus network restrictions. Worked on a later retry.
- The Swagger package mismatch was tricky to trace because it threw at startup, not compile time.
- The in-memory EF provider doesn't support transactions, so the happy-path test initially returned 500. Suppressed the TransactionIgnoredWarning in the test factory. The rule-failure tests passed all along because they bail out before the transaction.
- Query 3 — my first version used a JOIN and a != check, which gave the wrong answer (duplicated rows and missing participants with no redemptions). NOT EXISTS was correct and much clearer.
- Test cases: I picked TC-10 (rule order — inactive participant with insufficient points gets the "inactive" error first) and TC-11 (Rule 6 only blocks the same reward, not any reward) because they test the semantics of the rules instead of just the happy path.