using System;
using System.Collections.Generic;

namespace ViverApp.Shared.Models;

public partial class Schedule
{
    public int Idschedule { get; set; }

    public int Idappointment { get; set; }

    public int Iduser { get; set; }

    public int Iddoctor { get; set; }

    public int Idclinic { get; set; }

    public int Status { get; set; }

    public DateTime? Appointmentdate { get; set; }

    public string? Obs { get; set; }

    public sbyte Isonline { get; set; }

    public sbyte? Callconcluded { get; set; }

    public sbyte Rescheduled { get; set; }

    public DateTime? Originaldate { get; set; }

    public sbyte Pendingpayment { get; set; }

    public float? Rating { get; set; }

    public string? Medicalreport { get; set; }

    public string? Feedback { get; set; }

    public DateTime Createdat { get; set; }

    public virtual Appointment IdappointmentNavigation { get; set; } = null!;

    public virtual Clinic IdclinicNavigation { get; set; } = null!;

    public virtual User IddoctorNavigation { get; set; } = null!;

    public virtual User IduserNavigation { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<ScheduleAttachment> ScheduleAttachments { get; set; } = new List<ScheduleAttachment>();
}
