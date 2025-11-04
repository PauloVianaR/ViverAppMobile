using CommunityToolkit.Mvvm.Input;
using Mopups.Pages;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ViverApp.Shared.DTos;
using ViverApp.Shared.Models;
using ViverAppMobile.Controls;
using ViverAppMobile.Helpers;
using ViverAppMobile.Services;
using ViverAppMobile.Workers;

namespace ViverAppMobile.Views.Popups;

public partial class CompleteSchedulePopup : PopupPage, IPopupAsync
{
    private readonly TaskCompletionSource<object?> _taskCompletionSource = new();
    private readonly ObservableCollection<ScheduleAttachment> Attachments = [];
    private readonly ScheduleAttachmentsService scheduleAttachmentsService;

    public ICommand DeleteFileAsyncCommand { get; private set; }
    public ScheduleDto? UserSchedule { get; set; }

    private bool canFinalize = false;
    public bool CanFinalize
    {
        get => canFinalize;
        set
        {
            canFinalize = value;
            OnPropertyChanged(nameof(CanFinalize));
        }
    }

    public CompleteSchedulePopup()
	{
		InitializeComponent();

        scheduleAttachmentsService = new();
        UserSchedule = PopupHelper<ScheduleDto>.GetValue();
        DeleteFileAsyncCommand = new AsyncRelayCommand<ScheduleAttachment>(DeleteFileAsync);

        AttachmentsCollection.ItemsSource = Attachments;
        BindingContext = this;
	}

    public bool ClosePopup()
    {
        _taskCompletionSource.TrySetResult(false);
        return base.OnBackgroundClicked();
    }

    public Task<object?> WaitForResultAsync() => _taskCompletionSource.Task;
    protected override bool OnBackButtonPressed() => ClosePopup();
    protected override bool OnBackgroundClicked() => ClosePopup();
    private void BackButtonPressed(object sender, TappedEventArgs e) => ClosePopup();
    private void MedicalReportEditor_TextChanged(object sender, TextChangedEventArgs e) => CanFinalize = !string.IsNullOrWhiteSpace(e.NewTextValue);

    private void FinalizeSchedule(object sender, TappedEventArgs e)
    {
        if (!CanFinalize)
            return;

        PopupHelper<string>.SetValue(MedicalReportEditor.Text);
        _taskCompletionSource?.TrySetResult(true);
    }

    private void UploadFile(object sender, TappedEventArgs e) => _ = UploadFileAsync();
    private async Task UploadFileAsync()
    {
        try
        {
            await PopupHelper.PushLoadingAsync();

            var allowedFileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                {
                    DevicePlatform.Android,
                    new[]
                    {
                        "application/pdf",
                        "text/plain",
                        "application/msword",
                        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                        "application/rtf",
                        "application/vnd.oasis.opendocument.text",
                        "image/jpeg",
                        "image/png",
                        "image/gif",
                        "image/webp"
                    }
                },
                {
                    DevicePlatform.WinUI,
                    new[]
                    {
                        ".pdf", ".txt", ".doc", ".docx", ".rtf", ".odt",
                        ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".webp"
                    }
                },
                {
                    DevicePlatform.iOS,
                    new[]
                    {
                        "public.text",
                        "public.image",
                        "com.adobe.pdf",
                        "com.microsoft.word.doc",
                        "org.openxmlformats.wordprocessingml.document"
                    }
                }
            });

            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Selecione um arquivo de texto ou imagem",
                FileTypes = allowedFileTypes
            }) ?? throw new OperationCanceledException();

            var stream = await result.OpenReadAsync();

            var dto = new ScheduleAttachmentDto
            {
                Idschedule = UserSchedule.IdSchedule,
                Filename = result.FileName,
                Size = (float)Math.Round(stream.Length / 1000f, 2),
                FileStream = stream
            };

            var resp = await scheduleAttachmentsService.UploadFileAsync(dto);

            if (!resp.WasSuccessful)
            {
                Master.GlobalToken.ThrowIfCancellationRequested();
                resp.ThrowIfIsNotSucess();
            }

            var newAttachment = resp.Response ?? throw new Exception("Não foi possível recuperar o arquivo enviado");
            Attachments.Add(newAttachment);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            await Messenger.ShowErrorMessageAsync(ex.Message, "Ops...");
        }
        finally
        {
            await PopupHelper.PopLoadingAsync();
        }
    }

    private async Task DeleteFileAsync(ScheduleAttachment? scheduleAttachment)
    {
        if (!await Messenger.ShowQuestionMessage(
            $"Deseja mesmo exlcuir o arquivo \"{scheduleAttachment.Filename}\" ??\n\nObs: Esta ação não pode ser desfeita"))
            return;

        try
        {
            await PopupHelper.PushLoadingAsync();

            var resp = await scheduleAttachmentsService.DeleteAsync(scheduleAttachment.Idscheduleattachments);
            if (!resp.WasSuccessful)
            {
                Master.GlobalToken.ThrowIfCancellationRequested();
                resp.ThrowIfIsNotSucess();
            }

            Attachments.Remove(scheduleAttachment);

            await Messenger.ShowToastMessage("Anexo Removido");
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            await Messenger.ShowErrorMessageAsync(ex.Message);
        }
        finally
        {
            await PopupHelper.PopLoadingAsync();
        }
    }
}