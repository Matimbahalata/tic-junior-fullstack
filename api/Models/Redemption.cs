using System;
using System.Collections.Generic;

namespace api.Models;

public partial class Redemption
{
    public int RedemptionId { get; set; }

    public int ParticipantId { get; set; }

    public int RewardId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime RequestedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? FailureReason { get; set; }

    public virtual Participant Participant { get; set; } = null!;

    public virtual Reward Reward { get; set; } = null!;
}
