using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViverApp.Shared.Models
{
    public enum ConfigType
    {
        AppOnline = 1,
        CanOnlineCalls,
        CanCancelAppointments,
        CancellationPeriodDays,
        MaxScheduleDays,
        NotifyEmail,
        NotifyPush,
        ProductionMode,
        AverageConsultationTimeMin,
        AverageExaminationTimeMin,
        AverageSurgeryTimeMin,
        AppointmentIntervalMin,
        AppOnlineMaster
    }
}
