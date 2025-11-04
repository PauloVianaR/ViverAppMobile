using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViverAppMobile.Models
{
    public class CarouselItem(string image, string title, string sub, string info, string icon, Color backcolor, Thickness margin)
    {
        public string Image { get; set; } = image;
        public string Title { get; set; } = title;
        public string Subtitle { get; set; } = sub;
        public string Info { get; set; } = info;
        public string Icon { get; set; } = icon;
        public Color IconBackColor { get; set; } = backcolor;
        public Thickness Margin { get; set; } = margin;
    }
}
