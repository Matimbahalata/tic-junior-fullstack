# TIC Take-Home — Junior Full-Stack

A small rewards platform where participants earn points and redeem rewards.  
Each redemption has a status of `Pending`, `Completed`, or `Failed`.

## Project structure

```text
.
├── sql/
│   ├── sample-data.sql      Schema + seed data
│   └── queries.sql          Part 1 queries + index
├── api/                     ASP.NET Core Web API (.NET 9 / EF Core)
├── api.tests/               xUnit integration tests
├── web/                     Vue 3 + TypeScript frontend (Vite)
├── NOTES.md                 Notes, decisions, tools, limitations
├── TESTING.md               Manual test cases for POST /api/redemptions
├── SUPPORT.md               Timo support ticket write-up
└── README.md                This file
```

## Prerequisites

You'll need:

- .NET 9 SDK (8 works too)
- Node.js 18+ with npm
- SQL Server — any of these:
  - SQL Server Express + SSMS
  - LocalDB (comes with Visual Studio)
  - SQL Server in Docker + Azure Data Studio
- Git

## Database setup

### 1. Create the database

Open SSMS or Azure Data Studio and connect to your SQL Server.

Create the database:

```sql
CREATE DATABASE RewardsDB;
```

### 2. Load the schema and seed data

Open `sql/sample-data.sql` in SSMS.

Make sure the database dropdown at the top says `RewardsDB`, then press F5 to run it.

Verify:

```sql
SELECT * FROM Participants;
SELECT * FROM Rewards;
SELECT * FROM Redemptions;
```

You should see:

- 4 participants
- 4 rewards
- 4 redemptions

### 3. Run the Part 1 queries and add the index

Open `sql/queries.sql` and run it.

It contains the five queries from Part 1, plus this index:

```sql
CREATE INDEX IX_Redemptions_ParticipantId_Status
    ON Redemptions (ParticipantId, Status);
```

I added the index because the POST endpoint checks for existing `Pending` redemptions by participant and reward (Rule 6).

Queries 3 and 5 also join on `ParticipantId` and filter or group by `Status`.

`ParticipantId` comes first because it is more selective than `Status` in these queries.

## Connection string

The API reads its connection string from the `DefaultConnection` key.

Three files are involved:

- `api/appsettings.json` — committed, contains a placeholder
- `api/appsettings.Development.json` — not committed because it is gitignored; contains the real local value
- `api/appsettings.Development.json.example` — template to copy from

Copy the example:

```bash
cp api/appsettings.Development.json.example api/appsettings.Development.json
```

Then open:

```text
api/appsettings.Development.json
```

and replace the placeholder with your own connection string.

### LocalDB

```text
Server=(localdb)\MSSQLLocalDB;Database=RewardsDB;Trusted_Connection=True;TrustServerCertificate=True;
```

### SQL Server Express with SQL authentication

```text
Server=YOUR_MACHINE\SQLEXPRESS;Database=RewardsDB;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;
```

Replace `YOUR_MACHINE` and the password with your own values.

Keep this file out of Git — that is the purpose of the `.example` file.

## Running the API

```bash
cd api
dotnet restore
dotnet run
```

The API prints something similar to:

```text
Now listening on: https://localhost:7213
Now listening on: http://localhost:5213
```

The examples below use port `5213` for the HTTP API.

If your API runs on a different port, substitute the correct port.

## Endpoints

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/participants` | List participants |
| GET | `/api/rewards` | List rewards |
| GET | `/api/redemptions` | List redemptions |
| GET | `/api/redemptions?status=Pending&countryCode=NA` | Filter redemptions |
| POST | `/api/redemptions` | Create a redemption |

### POST `/api/redemptions`

Request body:

```json
{
  "participantId": 2,
  "rewardId": 13
}
```

The POST endpoint applies 8 rules in a specific order.

The full list of rules and manual test cases is documented in `TESTING.md`.

## Trying it with curl

If you don't have Postman available, you can use `curl`.

### List participants

```bash
curl http://localhost:5213/api/participants
```

### List pending redemptions in Namibia

```bash
curl "http://localhost:5213/api/redemptions?status=Pending&countryCode=NA"
```

### Create a redemption

```bash
curl -X POST http://localhost:5213/api/redemptions \
  -H "Content-Type: application/json" \
  -d '{"participantId":2,"rewardId":13}'
```

## Example responses

### GET `/api/participants`

```json
[
  {
    "participantId": 1,
    "fullName": "Amina Jacobs",
    "countryCode": "ZA",
    "pointsBalance": 2000,
    "isActive": true
  },
  {
    "participantId": 4,
    "fullName": "Lerato Mokoena",
    "countryCode": "ZA",
    "pointsBalance": 100,
    "isActive": true
  },
  {
    "participantId": 3,
    "fullName": "Sam Greene",
    "countryCode": "GB",
    "pointsBalance": 1500,
    "isActive": false
  },
  {
    "participantId": 2,
    "fullName": "Timo Negonga",
    "countryCode": "NA",
    "pointsBalance": 600,
    "isActive": true
  }
]
```

### Successful POST

A successful POST returns HTTP `201 Created`:

```json
{
  "redemptionId": 104,
  "participantId": 2,
  "rewardId": 13,
  "status": "Pending",
  "requestedAt": "2026-09-30T04:04:06.0810962Z",
  "remainingPointsBalance": 300
}
```

### Error response

```json
{
  "message": "You do not have enough points. This reward costs 1000 points and your balance is 100."
}
```

The API never returns stack traces or internal exception details to the client. Those are logged server-side instead.

## Running the tests

```bash
cd api.tests
dotnet test
```

Expected result:

```text
Passed!  - Failed: 0, Passed: 6, Skipped: 0, Total: 6
```

## What the tests cover

There are 6 tests in `RedemptionsPostTests.cs`.

They use `WebApplicationFactory` to spin up the real API in memory, backed by an EF Core in-memory database seeded with the same shape of data as `sample-data.sql`.

| Test | Scenario | Expected |
|---|---|---:|
| `Create_WithValidRequest_Returns201` | Valid redemption | 201 |
| `Create_WithInsufficientPoints_Returns400` | Rule 7 | 400 |
| `Create_WithExistingPendingRedemption_Returns409` | Rule 6 | 409 |
| `Create_WithInactiveParticipant_Returns400` | Rule 3 | 400 |
| `Create_WithInactiveReward_Returns400` | Rule 5 | 400 |

### Transaction note

The POST endpoint uses `BeginTransactionAsync` so the redemption insert and points deduction happen together or not at all.

SQL Server supports this normally.

The EF Core in-memory provider does not support database transactions in the same way, so the test factory suppresses `TransactionIgnoredWarning`.

The rule-failure tests never reach the transaction.

## Running the frontend

In a new terminal:

```bash
cd web
npm install
npm run dev
```

The frontend starts at:

```text
http://localhost:5173
```

## What the frontend does

The frontend is a single-page Vue 3 + TypeScript application built with Vite.

It provides:

- A redemptions table
- Status filter:
  - All
  - Pending
  - Completed
  - Failed
- Country filter
- Loading, error, and empty states
- A form to create a new redemption
- Participant and reward dropdowns
- The selected participant's current points balance
- Clear display of API error messages
- Inactive rewards are shown but disabled in the reward dropdown

## API base URL

The frontend API URL is configured in:

```text
web/src/api.ts
```

If your API runs on a different port, update the URL there.

## CORS

The API allows requests from:

```text
http://localhost:5173
```

This is the default Vite development server URL.

If the frontend runs on a different port, update the CORS policy in:

```text
api/Program.cs
```

# Troubleshooting

## `dotnet ef` command not found

Install the Entity Framework CLI tool:

```bash
dotnet tool install --global dotnet-ef --version 9.*
```

## HTTPS certificate warning in the browser

Trust the .NET development certificate:

```bash
dotnet dev-certs https --trust
```

## "Login failed for user" in SSMS

Double-check:

- SQL Server instance name
- Username
- Password
- `DefaultConnection` in `appsettings.Development.json`

## API says it can't resolve `RewardsDBContext`

Check that:

1. `appsettings.Development.json` exists.
2. The connection string key is named `DefaultConnection`.
3. `Program.cs` calls `AddDbContext<RewardsDBContext>`.
4. The connection string points to the correct SQL Server instance.

## CORS error in the browser console

Make sure the frontend URL matches the origins allowed in `Program.cs`.

The default is:

```text
http://localhost:5173
```

## `npm install` timing out or hitting permission errors

Try:

```bash
npm install --fetch-timeout=600000 --fetch-retries=5
```

Corporate proxies and antivirus software can interfere with npm.

A different network, such as a personal Wi-Fi connection or hotspot, may resolve the issue.

# What's implemented

- All 5 SQL queries plus the index
- 4 API endpoints (participants, rewards, redemptions GET + POST)
- All 8 POST rules in the order from the brief
- Atomic transaction around the create and point deduction
- 6 xUnit tests
- 12 manual test cases in `TESTING.md`
- Support ticket write-up in `SUPPORT.md`
- Vue 3 + TypeScript frontend with table, filters, create form, and error handling

# Things I'd add with more time

## Concurrency on Rule 6

Two POST requests fired at exactly the same time for the same participant and reward could both pass the duplicate `Pending` check before either transaction commits.

A unique filtered index on:

```text
(ParticipantId, RewardId)
```

with:

```text
WHERE Status = 'Pending'
```

would prevent this at the database level.

## PATCH endpoint

The optional stretch endpoint for moving a `Pending` redemption to `Completed` or `Failed` is not implemented.

## Frontend Complete/Fail buttons

The frontend could be extended with Complete and Fail buttons using the PATCH endpoint described above.

More detail about the implementation, including time spent, tools used, decisions, and things that were difficult, is available in `NOTES.md`.

## Before saving

### Check the API port

The current API HTTP port is:

```text
5213
```

If the API is running on a different port, replace `5213` throughout this README.

The HTTPS development port is:

```text
7213
```

## Commit and push

```bash
cd ~/tic-junior-fullstack
git add README.md sql/
git commit -m "README: full setup instructions; add sample-data.sql"
git push
```