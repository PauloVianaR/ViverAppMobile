using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViverApp.Shared.Models;

namespace ViverApp.Shared.DTos
{
    public class DoctorPropDto
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

        public DoctorPropDto() { }

        public DoctorPropDto(DoctorProp? prop)
        {
            if(prop is not null)
            {
                Iddoctorprops = prop.Iddoctorprops;
                Iddoctor = prop.Iddoctor;
                Title = prop.Title;
                Crm = prop.Crm;
                Mainspecialty = prop.Mainspecialty;
                Medicalexperience = prop.Medicalexperience;
                Rating = prop.Rating;
                Attendonline = prop.Attendonline;
                Maxonlinedayconsultation = prop.Maxonlinedayconsultation;
                Maxpresencialdayconsultation = prop.Maxpresencialdayconsultation;
            }
        }
    }
}
