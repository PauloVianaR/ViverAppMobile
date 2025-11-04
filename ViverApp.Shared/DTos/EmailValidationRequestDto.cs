using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViverApp.Shared.DTos
{
    public class EmailValidationRequestDto
    {
        public string? Email { get; set; }
        public int ConfirmationCode { get; set; }
    }
}
