using System;
using System.Collections.Generic;

namespace ViverApp.Shared.Models;

public partial class Payment
{
    public int Idpayment { get; set; }

    public int Idpaymenttype { get; set; }

    public int Idschedule { get; set; }

    public DateTime? Paidday { get; set; }

    public decimal? Paidprice { get; set; }

    public sbyte Paidonline { get; set; }

    public string? Cardlast4 { get; set; }

    public string? Cardauthorization { get; set; }

    public virtual PaymentType IdpaymenttypeNavigation { get; set; } = null!;

    public virtual Schedule IdscheduleNavigation { get; set; } = null!;
}
