using System;
using System.Collections.Generic;

namespace ViverApp.Shared.Models;

public partial class User
{
    public int Iduser { get; set; }

    public int Usertype { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Fone { get; set; }

    public DateOnly? Birthdate { get; set; }

    public string? Password { get; set; }

    public int Status { get; set; }

    public sbyte? Ispremium { get; set; }

    public sbyte Notifyemail { get; set; }

    public sbyte Notifypush { get; set; }

    public string? Cpf { get; set; }

    public string? Adress { get; set; }

    public string? Neighborhood { get; set; }

    public string? Number { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Postalcode { get; set; }

    public string? Complement { get; set; }

    public DateTime Createdat { get; set; }

    public string? Devicetoken { get; set; }

    public virtual ICollection<AvailabilityDoctor> AvailabilityDoctors { get; set; } = new List<AvailabilityDoctor>();

    public virtual DoctorProp? DoctorProp { get; set; }

    public virtual ICollection<PremiumUser> PremiumUsers { get; set; } = new List<PremiumUser>();

    public virtual ICollection<Schedule> ScheduleIddoctorNavigations { get; set; } = new List<Schedule>();

    public virtual ICollection<Schedule> ScheduleIduserNavigations { get; set; } = new List<Schedule>();

    public virtual ICollection<SpecialtysDoctor> SpecialtysDoctors { get; set; } = new List<SpecialtysDoctor>();

    public virtual ICollection<UserToken> UserTokens { get; set; } = new List<UserToken>();
}
