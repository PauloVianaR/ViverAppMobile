using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViverApp.Shared.Models
{
    public enum NotificationType
    {
        UpdatedSystem = 1,
        AwaitingApproval,
        PendingPayment,
        RescheduledAppointment,
        CanceledAppointment,
        FinishedAppointment,
        PaymentSuccess
    }

    public enum Severity
    {
        None,
        High,
        Medium,
        Low
    }
}
