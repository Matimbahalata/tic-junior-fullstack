using System;
using System.Collections.Generic;

namespace api.Models;

public partial class Reward
{
    public int RewardId { get; set; }

    public string Name { get; set; } = null!;

    public int PointsCost { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Redemption> Redemptions { get; set; } = new List<Redemption>();
}
