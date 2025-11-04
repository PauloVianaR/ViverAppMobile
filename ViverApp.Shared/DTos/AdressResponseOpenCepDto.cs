using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViverApp.Shared.DTos
{
    public class AdressResponseOpenCepDto
    {
        public string? Cep { get; set; }
        public string? Logradouro { get; set; }
        public string? Bairro { get; set; }
        public string? Localidade { get; set; }
        public string? Uf { get; set; }
        public string? Ibge { get; set; }
    }
}
