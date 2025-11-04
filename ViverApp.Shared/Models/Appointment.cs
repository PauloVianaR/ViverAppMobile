using System;
using System.Collections.Generic;

namespace ViverApp.Shared.Models;

public partial class Appointment
{
    public int Idappointment { get; set; }

    public int Idappointmenttype { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public TimeOnly? Averagetime { get; set; }

    public decimal? Price { get; set; }

    public sbyte Ispopular { get; set; }

    public sbyte Canonline { get; set; }

    public int Status { get; set; }

    public virtual AppointmentType IdappointmenttypeNavigation { get; set; } = null!;

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    public virtual ICollection<SpecialtysDoctor> SpecialtysDoctors { get; set; } = new List<SpecialtysDoctor>();
}
