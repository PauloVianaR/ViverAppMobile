using System;
using System.Collections.Generic;

namespace ViverApp.Shared.Models;

public partial class Notification
{
    public int Idnotification { get; set; }

    public int Notificationtype { get; set; }

    public int Severity { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Pushdescription { get; set; }

    public DateTime Createdat { get; set; }

    public sbyte Read { get; set; }

    public sbyte Sent { get; set; }

    public int? Targetid { get; set; }
}
