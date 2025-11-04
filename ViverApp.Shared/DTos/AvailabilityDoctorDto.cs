using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViverApp.Shared.Models;

namespace ViverApp.Shared.DTos
{
    public class AvailabilityDoctorDto
    {
        public int Idavailabilitydoctor { get; set; }

        public int Iddoctor { get; set; }

        public int? Daytype { get; set; }

        public TimeOnly? Starttime { get; set; }

        public TimeOnly? Endtime { get; set; }

        public sbyte Isonline { get; set; }

        public AvailabilityDoctorDto() { }

        public AvailabilityDoctorDto(AvailabilityDoctor availabilityDoctor)
        {
            Idavailabilitydoctor = availabilityDoctor.Idavailabilitydoctor;
            Iddoctor = availabilityDoctor.Iddoctor;
            Daytype = availabilityDoctor.Daytype;
            Starttime = availabilityDoctor.Starttime;
            Endtime = availabilityDoctor.Endtime;
            Isonline = availabilityDoctor.Isonline;
        }
    }
}
