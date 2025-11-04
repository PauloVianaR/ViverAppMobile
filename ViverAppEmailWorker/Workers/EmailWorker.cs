using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks.Dataflow;
using ViverApp.Shared.Context;
using ViverApp.Shared.Models;

namespace ViverAppNotificationWorker.Workers
{
    public class EmailWorker(ILogger<EmailWorker> logger, IServiceProvider serviceProvider, IConfiguration config) : BackgroundService
    {
        private readonly ILogger<EmailWorker> _logger = logger;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly IConfiguration _config = config;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 Email Worker paralelo iniciado.");

            int pollingInterval = _config.GetValue("Worker:PollingIntervalSeconds", 10);
            int maxDegreeOfParallelism = _config.GetValue("Worker:MaxConcurrentSends", 5);

            var options = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism,
                CancellationToken = stoppingToken
            };

            var actionBlock = new ActionBlock<EmailQueue>(async email =>
            {
                await ProcessEmailAsync(email, stoppingToken);
            }, options);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ViverappmobileContext>();

                    var pendingEmails = await db.EmailQueues
                        .Where(e => e.Status == (int)EmailStatus.Pending)
                        .OrderBy(e => e.Severity)
                        .ThenBy(e => e.Createdat)
                        .Take(maxDegreeOfParallelism * 2)
                        .ToListAsync(stoppingToken);

                    if (pendingEmails.Count == 0)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(pollingInterval), stoppingToken);
                        continue;
                    }

                    foreach (var email in pendingEmails)
                    {
                        actionBlock.Post(email);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro no loop principal do Worker.");
                }

                await Task.Delay(TimeSpan.FromSeconds(pollingInterval), stoppingToken);
            }

            actionBlock.Complete();
            await actionBlock.Completion;
        }

        private async Task ProcessEmailAsync(EmailQueue email, CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ViverappmobileContext>();

                var existingemail = await db.EmailQueues.FirstOrDefaultAsync(e => e.Idemail == email.Idemail, cancellationToken);
                if (existingemail == null || email.Status != 1) return;

                email = existingemail;

                _logger.LogInformation($"📧 Tentando enviar e-mail para {email.Receiver}. Tentativa #{email.Tries + 1}");

                bool sent = await TrySendEmailAsync(email);

                email.Tries += 1;

                if (sent)
                {
                    email.Status = (int)EmailStatus.Sent;
                    _logger.LogInformation($"✅ E-mail enviado com sucesso para {email.Receiver}");
                }
                else if (email.Tries >= 5)
                {
                    email.Status = (int)EmailStatus.Fail;
                    _logger.LogWarning($"❌ E-mail para {email.Receiver} não enviado após 5 tentativas.");
                }
                else
                {
                    int delaySeconds = GetBackoffDelay(email.Tries);
                    _logger.LogInformation($"⏳ Falha no envio. Re-tentando em {delaySeconds} segundos.");
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
                }

                await db.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao processar e-mail {email.Receiver}.");
            }
        }

        private static int GetBackoffDelay(int tries) => tries switch
        {
            1 => 5,
            2 => 30,
            3 => 120,
            4 => 300,
            _ => 600
        };

        private async Task<bool> TrySendEmailAsync(EmailQueue email)
        {
            try
            {
                var smtpSection = _config.GetSection("Smtp");

                using var smtp = new SmtpClient(smtpSection["Host"], smtpSection.GetValue<int>("Port"))
                {
                    Credentials = new NetworkCredential(smtpSection["User"], smtpSection["Password"]),
                    EnableSsl = true
                };

                var mail = new MailMessage
                {
                    From = new MailAddress(smtpSection["User"]!, "Suporte Viver"),
                    Subject = email.Subject,
                    Body = email.Body,
                    IsBodyHtml = true
                };

                mail.To.Add(email.Receiver);

                await smtp.SendMailAsync(mail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"❌ Falha ao enviar e-mail {email.Receiver}: {ex.Message}");
                return false;
            }
        }
    }
}
