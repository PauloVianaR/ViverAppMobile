using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using ViverApp.Shared.Context;
using ViverApp.Shared.Models;

namespace ViverAppNotificationWorker.Workers
{
    public class NotificationWorker(ILogger<EmailWorker> logger, IServiceProvider serviceProvider, IConfiguration config) : BackgroundService
    {
        private readonly ILogger<EmailWorker> _logger = logger;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly IConfiguration _config = config;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("NotificationWorker iniciado.");

            try
            {
                var credentialPath = Path.Combine(AppContext.BaseDirectory, "Credentials", "viverappmobilemaui-firebase-adminsdk-fbsvc-488e62cbb3.json");

                if (!File.Exists(credentialPath))
                {
                    _logger.LogError($"Arquivo de credencial Firebase não encontrado em: {credentialPath}");
                    return;
                }

                if (FirebaseApp.DefaultInstance == null)
                {
                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = GoogleCredential.FromFile(credentialPath)
                    });

                    _logger.LogInformation("FirebaseApp inicializado com sucesso.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao inicializar o Firebase Admin SDK.");
                return;
            }

            var messaging = FirebaseMessaging.DefaultInstance;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ViverappmobileContext>();

                    var pending = await context.Notifications
                        .Where(n => n.Sent == 0 && n.Targetid != null)
                        .OrderBy(n => n.Severity)
                        .ThenBy(n => n.Createdat)
                        .Take(10)
                        .ToListAsync(stoppingToken);

                    foreach (var n in pending)
                    {
                        try
                        {
                            var user = await context.Users.FirstOrDefaultAsync(u => u.Iduser == n.Targetid);
                            if (user is null)
                                continue;

                            if (user.Notifypush != (sbyte)1)
                            {
                                _logger.LogWarning($"Usuário {n.Targetid} desabilitou a notificação por push. Notificação [ID][{n.Idnotification}] ignorada.");
                                continue;
                            }

                            var devicetoken = user.Devicetoken;

                            if (string.IsNullOrEmpty(devicetoken))
                            {
                                _logger.LogWarning($"Usuário {n.Targetid} sem DeviceToken. Notificação [ID][{n.Idnotification}] ignorada.");
                                continue;
                            }

                            var message = new Message
                            {
                                Token = devicetoken,
                                Notification = new FirebaseAdmin.Messaging.Notification
                                {
                                    Title = n.Title ?? "Nova notificação",
                                    Body = n.Pushdescription ?? "",
                                },
                                Data = new Dictionary<string, string>
                                {
                                    { "notificationId", n.Idnotification.ToString() },
                                    { "type", n.Notificationtype.ToString() }
                                },
                                Android = new AndroidConfig
                                {
                                    Priority = n.Severity == (int)Severity.High ? Priority.High : Priority.Normal,
                                    Notification = new AndroidNotification
                                    {
                                        Sound = "default"
                                    }
                                }
                            };

                            var response = await messaging.SendAsync(message, stoppingToken);

                            n.Sent = 1;
                            context.Notifications.Update(n);
                            await context.SaveChangesAsync(stoppingToken);

                            _logger.LogInformation($"Notificação {n.Idnotification} enviada com sucesso (MsgId: {response}) para TargetId {n.Targetid}.");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Erro ao enviar notificação {n.Idnotification}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro geral no NotificationWorker");
                }

                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }

            _logger.LogInformation("NotificationWorker finalizado.");
        }
    }
}
