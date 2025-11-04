using System;
using System.Collections.Generic;

namespace ViverApp.Shared.Models;

public partial class AvailabilityClinic
{
    public int Idavailabilityclinic { get; set; }

    public int Idclinic { get; set; }

    public int? Daytype { get; set; }

    public TimeOnly? Starttime { get; set; }

    public TimeOnly? Endtime { get; set; }

    public virtual Clinic IdclinicNavigation { get; set; } = null!;
}
