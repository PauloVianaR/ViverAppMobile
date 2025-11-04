using ViverApp.Shared.Models;
using ViverAppMobile.Helpers;
using ViverAppMobile.Services;

namespace ViverAppMobile.Workers
{
    public static class Notificator
    {
        private static readonly NotificationService service = new();

        public static void Send(NotificationType notificationType, string description, string _title = "", int? targetId = null, string? pushdescription = null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    string title = string.IsNullOrWhiteSpace(_title) ? EnumTranslator.TranslateNotificationType(notificationType) : _title;

                    _ = await service.CreateAsync(new Notification()
                    {
                        Title = title,
                        Description = description,
                        Pushdescription = pushdescription,
                        Notificationtype = (int)notificationType,
                        Severity = (int)GetSeverity(notificationType),
                        Targetid = targetId
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro Notificator.Send: {ex}");
                }
            });
        }

        public static void Read(Notification notification)
        {
            if (notification.Read == 1)
                return;

            _ = Task.Run(async () =>
            {
                try
                {
                    notification.Severity = (int)Severity.None;
                    _ = await service.UpdateAsync(notification.Idnotification, notification);
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Erro Notificator.Read: {ex}");
                }
            });
        }

        public static Severity GetSeverity(NotificationType type)
        {
            return type switch
            {
                NotificationType.UpdatedSystem => Severity.Low,
                NotificationType.AwaitingApproval => Severity.High,
                NotificationType.PendingPayment => Severity.Medium,
                NotificationType.RescheduledAppointment => Severity.Medium,
                NotificationType.CanceledAppointment => Severity.Low,
                NotificationType.FinishedAppointment => Severity.Low,
                _ => Severity.Low
            };
        }
    }
}
