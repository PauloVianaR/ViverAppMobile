using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViverApp.Shared.DTos
{
    public class ScheduleCreateDto
    {
        public int Idappointment { get; set; }
        public int Iduser { get; set; }
        public int Iddoctor { get; set; }
        public int Idclinic { get; set; }
        public int Status { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public string? Obs { get; set; }
        public sbyte Rescheduled { get; set; }
        public sbyte IsOnline { get; set; }
        public DateTime? OriginalDate { get; set; }
        public sbyte PendingPayment { get; set; }
    }
}
