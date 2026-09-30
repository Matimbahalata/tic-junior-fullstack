-- TIC Junior Take-Home — schema + seed data (SQL Server)
CREATE TABLE Participants (
  ParticipantId INT PRIMARY KEY,
  FullName NVARCHAR(200) NOT NULL,
  Email NVARCHAR(200) NOT NULL,
  CountryCode CHAR(2) NOT NULL,
  PointsBalance INT NOT NULL,
  IsActive BIT NOT NULL
);

CREATE TABLE Rewards (
  RewardId INT PRIMARY KEY,
  Name NVARCHAR(200) NOT NULL,
  PointsCost INT NOT NULL,
  IsActive BIT NOT NULL
);

CREATE TABLE Redemptions (
  RedemptionId INT IDENTITY(100,1) PRIMARY KEY,
  ParticipantId INT NOT NULL REFERENCES Participants(ParticipantId),
  RewardId INT NOT NULL REFERENCES Rewards(RewardId),
  Status NVARCHAR(20) NOT NULL,           -- Pending | Completed | Failed
  RequestedAt DATETIME2 NOT NULL,
  CompletedAt DATETIME2 NULL,
  FailureReason NVARCHAR(500) NULL
);

INSERT INTO Participants VALUES
 (1, N'Amina Jacobs',   N'amina@example.com',  'ZA', 2000, 1),
 (2, N'Timo Negonga',   N'timo@example.com',   'NA',  600, 1),
 (3, N'Sam Greene',     N'sam@example.com',    'GB', 1500, 0),
 (4, N'Lerato Mokoena', N'lerato@example.com', 'ZA',  100, 1);

INSERT INTO Rewards VALUES
 (10, N'Airtime R50',     500, 1),
 (11, N'Voucher R100',   1000, 1),
 (12, N'Legacy Gift',     200, 0),
 (13, N'Data Bundle 1GB', 300, 1);

SET IDENTITY_INSERT Redemptions ON;
INSERT INTO Redemptions (RedemptionId, ParticipantId, RewardId, Status, RequestedAt, CompletedAt, FailureReason) VALUES
 (100, 1, 10, N'Completed', '2026-09-01T09:00:00', '2026-09-01T09:05:00', NULL),
 (101, 2, 11, N'Pending',   '2026-09-08T14:00:00', NULL, NULL),
 (102, 2, 10, N'Failed',    '2026-09-07T11:00:00', NULL, N'Provider timeout'),
 (103, 1, 11, N'Completed', '2026-09-09T08:00:00', '2026-09-09T08:02:00', NULL);
SET IDENTITY_INSERT Redemptions OFF;
