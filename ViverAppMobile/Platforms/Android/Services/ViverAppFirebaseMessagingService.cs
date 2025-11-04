using Android.App;
using Android.OS;
using AndroidX.Core.App;
using Firebase.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViverAppMobile.Platforms.Android.Services
{
    [Service(Exported = true, Name = "com.vivercompany.viverappmobile.ViverAppFirebaseMessagingService")]
    [IntentFilter(["com.google.firebase.MESSAGING_EVENT"])]
    public class ViverAppFirebaseMessagingService : FirebaseMessagingService
    {
        public override void OnMessageReceived(RemoteMessage message)
        {
            base.OnMessageReceived(message);

            var title = message.GetNotification()?.Title ?? "ViverApp";
            var body = message.GetNotification()?.Body ?? "Você tem uma nova notificação.";

            ShowNotification(title, body);
        }

        private void ShowNotification(string title, string body)
        {
            var channelId = "viverapp_channel";
            var notificationManager = NotificationManagerCompat.From(this);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(channelId, "ViverApp Notifications", NotificationImportance.Default);
                var manager = (NotificationManager)GetSystemService(NotificationService)!;
                manager.CreateNotificationChannel(channel);
            }

            var builder = new NotificationCompat.Builder(this, channelId)
                .SetContentTitle(title)
                .SetContentText(body)
                .SetSmallIcon(Resource.Mipmap.appicon)
                .SetAutoCancel(true);

            notificationManager.Notify(new Random().Next(), builder.Build());
        }

        public override void OnNewToken(string token)
        {
            base.OnNewToken(token);
            System.Diagnostics.Debug.WriteLine($"[FCM TOKEN ATUALIZADO] {token}");
        }
    }
}
