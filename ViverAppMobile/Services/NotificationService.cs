using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViverApp.Shared.Models;

namespace ViverAppMobile.Services
{
    public class NotificationService : Service<Notification>
    {
        public const string endPoint = $"{baseApiPoint}/Notification";
        public NotificationService() : base(endPoint) { }
    }
}
