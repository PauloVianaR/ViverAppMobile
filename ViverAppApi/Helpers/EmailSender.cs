using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Mail;
using ViverApp.Shared.Models;
using ViverApp.Shared.Context;

namespace ViverAppApi.Helpers;

public static class EmailSender
{
    private const string _smtpUser = "vivermobileapp@gmail.com";
    private const string _smtpPassword = "joymfkcglqchmjgi";
    public static ViverappmobileContext? Context { get; set; }

    public static async Task SendEmailAsync(string to, string subject, string body, Severity severity = Severity.Low)
    {
        if (Context == null)
            throw new InvalidOperationException("O EmailSender.Context não foi configurado.");

        var email = new EmailQueue
        {
            Sender = _smtpUser,
            Receiver = to,
            Subject = subject,
            Body = body,
            Severity = (int)severity,
            Createdat = DateTime.Now
        };

        Context.EmailQueues.Add(email);
        await Context.SaveChangesAsync();

        Console.WriteLine($"📬 E-mail enfileirado para {to} (prioridade {severity}).");
    }

    public static async Task SendEmailConfirmationAsync(string to, string subject, string body, int confirmationCode)
    {
        if (Context == null)
            throw new InvalidOperationException("O EmailSender.Context não foi configurado.");

        using var transaction = await Context.Database.BeginTransactionAsync();

        var email = new EmailQueue
        {
            Sender = _smtpUser,
            Receiver = to,
            Subject = subject,
            Body = body,
            Severity = (int)Severity.High,
            Createdat = DateTime.Now
        };

        Context.EmailQueues.Add(email);
        await Context.SaveChangesAsync();

        var confirmation = new EmailConfirmation
        {
            Idemail = email.Idemail,
            Confirmationcode = confirmationCode,
            Expiresat = DateTime.Now.AddMinutes(15)
        };

        Context.EmailConfirmations.Add(confirmation);
        await Context.SaveChangesAsync();

        await transaction.CommitAsync();

        Console.WriteLine($"✅ E-mail de confirmação enfileirado para {to} (expira às {DateTime.Now.AddMinutes(15):HH:mm}).");
    }
}
