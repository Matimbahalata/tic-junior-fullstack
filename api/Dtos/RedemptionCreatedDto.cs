namespace api.Dtos;

public class RedemptionCreatedDto
{
    public int RedemptionId { get; set; }
    public int ParticipantId { get; set; }
    public int RewardId { get; set; }
    public string Status { get; set; } = null!;
    public DateTime RequestedAt { get; set; }
    public int RemainingPointsBalance { get; set; }
}