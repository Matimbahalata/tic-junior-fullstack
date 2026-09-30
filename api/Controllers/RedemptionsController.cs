using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Data;
using api.Dtos;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RedemptionsController : ControllerBase
{
    private readonly RewardsDBContext _db;

    public RedemptionsController(RewardsDBContext db)
    {
        _db = db;
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
}