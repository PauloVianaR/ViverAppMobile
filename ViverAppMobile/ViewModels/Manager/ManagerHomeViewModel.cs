using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Globalization;
using ViverApp.Shared.DTos;
using ViverApp.Shared.Models;
using ViverAppMobile.Controls;
using ViverAppMobile.Helpers;
using ViverAppMobile.Models;
using ViverAppMobile.Services;
using ViverAppMobile.Views.Popups;
using ViverAppMobile.Workers;

namespace ViverAppMobile.ViewModels.Manager
{
    public partial class ManagerHomeViewModel : ObservableObject, IViewModelInstancer
    {
        private readonly ScheduleService scheduleService;
        private readonly PaymentService paymentService;

        [ObservableProperty] private int scheduleToday = 0;
        [ObservableProperty] private int schedulePresencial = 0;
        [ObservableProperty] private int scheduleOnline = 0;
        [ObservableProperty] private int doctorsTodayCount = 0;
        [ObservableProperty] private int scheduleConfirmedPaid = 0;
        [ObservableProperty] private int scheduleConfirmedNotPaid = 0;
        [ObservableProperty] private bool isReloading = false;
        [ObservableProperty] private bool isLoading = false;
        [ObservableProperty] private UserDto? loggedUser;

        public ObservableCollection<AsyncModel<ScheduleDto>> ConfirmedTodaySchedule { get; set; } = [];

        public string TodayWeek => DateTime.Today.ToString("D", Master.Culture);

        public ManagerHomeViewModel()
        {
            scheduleService = new();
            paymentService = new();
        }

        public async Task InitializeAsync()
        {
            await Loader.RunWithLoadingAsync(LoadAllAsync);
        }

        public async Task<string?> LoadAllAsync()
        {
            if (IsLoading)
                return null;
            IsLoading = true;

            try
            {
                LoggedUser = UserHelper.GetLoggedUser() ?? throw new Exception("Falha ao carregar dados do usuário logado.\nTente fazer o login novamente!");

                var scheduleResp = await scheduleService.GetScheduleAsync(0, isDoctor: default, isHistoric: false, page: 0, pagesize: int.MaxValue, filterStatus: (int)ScheduleStatus.Confirmed);
                if (!scheduleResp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    scheduleResp.ThrowIfIsNotSucess();
                }

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    var allSchedule = scheduleResp?.Response?.ToArray() ?? [];
                    var scheduleToday = allSchedule.Where(s => s.AppointmentDate.GetValueOrDefault().Date == DateTime.Today.Date);

                    ScheduleToday = scheduleToday.Count();
                    DoctorsTodayCount = scheduleToday.DistinctBy(s => s.Iddoctor).Count();
                    SchedulePresencial = scheduleToday.Count(s => s.IsOnline == 0 && s.PendingPayment == 0);
                    ScheduleConfirmedPaid = scheduleToday.Count(s => s.PendingPayment == 0);
                    ScheduleConfirmedNotPaid = ScheduleToday - ScheduleConfirmedPaid;
                    ScheduleOnline = ScheduleConfirmedPaid - SchedulePresencial;

                    scheduleToday
                        .OrderBy(s => s.AppointmentDate.GetValueOrDefault().Hour)
                        .ThenBy(s => s.AppointmentDate.GetValueOrDefault().Minute)
                        .ToList()
                        .ForEach(s => ConfirmedTodaySchedule.Add(new(s) { IsActive = s.PendingPayment == 0}));
                });

                return null;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ReloadPage()
        {
            if (IsReloading)
                return;
            IsReloading = true;
            await Loader.RunWithLoadingAsync(LoadAllAsync);
            IsReloading = false;
        }

        [RelayCommand]
        private async Task ShowScheduleDetails(AsyncModel<ScheduleDto> model)
        {
            var schedule = model.Model;
            await PopupHelper<ScheduleDto>.PushInstanceAsync<ScheduleDetailsPopup>(schedule);
        }

        [RelayCommand]
        private async Task CallToPacient(AsyncModel<ScheduleDto> model)
        {
            await PhoneHelper.CallAsync(model.Model.UserPhone);
        }

        [RelayCommand]
        private async Task ConfirmPayment(AsyncModel<ScheduleDto> schedule)
        {
            string[] arrayToSend = [schedule.Model?.UserName ?? "paciente", schedule.Model?.AppointmentPrice?.ToString("c2", new CultureInfo("pt-BR")) ?? "R$ 0,00"];

            var returnValue = await PopupHelper<string[]>.PushInstanceAsync<ConfirmPaymentPopup>(arrayToSend);
            if (!returnValue)
                return;

            await schedule.ExecuteAsync(async model =>
            {
                try
                {
                    string?[] responseArray = PopupHelper<string?[]>.GetValue()
                        ?? throw new Exception("Ocorreu um erro ao tentar confirmar o pagamento.\nTente novamente mais tarde");

                    if (responseArray.Length != 4)
                        throw new Exception("Ocorreu um erro ao tentar confirmar o pagamento.\nTente novamente mais tarde");

                    var paymethod = Enum.TryParse(responseArray[0], ignoreCase: true, out PayMethod method) ? method : PayMethod.None;
                    if (paymethod == PayMethod.None)
                        throw new Exception("Não foi possível carregar a forma de pagamento escolhida...");
                    string? cardlast4 = responseArray[1];
                    string? cardauthoriation = responseArray[2];
                    var paidday = DateTime.TryParse(responseArray[3], out DateTime datetimeformated) ? datetimeformated : DateTime.MinValue;

                    var paymentresp = await paymentService.CreateAsync(new Payment
                    {
                        Idpayment = 0,
                        Idpaymenttype = (int)paymethod,
                        Idschedule = model.IdSchedule,
                        Paidday = paidday,
                        Paidprice = model.AppointmentPrice,
                        Paidonline = 0,
                        Cardlast4 = cardlast4,
                        Cardauthorization = cardauthoriation
                    });

                    if (!paymentresp.WasSuccessful)
                    {
                        Master.GlobalToken.ThrowIfCancellationRequested();
                        paymentresp.ThrowIfIsNotSucess();
                    }

                    model.PendingPayment = 0;
                    schedule.IsActive = true;

                    ScheduleConfirmedNotPaid--;
                    ScheduleConfirmedPaid++;
                    SchedulePresencial++;
                    ScheduleOnline = ScheduleConfirmedPaid - SchedulePresencial;

                    WeakReferenceMessenger.Default.Send(new DesinstancePagesExceptOneMessage(ManagerPage.Home.ToString()));
                    await Messenger.ShowToastMessage("Atendimento pago!");
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    await Messenger.ShowErrorMessageAsync(ex.Message, "Ops...");
                }
            });
        }
    }
}
