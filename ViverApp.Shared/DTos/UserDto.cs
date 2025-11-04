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
    public class UserDto
    {
        public int IdUser { get; set; }
        public int Usertype { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Fone { get; set; }
        public DateOnly? BirthDate { get; set; }
        public int Status { get; set; }
        public sbyte? IsPremium { get; set; }
        public sbyte? NotifyEmail { get; set; }
        public sbyte? Notifypush { get; set; }
        public string? Cpf { get; set; }
        public string? Adress { get; set; }
        public string? Neighborhood { get; set; }
        public string? Number { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Postalcode { get; set; }
        public string? Complement { get; set; }
        public DateTime Createdat { get; set; }

        public UserDto() { }

        public UserDto(User u)
        {
            IdUser = u.Iduser;
            Usertype = u.Usertype;
            Name = u.Name;
            Email = u.Email;
            Fone = u.Fone;
            BirthDate = u.Birthdate;
            IsPremium = u.Ispremium;
            Status = u.Status;
            NotifyEmail = u.Notifyemail;
            Notifypush = u.Notifypush;
            Cpf = u.Cpf;
            Adress = u.Adress;
            Neighborhood = u.Neighborhood;
            Number = u.Number;
            City = u.City;
            State = u.State;
            Postalcode = u.Postalcode;
            Complement = u.Complement;
            Createdat = u.Createdat;
        }
    }
}
