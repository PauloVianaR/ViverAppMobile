using System.Security.Cryptography.X509Certificates;
using ViverAppVideoHub.Controllers;

var builder = WebApplication.CreateBuilder(args);

#if DEBUG
var cert = X509CertificateLoader.LoadPkcs12FromFile(
    @"C:\Certs\devhub_san.pfx",
    "1234",
    X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable
);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5279);
    options.ListenAnyIP(7177, listenOptions =>
    {
        listenOptions.UseHttps(cert);
    });
});
#endif

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAppDir", policy =>
    {
        policy
#if DEBUG
            .WithOrigins(
                "https://appdir",
                "https://192.168.18.2:7177",
                "http://192.168.18.2:5279",
                "null"
            )
#else
            .SetIsOriginAllowed(_ => true)
#endif
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAppDir");
app.MapControllers().RequireCors("AllowAppDir");
app.MapHub<VideoHub>("/videohub").RequireCors("AllowAppDir");

app.Run();