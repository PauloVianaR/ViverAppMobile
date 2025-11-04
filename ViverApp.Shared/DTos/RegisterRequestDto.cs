using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ViverApp.Shared.Models;

namespace ViverApp.Shared.DTos
{
    public class RegisterRequestDto
    {
        public User User { get; set; } = null!;
        public DoctorPropDto? DoctorProp { get; set; }

        [JsonConstructor]
        internal RegisterRequestDto() { }

        public RegisterRequestDto(User _user, DoctorPropDto? _doctorProp)
        {
            User = _user;
            DoctorProp = _doctorProp;
        }
    }
}
