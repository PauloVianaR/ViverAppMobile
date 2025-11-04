using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ViverApp.Shared.Models;

namespace ViverApp.Shared.DTos
{
    public class ScheduleAttachmentDto
    {
        public int Idschedule { get; set; }
        public string Filename { get; set; } = null!;
        public float? Size { get; set; }
        public IFormFile? File { get; set; }

        [JsonIgnore]
        public Stream? FileStream { get; set; }
    }
}
