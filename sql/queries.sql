--part 1 -All Pending redemptions with participant name, email, country, reward name and points cost.
Select
    r.RedemptionId,
    p.FullName,
    p.Email,
    p.CountryCode,
    rw.Name        AS RewardName,
    rw.PointsCost
From Redemptions r
join Participants p ON p.ParticipantId = r.ParticipantId
join Rewards      rw ON rw.RewardId      = r.RewardId
Where r.Status = 'Pending'

--The number of redemptions per country and status.

select
    p.CountryCode,
    r.Status,
    COUNT(r.RedemptionId) AS RedemptionCount
from Redemptions r
join Participants p ON p.ParticipantId = r.ParticipantId
group by p.CountryCode, r.Status
order by p.CountryCode, r.Status


--- Active participants who have no Completed redemption


select
    p.ParticipantId,
    p.FullName,
    p.CountryCode
from Participants p
where p.IsActive = 1
  and  not exists (
      select 1
      from Redemptions r
      where r.ParticipantId = p.ParticipantId
        and r.Status = 'Completed'
  )
order by p.ParticipantId;


-- Active participants who can currently afford the Voucher R100 (PointsCost 1000)

select
    p.ParticipantId,
    p.FullName,
    p.PointsBalance
from Participants p
join Rewards w ON w.Name = 'Voucher R100'
where p.IsActive = 1
  and p.PointsBalance >= w.PointsCost
order by p.ParticipantId;


-- Total points spent on Completed redemptions for every participant, including those with none (0).

select
    p.ParticipantId,
    p.FullName,
    ISNULL(SUM(w.PointsCost), 0) AS TotalPointsSpent
from Participants p
left join Redemptions r
       ON r.ParticipantId = p.ParticipantId
      AND r.Status = 'Completed'
left join Rewards w
       ON w.RewardId = r.RewardId
group by p.ParticipantId, p.FullName
order by p.ParticipantId;