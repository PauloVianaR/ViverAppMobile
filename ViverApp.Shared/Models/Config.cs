using System;
using System.Collections.Generic;

namespace ViverApp.Shared.Models;

public partial class Config
{
    public int Idconfig { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public int? Value { get; set; }

    public sbyte Valueisbool { get; set; }

    public sbyte? Canshow { get; set; }
}
