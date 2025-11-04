using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViverApp.Shared.Models;

namespace ViverApp.Shared.DTos
{
    public class PaymentHistoricDto
    {
        public int Idpayment { get; set; }
        public int Idpaymenttype { get; set; }
        public string? PaymentDescription { get; set; }
        public string? AppointmentTitle { get; set; }
        public string? AppointmentDescription { get; set; }
        public string? AppointmentTypeDescription { get; set; }
        public DateTime? ScheduleDate { get; set; }
        public DateTime? Paidday { get; set; }
        public decimal? Paidprice { get; set; }
        public sbyte Paidonline { get; set; }
        public string? ProfessionalDoctorName { get; set; }
        public sbyte? IsUserPremium { get; set; }
    }
}
