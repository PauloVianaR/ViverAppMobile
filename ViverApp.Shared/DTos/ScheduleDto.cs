using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViverApp.Shared.DTos
{
    public class ScheduleDto
    {
        public int IdSchedule { get; set; }
        public int IdAppointment { get; set; }
        public string? AppointmentTitle { get; set; }
        public string? AppointmentDescription { get; set; } = string.Empty;
        public int AppointmentType { get; set; }
        public decimal? AppointmentPrice { get; set; }
        public TimeOnly? AverageTime { get; set; }
        public string? UserName { get; set; } = string.Empty;
        public string? UserPhone { get; set; } = string.Empty;
        public string? PatientCpf { get; set; } = string.Empty;
        public string? PatientEmail { get; set; } = string.Empty;
        public DateOnly? PatientBirthDate { get; set; }
        public int IdPatient { get; set; }
        public int Iddoctor { get; set; }
        public string? DoctorName { get; set; } = string.Empty;
        public string? DoctorSpecialty { get; set; } = string.Empty;
        public string? DoctorTitle { get; set; } = string.Empty;
        public string ProfessionalDoctorName => $"{DoctorTitle} {DoctorName}";
        public string? ClinicFantasyName { get; set; } = string.Empty;
        public string? ClinicComplement { get; set; } = string.Empty;
        public string? ClinicAdress { get; set; } = string.Empty;
        public string? ClinicNumber { get; set; } = string.Empty;
        public string? ClinicNeighborhood { get; set; } = string.Empty;
        public string? ClinicCity { get; set; } = string.Empty;
        public string? ClinicState { get; set; } = string.Empty;
        public string? ClinicPostalCode { get; set; } = string.Empty;
        public string? ClinicPhone { get; set; } = string.Empty;
        public int? Status { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public string? Obs { get; set; } = string.Empty;
        public sbyte IsOnline { get; set; }
        public sbyte? CallConcluded { get; set; }
        public sbyte Rescheduled { get; set; }
        public sbyte PendingPayment { get; set; }
        public DateTime? OriginalDate { get; set; }
        public float? Rating { get; set; }
        public string? MedicalReport { get; set; } = string.Empty;
        public string? FeedBack { get; set; } = string.Empty;
    }
}
