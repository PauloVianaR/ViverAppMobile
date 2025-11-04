using System;
using System.Collections.Generic;

namespace ViverApp.Shared.Models;

public partial class SpecialtysDoctor
{
    public int Idspecialtysdoctor { get; set; }

    public int? Iddoctor { get; set; }

    public int? Idappointment { get; set; }

    public virtual Appointment? IdappointmentNavigation { get; set; }

    public virtual User? IddoctorNavigation { get; set; }

    public SpecialtysDoctor() { }

    public SpecialtysDoctor(int iddoctor, int idappnt)
    {
        Iddoctor = iddoctor;
        Idappointment = idappnt;
    }
}
