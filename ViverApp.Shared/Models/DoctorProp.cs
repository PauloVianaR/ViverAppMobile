using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using ViverApp.Shared.DTos;

namespace ViverApp.Shared.Models;

public partial class DoctorProp
{
    public int Iddoctorprops { get; set; }

    public int Iddoctor { get; set; }

    public string? Title { get; set; }

    public string? Crm { get; set; }

    public string? Mainspecialty { get; set; }

    public int? Medicalexperience { get; set; }

    public float? Rating { get; set; }

    public sbyte Attendonline { get; set; }

    public int Maxonlinedayconsultation { get; set; }

    public int Maxpresencialdayconsultation { get; set; }

    public virtual User IddoctorNavigation { get; set; } = null!;

    public DoctorProp() { }

    public DoctorProp(DoctorDto doctorDto)
    {
        Iddoctorprops = doctorDto.Iddoctorprops;
        Iddoctor = doctorDto.IdUser;
        Title = doctorDto.Title;
        Crm = doctorDto.Crm;
        Mainspecialty = doctorDto.Mainspecialty;
        Medicalexperience = doctorDto.Medicalexperience;
        Rating = doctorDto.Rating;
        Attendonline = doctorDto.Attendonline;
        Maxonlinedayconsultation = doctorDto.Maxonlinedayconsultation;
        Maxpresencialdayconsultation = doctorDto.Maxpresencialdayconsultation;
    }

    public DoctorProp(DoctorPropDto doctorPropDto)
    {
        Iddoctorprops = doctorPropDto.Iddoctorprops;
        Iddoctor = doctorPropDto.Iddoctor;
        Title = doctorPropDto.Title;
        Crm = doctorPropDto.Crm;
        Mainspecialty = doctorPropDto.Mainspecialty;
        Medicalexperience = doctorPropDto.Medicalexperience;
        Rating = doctorPropDto.Rating;
        Attendonline = doctorPropDto.Attendonline;
        Maxonlinedayconsultation = doctorPropDto.Maxonlinedayconsultation;
        Maxpresencialdayconsultation = doctorPropDto.Maxpresencialdayconsultation;
    }
}
