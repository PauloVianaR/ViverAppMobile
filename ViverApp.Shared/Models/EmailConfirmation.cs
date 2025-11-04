using System;
using System.Collections.Generic;

namespace ViverApp.Shared.Models;

public partial class EmailConfirmation
{
    public int Idemailconfirmation { get; set; }

    public int Idemail { get; set; }

    public int Confirmationcode { get; set; }

    public DateTime Expiresat { get; set; }

    public virtual EmailQueue IdemailNavigation { get; set; } = null!;
}
