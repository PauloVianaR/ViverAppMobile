using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViverApp.Shared.DTos
{
    public class AvailabilityClinicDto
    {
        public int Idavailabilityclinic { get; set; }

        public int Idclinic { get; set; }

        public int? Daytype { get; set; }

        public TimeOnly? Starttime { get; set; }

        public TimeOnly? Endtime { get; set; }
    }
}
