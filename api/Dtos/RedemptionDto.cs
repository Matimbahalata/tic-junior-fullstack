namespace api.Dtos;

public class RedemptionDto
{
    public int RedemptionId { get; set; }
    public string ParticipantName { get; set; } = null!;
    public string CountryCode { get; set; } = null!;
    public string RewardName { get; set; } = null!;
    public int PointsCost { get; set; }
    public string Status { get; set; } = null!;
    public DateTime RequestedAt { get; set; }
}