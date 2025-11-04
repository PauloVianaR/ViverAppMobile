using System;
using System.Collections.Generic;

namespace ViverApp.Shared.Models;

public partial class ScheduleAttachment
{
    public int Idscheduleattachments { get; set; }

    public int Idschedule { get; set; }

    public string Filepath { get; set; } = null!;

    public string Filename { get; set; } = null!;

    public float? Size { get; set; }

    public virtual Schedule IdscheduleNavigation { get; set; } = null!;
}
