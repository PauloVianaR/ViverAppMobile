using System;
using System.Collections.Generic;
using ViverApp.Shared.DTos;

namespace ViverApp.Shared.Models;

public partial class AvailabilityDoctor
{
    public int Idavailabilitydoctor { get; set; }

    public int Iddoctor { get; set; }

    public int? Daytype { get; set; }

    public TimeOnly? Starttime { get; set; }

    public TimeOnly? Endtime { get; set; }

    public sbyte Isonline { get; set; }

    public virtual User IddoctorNavigation { get; set; } = null!;

    public AvailabilityDoctor() { }

    public AvailabilityDoctor(AvailabilityDoctorDto availabilityDoctorDto)
    {
        Iddoctor = availabilityDoctorDto.Iddoctor;
        Daytype = availabilityDoctorDto.Daytype;
        Starttime = availabilityDoctorDto.Starttime;
        Endtime = availabilityDoctorDto.Endtime;
        Isonline = availabilityDoctorDto.Isonline;
    }

    public AvailabilityDoctor(int id,int iddoctor, int daytype, TimeOnly starttime, TimeOnly endtime, sbyte isonline)
    {
        Idavailabilitydoctor = id;
        Iddoctor = iddoctor;
        Daytype = daytype;
        Starttime = starttime;
        Endtime = endtime;
        Isonline = isonline;
    }
}
