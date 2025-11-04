using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViverApp.Shared.Models;

namespace ViverApp.Shared.DTos
{
    public class PaymentCreateDto
    {
        public int Idpaymenttype { get; set; }

        public int? Idusercard { get; set; }

        public int Idschedule { get; set; }

        public DateTime? Paidday { get; set; }

        public decimal? Paidprice { get; set; }

        public sbyte Paidonline { get; set; }

        public string? Cardlast4 { get; set; }

        public string? Cardauthorization { get; set; }

        public PaymentCreateDto() { }

        public PaymentCreateDto(Payment payment)
        {
            Idpaymenttype = payment.Idpaymenttype;
            Idschedule = payment.Idschedule;
            Paidday = payment.Paidday;
            Paidprice = payment.Paidprice;
            Paidonline = payment.Paidonline;
            Cardlast4 = payment.Cardlast4;
            Cardauthorization = payment.Cardauthorization;
        }
    }
}
