using System;
using System.Collections.Generic;

namespace ViverApp.Shared.Models;

public partial class Clinic
{
    public int Idclinic { get; set; }

    public string? Corporatereason { get; set; }

    public string? Fantasyname { get; set; }

    public string? Cnpj { get; set; }

    public string? Email { get; set; }

    public string? Adress { get; set; }

    public string? Number { get; set; }

    public string? Neighborhood { get; set; }

    public string? Complement { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Fone { get; set; }

    public string? Postalcode { get; set; }

    public virtual ICollection<AvailabilityClinic> AvailabilityClinics { get; set; } = new List<AvailabilityClinic>();

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}
