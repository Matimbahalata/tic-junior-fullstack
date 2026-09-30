using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Data;
using api.Dtos;
using api.Models;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RedemptionsController : ControllerBase
{
    private readonly RewardsDBContext _db;
    private readonly ILogger<RedemptionsController> _logger;

    public RedemptionsController(RewardsDBContext db, ILogger<RedemptionsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET /api/redemptions?status=Pending&countryCode=NA
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RedemptionDto>>> GetAll(
        [FromQuery] string? status,
        [FromQuery] string? countryCode)
    {
        var query = _db.Redemptions
            .Include(r => r.Participant)
            .Include(r => r.Reward)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(r => r.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(countryCode))
        {
            query = query.Where(r => r.Participant.CountryCode == countryCode);
        }

        var redemptions = await query
            .OrderByDescending(r => r.RequestedAt)
            .Select(r => new RedemptionDto
            {
                RedemptionId = r.RedemptionId,
                ParticipantName = r.Participant.FullName,
                CountryCode = r.Participant.CountryCode,
                RewardName = r.Reward.Name,
                PointsCost = r.Reward.PointsCost,
                Status = r.Status,
                RequestedAt = r.RequestedAt,
            })
            .ToListAsync();

        return Ok(redemptions);
    }

    // POST /api/redemptions
    [HttpPost]
    public async Task<ActionResult<RedemptionCreatedDto>> Create([FromBody] CreateRedemptionRequest request)
    {
        // Rule 1: IDs present and greater than 0
        if (request.ParticipantId <= 0 || request.RewardId <= 0)
        {
            return BadRequest(new { message = "participantId and rewardId must be present and greater than 0." });
        }

        // Rule 2: Participant must exist
        var participant = await _db.Participants.FindAsync(request.ParticipantId);
        if (participant == null)
        {
            return NotFound(new { message = $"Participant {request.ParticipantId} was not found." });
        }

        // Rule 3: Participant must be active
        if (!participant.IsActive)
        {
            return BadRequest(new { message = "This participant is not active and cannot redeem rewards." });
        }

        // Rule 4: Reward must exist
        var reward = await _db.Rewards.FindAsync(request.RewardId);
        if (reward == null)
        {
            return NotFound(new { message = $"Reward {request.RewardId} was not found." });
        }

        // Rule 5: Reward must be active
        if (!reward.IsActive)
        {
            return BadRequest(new { message = "This reward is not currently available." });
        }

        // Rule 6: No existing Pending redemption for same participant + reward
        var existingPending = await _db.Redemptions.AnyAsync(r =>
            r.ParticipantId == request.ParticipantId &&
            r.RewardId == request.RewardId &&
            r.Status == "Pending");

        if (existingPending)
        {
            return Conflict(new { message = "You already have a pending redemption for this reward. Please wait for it to be processed." });
        }

        // Rule 7: Enough points
        if (participant.PointsBalance < reward.PointsCost)
        {
            return BadRequest(new { message = $"You do not have enough points. This reward costs {reward.PointsCost} points and your balance is {participant.PointsBalance}." });
        }

        // Rule 8: Create + deduct atomically
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var redemption = new Redemption
            {
                ParticipantId = participant.ParticipantId,
                RewardId = reward.RewardId,
                Status = "Pending",
                RequestedAt = DateTime.UtcNow,
            };

            participant.PointsBalance -= reward.PointsCost;

            _db.Redemptions.Add(redemption);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            var response = new RedemptionCreatedDto
            {
                RedemptionId = redemption.RedemptionId,
                ParticipantId = redemption.ParticipantId,
                RewardId = redemption.RewardId,
                Status = redemption.Status,
                RequestedAt = redemption.RequestedAt,
                RemainingPointsBalance = participant.PointsBalance,
            };

            return CreatedAtAction(nameof(GetAll), new { id = redemption.RedemptionId }, response);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Failed to create redemption for participant {ParticipantId}, reward {RewardId}",
                request.ParticipantId, request.RewardId);
            return StatusCode(500, new { message = "An unexpected error occurred. Please try again." });
        }
    }
}