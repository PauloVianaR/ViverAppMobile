using System.Net.Http.Json;
using ViverApp.Shared.DTos;
using ViverApp.Shared.Models;
using ViverAppMobile.Handlers;

#if ANDROID
using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
#endif

namespace ViverAppMobile.Services
{
    public class ScheduleAttachmentsService : Service<ScheduleAttachment>
    {
        public const string endPoint = $"{baseApiPoint}/ScheduleAttachments";
        public ScheduleAttachmentsService() : base(endPoint) { }

        public async Task<ResponseHandler<ScheduleAttachment>> UploadFileAsync(ScheduleAttachmentDto entity)
        {
            ResponseHandler<ScheduleAttachment> resp = new();

            try
            {
                using var content = new MultipartFormDataContent
                {
                    { new StringContent(entity.Idschedule.ToString()), nameof(entity.Idschedule) },
                    { new StringContent(entity.Filename ?? string.Empty), nameof(entity.Filename) },
                    { new StringContent(entity.Size?.ToString("F2") ?? "0"), nameof(entity.Size) }
                };

                if (entity.FileStream != null)
                {
                    var streamContent = new StreamContent(entity.FileStream);
                    streamContent.Headers.ContentType =
                        new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                    content.Add(streamContent, nameof(entity.File), entity.Filename ?? string.Empty);
                }

                var httpResp = await HttpClient.PostAsync(endPoint, content);

                if (!httpResp.IsSuccessStatusCode)
                    throw new Exception(await httpResp.Content.ReadAsStringAsync());

                var data = await httpResp.Content.ReadFromJsonAsync<ScheduleAttachment>() ?? new();
                resp.IsSuccess(data);
            }
            catch (Exception ex)
            {
                resp.IsNotSuccess(ex.Message);
            }

            return resp;
        }

        public async Task<ResponseHandler<string>> DownloadAttachmentAsync(int idScheduleAttachment)
        {
            ResponseHandler<string> resp = new();

            try
            {
                var response = await HttpClient.GetAsync($"{endPoint}/download/{idScheduleAttachment}");
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Erro ao obter URL: {error}");
                }

                var fileUrl = (await response.Content.ReadAsStringAsync()).Trim('"');
                var fileName = Path.GetFileName(fileUrl);
                var extension = Path.GetExtension(fileName).ToLowerInvariant();

                var allowedTextExtensions = new[] { ".txt", ".pdf", ".doc", ".docx", ".rtf", ".odt" };
                var allowedImageExtensions = new[] { ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".webp" };

                var isAllowed =
                    allowedTextExtensions.Contains(extension) ||
                    allowedImageExtensions.Contains(extension);

                if (!isAllowed)
                    throw new Exception("Tipo de arquivo não permitido. Apenas documentos e imagens podem ser baixados.");

#if ANDROID
                var writeStatus = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
                var readStatus = await Permissions.CheckStatusAsync<Permissions.StorageRead>();

                if (writeStatus != PermissionStatus.Granted)
                    writeStatus = await Permissions.RequestAsync<Permissions.StorageWrite>();

                if (readStatus != PermissionStatus.Granted)
                    readStatus = await Permissions.RequestAsync<Permissions.StorageRead>();

                if (writeStatus != PermissionStatus.Granted || readStatus != PermissionStatus.Granted)
                    throw new Exception("O aplicativo precisa de permissão para salvar arquivos no dispositivo.");

                var context = Android.App.Application.Context;
                var uri = Android.Net.Uri.Parse(fileUrl);

                var request = new DownloadManager.Request(uri);
                request.SetTitle(fileName);
                request.SetDescription("Baixando arquivo...");
                request.SetNotificationVisibility(DownloadVisibility.VisibleNotifyCompleted);
                request.SetDestinationInExternalPublicDir(Android.OS.Environment.DirectoryDownloads, fileName);

                var downloadManager = (DownloadManager)context.GetSystemService(Context.DownloadService)!;
                downloadManager.Enqueue(request);

                resp.IsSuccess(Path.Combine(
                    Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath,
                    fileName));

#elif WINDOWS
                using var http = new HttpClient();
                var fileBytes = await http.GetByteArrayAsync(fileUrl);

                var downloadsPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "Downloads");

                var fullPath = Path.Combine(downloadsPath, fileName);
                await File.WriteAllBytesAsync(fullPath, fileBytes);
                resp.IsSuccess(fullPath);

#else
                throw new PlatformNotSupportedException("Plataforma não suportada.\n\nEntre em contato com o administrador");
#endif
            }
            catch (Exception ex)
            {
                resp.IsNotSuccess(ex.Message);
            }

            return resp;
        }


        public async Task<ResponseHandler<IEnumerable<ScheduleAttachment>>> GetAllByIdSchedule(int idSchedule)
        {
            ResponseHandler<IEnumerable<ScheduleAttachment>> resp = new();
            try
            {
                var httpResp = await HttpClient.GetAsync($"{endPoint}/getAllBySchedule/{idSchedule}");
                if (!httpResp.IsSuccessStatusCode)
                    throw new Exception(await httpResp.Content.ReadAsStringAsync());

                var data = await httpResp.Content.ReadFromJsonAsync<IEnumerable<ScheduleAttachment>>() ?? [];
                resp.IsSuccess(data);
            }
            catch (Exception ex)
            {
                resp.IsNotSuccess(ex.Message);
            }
            return resp;
        }
    }
}
