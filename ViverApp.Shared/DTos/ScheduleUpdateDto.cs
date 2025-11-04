using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViverApp.Shared.DTos
{
    public class ScheduleUpdateDto
    {
        public int Idschedule { get; set; }
        public int Status { get; set; }
        public sbyte PendingPayment { get; set; }
        public string? AppointmentTitle { get; set; } = string.Empty;
        public string? AppointmentDescription { get; set; } = string.Empty;
        public int AppointmentType { get; set; }
        public DateTime? Appointmentdate { get; set; }
        public DateTime? OriginalDate { get; set; }
        public decimal? AppointmentPrice { get; set; }
        public sbyte Rescheduled { get; set; }
        public string? MedicalReport { get; set; }
        public string? Feedback { get; set; }
        public float? Rating { get; set; }
        public int IdPacient { get; set; }
        public string? PacientName { get; set; }
        public string? DoctorName { get; set; }
        public int UserTypeUpdated { get; set; }
        public string? UserNameUpdated { get; set; }
        public sbyte IsOnline { get; set; }
        public sbyte? CallConcluded { get; set; }
        public int? OldStatus { get; set; }

        public ScheduleUpdateDto() { }

        public ScheduleUpdateDto(ScheduleDto dto, int userTypeUpd, string userNameUpd = "", int? oldstatus = default)
        {
            Idschedule = dto.IdSchedule;
            Status = dto.Status ?? 1;
            PendingPayment = dto.PendingPayment;
            AppointmentTitle = dto.AppointmentTitle;
            AppointmentDescription = dto.AppointmentDescription;
            AppointmentType = dto.AppointmentType;
            Appointmentdate = dto.AppointmentDate;
            OriginalDate = dto.OriginalDate;
            AppointmentPrice = dto.AppointmentPrice;
            Rescheduled = dto.Rescheduled;
            MedicalReport = dto.MedicalReport;
            Feedback = dto.FeedBack;
            Rating = dto.Rating;
            IdPacient = dto.IdPatient;
            PacientName = dto.UserName;
            DoctorName = dto.DoctorName;
            UserTypeUpdated = userTypeUpd;
            UserNameUpdated = userNameUpd;
            IsOnline = dto.IsOnline;
            CallConcluded = dto.CallConcluded;
            OldStatus = oldstatus;
        }
    }
}
