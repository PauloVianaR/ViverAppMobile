using System;
using System.Collections.Generic;

namespace ViverApp.Shared.Models;

public partial class AppointmentType
{
    public int Idappointmenttype { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
