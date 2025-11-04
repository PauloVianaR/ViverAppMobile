using System;
using System.Collections.Generic;

namespace ViverApp.Shared.Models;

public partial class EmailQueue
{
    public int Idemail { get; set; }

    public string Sender { get; set; } = null!;

    public string Receiver { get; set; } = null!;

    public string Subject { get; set; } = null!;

    public string Body { get; set; } = null!;

    public int Severity { get; set; }

    public int Status { get; set; }

    public int Tries { get; set; }

    public DateTime Createdat { get; set; }

    public virtual EmailConfirmation? EmailConfirmation { get; set; }
}
