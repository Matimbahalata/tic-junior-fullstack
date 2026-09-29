using System;
using System.Collections.Generic;

namespace api.Models;

public partial class Participant
{
    public int ParticipantId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string CountryCode { get; set; } = null!;

    public int PointsBalance { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Redemption> Redemptions { get; set; } = new List<Redemption>();
}
