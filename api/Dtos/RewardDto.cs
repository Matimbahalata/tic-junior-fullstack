namespace api.Dtos;

public class RewardDto
{
    public int RewardId { get; set; }
    public string Name { get; set; } = null!;
    public int PointsCost { get; set; }
    public bool IsActive { get; set; }
}