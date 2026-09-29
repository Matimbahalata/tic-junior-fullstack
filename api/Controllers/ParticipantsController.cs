using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Data;
using api.Dtos;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ParticipantsController : ControllerBase
{
    private readonly RewardsDBContext _db;

    public ParticipantsController(RewardsDBContext db)
    {
        _db = db;
    }

    // GET /api/participants
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ParticipantDto>>> GetAll()
    {
        var participants = await _db.Participants
            .OrderBy(p => p.FullName)
            .Select(p => new ParticipantDto
            {
                ParticipantId = p.ParticipantId,
                FullName = p.FullName,
                CountryCode = p.CountryCode,
                PointsBalance = p.PointsBalance,
                IsActive = p.IsActive,
            })
            .ToListAsync();

        return Ok(participants);
    }
}