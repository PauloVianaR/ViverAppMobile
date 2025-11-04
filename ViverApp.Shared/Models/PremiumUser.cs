using System;
using System.Collections.Generic;

namespace ViverApp.Shared.Models;

public partial class PremiumUser
{
    public int IdpremiumUser { get; set; }

    public int Iduser { get; set; }

    public int Idpremiumplan { get; set; }

    public DateTime? Premiumdate { get; set; }

    public sbyte Intestperiod { get; set; }

    public virtual PremiumPlan IdpremiumplanNavigation { get; set; } = null!;

    public virtual User IduserNavigation { get; set; } = null!;
}
