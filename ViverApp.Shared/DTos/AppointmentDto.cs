using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViverApp.Shared.DTos
{
    public class AppointmentDto
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
    }
}
