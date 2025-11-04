using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViverAppMobile.Models
{
    public class ApiConfig
    {
        public bool UseLocalhost { get; set; }
        public string? ProductionUrl { get; set; }
        public string? LocalhostUrl { get; set; }
    }
}
