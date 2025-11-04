using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViverApp.Shared.Models;

namespace ViverApp.Shared.DTos
{
    public class SpecialtyDoctorDto
    {
        public int? Iddoctor { get; set; }

        public int? Idappointment { get; set; }

        public SpecialtyDoctorDto() { }

        public SpecialtyDoctorDto(SpecialtysDoctor sd)
        {
            Iddoctor = sd.Iddoctor;
            Idappointment = sd.Idappointment;
        }
    }
}
