using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Data;
using api.Dtos;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RewardsController : ControllerBase
{
    private readonly RewardsDBContext _db;

    public RewardsController(RewardsDBContext db)
    {
        _db = db;
    }

    // GET /api/rewards
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RewardDto>>> GetAll()
    {
        var rewards = await _db.Rewards
            .OrderBy(r => r.RewardId)
            .Select(r => new RewardDto
            {
                RewardId = r.RewardId,
                Name = r.Name,
                PointsCost = r.PointsCost,
                IsActive = r.IsActive,
            })
            .ToListAsync();

        return Ok(rewards);
    }
}