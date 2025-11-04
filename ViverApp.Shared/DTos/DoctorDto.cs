using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ViverApp.Shared.Models;
using static System.Net.Mime.MediaTypeNames;

namespace ViverApp.Shared.DTos
{
    public class DoctorDto
    {
        public int IdUser { get; set; }
        public int Usertype { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Fone { get; set; }
        public string? Cpf { get; set; }
        public int Status { get; set; }
        public sbyte? NotifyEmail { get; set; }
        public sbyte? Notifypush { get; set; }
        public int Iddoctorprops { get; set; }
        public string? Title { get; set; }
        public string ProfessionalDoctorName => $"{Title} {Name}";
        public string? Crm { get; set; }
        public string? Mainspecialty { get; set; }
        public int? Medicalexperience { get; set; }
        public float? Rating { get; set; }
        public sbyte Attendonline { get; set; }
        public int Maxonlinedayconsultation { get; set; }
        public int Maxpresencialdayconsultation { get; set; }
        public DateTime Createdat { get; set; }

        public DoctorDto() { }

        public DoctorDto(UserDto user, DoctorProp props)
        {
            IdUser = user.IdUser;
            Usertype = user.Usertype;
            Name = user.Name;
            Email = user.Email;
            Fone = user.Fone;
            Cpf = user.Cpf;
            Status = user.Status;
            NotifyEmail = user.NotifyEmail;
            Notifypush = user.Notifypush;
            Iddoctorprops = props.Iddoctorprops;
            Title = props.Title;
            Crm = props.Crm;
            Mainspecialty = props.Mainspecialty;
            Medicalexperience = props.Medicalexperience;
            Rating = props.Rating;
            Attendonline = props.Attendonline;
            Maxonlinedayconsultation = props.Maxonlinedayconsultation;
            Maxpresencialdayconsultation = props.Maxpresencialdayconsultation;
            Createdat = user.Createdat;
        }
    }
}
