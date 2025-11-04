using Microsoft.EntityFrameworkCore;
using System;
using System.Runtime.InteropServices;
using ViverApp.Shared.Context;
using ViverAppNotificationWorker.Workers;

#if DEBUG
[DllImport("user32.dll")]
static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

[DllImport("kernel32.dll")]
static extern IntPtr GetConsoleWindow();

const int SW_MINIMIZE = 6;

var handle = GetConsoleWindow();
ShowWindow(handle, SW_MINIMIZE);
#endif

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        string conn = context.Configuration.GetConnectionString("DefaultConnection")!;

        services.AddDbContext<ViverappmobileContext>(options =>
            options.UseMySql(conn, ServerVersion.AutoDetect(conn)));

        services.AddHostedService<EmailWorker>();
        services.AddHostedService<NotificationWorker>();
    })
    .Build();

await host.RunAsync();