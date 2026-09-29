namespace api.Dtos;

public class ParticipantDto
{
    public int ParticipantId { get; set; }
    public string FullName { get; set; } = null!;
    public string CountryCode { get; set; } = null!;
    public int PointsBalance { get; set; }
    public bool IsActive { get; set; }
}