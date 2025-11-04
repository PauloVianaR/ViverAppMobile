using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViverApp.Shared.DTos
{
    public class RefreshRequestDto
    {
        public string RefreshToken { get; set; } = null!;
        public int UserType { get; set; }
    }
}
