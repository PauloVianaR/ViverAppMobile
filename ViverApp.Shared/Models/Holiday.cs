using System;
using System.Collections.Generic;

namespace ViverApp.Shared.Models;

public partial class Holiday
{
    public int Idholiday { get; set; }

    public string? Holidayname { get; set; }

    public DateOnly? Holidaydate { get; set; }

    public sbyte Canschedule { get; set; }
}
