using System;
using System.Collections.Generic;

namespace ViverApp.Shared.Models;

public partial class PremiumPlan
{
    public int Idpremiumplan { get; set; }

    public decimal? Price { get; set; }

    public int? Plantype { get; set; }

    public string? Testperiod { get; set; }

    public virtual ICollection<PremiumUser> PremiumUsers { get; set; } = new List<PremiumUser>();
}
