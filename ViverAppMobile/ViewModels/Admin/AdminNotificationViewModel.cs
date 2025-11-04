using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViverApp.Shared.Models;
using ViverAppMobile.Controls;
using ViverAppMobile.Helpers;
using ViverAppMobile.Services;
using ViverAppMobile.Workers;

namespace ViverAppMobile.ViewModels.Admin
{
    public partial class AdminNotificationViewModel : ObservableObject, IViewModelInstancer
    {
        private readonly NotificationService notificationService;
        private bool isLoading = false;
        private List<Notification> allnotifications = [];

        [ObservableProperty] private int defaults = 0;
        [ObservableProperty] private int notread = 0;
        [ObservableProperty] private int highseverity = 0;
        [ObservableProperty] private int approvals = 0;
        [ObservableProperty] private bool isReloading = false;
        [ObservableProperty] private string selectedNotificationTypeFilter;
        [ObservableProperty] private string selectedNotificationReadFilter;
        [ObservableProperty] private ObservableCollection<Notification> notifications = [];

        public ObservableCollection<string> NotificationTypes { get; set; } = [];
        public ObservableCollection<string> NotificationsRead { get; set; } = ["Não Lidas", "Lidas"];

        public AdminNotificationViewModel()
        {
            notificationService = new();

            NotificationTypes = 
            [
                "Todos os tipos", 
                "Sistema Atualizado", 
                "Novo usuário aguardando aprovação", 
                "Atendimento confirmado com pagamento pendente", 
                "Atendimento reagendado", 
                "Atendimento cancelado"
            ];
            SelectedNotificationTypeFilter = NotificationTypes[0];
            SelectedNotificationReadFilter = NotificationsRead[0];
        }

        public async Task InitializeAsync()
        {
            await Loader.RunWithLoadingAsync(LoadAllAsync);
        }

        public async Task<string?> LoadAllAsync()
        {
            if (isLoading)
                return null;
            isLoading = true;

            try
            {
                Notifications.Clear();

                var resp = await notificationService.GetAllAsync();
                if (!resp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    resp.ThrowIfIsNotSucess();
                }

                allnotifications = resp?.Response?
                    .OrderBy(n => n.Severity)
                    .ThenByDescending(n => n.Createdat)
                    .ToList() ?? [];

                Notifications = allnotifications.Where(n => n.Read == 0).ToObservableCollection();

                Defaults = allnotifications.Count(n => n.Notificationtype == (int)NotificationType.PendingPayment);
                Notread = Notifications.Count;
                Highseverity = allnotifications.Count(n => n.Severity == (int)Severity.High);
                Approvals = allnotifications.Count(n => n.Notificationtype == (int)NotificationType.AwaitingApproval);

                return null;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                isLoading = false;
            }
        }

        [RelayCommand] private async Task ReloadPage()
        {
            if (IsReloading)
                return;
            IsReloading = true;
            await Loader.RunWithLoadingAsync(LoadAllAsync);
            IsReloading = false;
        }

        partial void OnSelectedNotificationReadFilterChanged(string value) => FilterNotifications();
        partial void OnSelectedNotificationTypeFilterChanged(string value) => FilterNotifications();

        private void FilterNotifications()
        {
            int notificationReadFilter = NotificationsRead.IndexOf(SelectedNotificationReadFilter);
            int notificationTypeFilter = NotificationTypes.IndexOf(SelectedNotificationTypeFilter);

            var filterednotifications = allnotifications.ToList();

            if (notificationReadFilter > 0)
                filterednotifications = filterednotifications.Where(n => n.Read == 1).ToList();
            else
                filterednotifications = filterednotifications.Where(n => n.Read == 0).ToList();

            if (notificationTypeFilter > 0)
                filterednotifications = filterednotifications.Where(n => n.Notificationtype == notificationTypeFilter).ToList();

            Notifications = filterednotifications.ToObservableCollection();
        }

        [RelayCommand] private async Task ReadAll()
        {
            try
            {
                await PopupHelper.PushLoadingAsync();

                allnotifications.Where(n => n.Read == 0).ToList().ForEach(n =>
                {
                    ReadNotificationRemoveCollection(n);
                    n.Read = 1;
                });

                SelectedNotificationReadFilter = NotificationsRead[0];
                SelectedNotificationTypeFilter = NotificationTypes[0];
                Defaults = 0;
                Notread = 0;
                Highseverity = 0;
                Approvals = 0;
            }
            catch(Exception ex)
            {
                await Messenger.ShowErrorMessageAsync(ex.Message);
            }

            await PopupHelper.PopAllPopUpAsync();
        }

        [RelayCommand] private void ReadNotification(Notification notification)
        {
            Notificator.Read(notification);

            switch ((NotificationType)notification.Notificationtype)
            {
                case NotificationType.AwaitingApproval:
                    Approvals--;
                    break;
                case NotificationType.PendingPayment:
                    Defaults--;
                    break;
            };

            if ((Severity)notification.Severity == Severity.High)
                Highseverity--;

            var existingNotification = allnotifications.FirstOrDefault(n => n.Idnotification == notification.Idnotification);
            if (existingNotification is not null)
            {
                existingNotification.Read = 1;
                existingNotification.Severity = (int)Severity.None;
            }

            Notread--;
        }

        [RelayCommand] private void ReadNotificationRemoveCollection(Notification notification)
        {
            Notifications.Remove(notification);
            this.ReadNotification(notification);
        }
    }
}
