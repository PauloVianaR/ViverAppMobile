using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Globalization;
using ViverApp.Shared.DTos;
using ViverApp.Shared.Models;
using ViverAppMobile.Controls;
using ViverAppMobile.Models;
using ViverAppMobile.Services;
using ViverAppMobile.Views.Popups;
using ViverAppMobile.Views.Admin;
using ViverAppMobile.Helpers;
using ViverAppMobile.Workers;

namespace ViverAppMobile.ViewModels.Admin
{
    public partial class AdminHomeViewModel : ObservableObject, IViewModelInstancer
    {
        private readonly UserService userService;
        private readonly DoctorPropsService doctorPropsService;
        private readonly ScheduleService scheduleService;
        private readonly PaymentService paymentService;
        private readonly AvailabilityDoctorService availabilityDoctorService;

        [ObservableProperty] private int usersActiveTot = 0;
        [ObservableProperty] private int scheduleTodayTot = 0;
        [ObservableProperty] private int patientTot = 0;
        [ObservableProperty] private int premiumTot = 0;
        [ObservableProperty] private int pendingTot = 0;
        [ObservableProperty] private int pendingApprovals = 0;
        [ObservableProperty] private int pendingPayments = 0;
        [ObservableProperty] private bool isLoading = false;
        [ObservableProperty] private bool isReloading = false;
        [ObservableProperty] private double premiumUsersPercent = 0;
        [ObservableProperty] private decimal monthlyIncome = 0;

        public ObservableCollection<AsyncModel<UserDto>> PendingUserApprovals { get; set; } = [];
        public ObservableCollection<AsyncModel<DoctorDto>> AllDoctors { get; set; } = [];
        public ObservableCollection<AsyncModel<ScheduleDto>> PendingPaymentAppointments { get; set; } = [];

        public AdminHomeViewModel()
        {
            userService = new();
            doctorPropsService = new();
            scheduleService = new();
            paymentService = new();
            availabilityDoctorService = new();

            WeakReferenceMessenger.Default.Register<UsersActiveChanged>(this, (r, m) => this.UpdateUsersActiveCount(m.Value));
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
                Master.GlobalToken.ThrowIfCancellationRequested();

                var usersResp = await userService.GetAllAsync(getBlocked: false, getRejected: false, getPendingApproval: true);
                if (!usersResp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    throw new Exception(usersResp.ResponseErr);
                }

                var users = usersResp?.Response?.ToList() ?? [];
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    UsersActiveTot = users.Count(u => u.Status == (int)UserStatus.Active);
                    PremiumTot = users.Where(u => u.IsPremium == (sbyte)1 && u.Usertype == (int)UserType.Patient).Count();

                    PatientTot = users.Count(u => u.Usertype == (int)UserType.Patient && u.Status == (int)UserStatus.Active);
                    PremiumUsersPercent = Math.Round((double)(PremiumTot * 100) / PatientTot, 2);

                    var usersPending = users.Where(u => u.Status == (int)UserStatus.PendingApproval).ToList();
                    usersPending.ForEach(u => PendingUserApprovals.Add(new AsyncModel<UserDto>(u)));
                    PendingApprovals = usersPending.Count;
                });

                var doctorPropsResp = await doctorPropsService.GetAllAsync();
                if (!doctorPropsResp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    doctorPropsResp.ThrowIfIsNotSucess();
                }
                var doctorProps = doctorPropsResp?.Response?.ToList() ?? [];


                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    var doctors = users.Where(u => u.Usertype == (int)UserType.Doctor).ToList();
                    doctors.ForEach(d =>
                    {
                        var doctorprop = doctorProps.FirstOrDefault(dp => dp.Iddoctor == d.IdUser);
                        if (doctorprop is not null)
                        {
                            DoctorDto docDto = new(d, doctorprop);
                            AllDoctors.Add(new AsyncModel<DoctorDto>(docDto) { IsActive = docDto.Attendonline == 1 });
                        }
                    });
                });

                var scheduleResp = await scheduleService.GetScheduleAsync(0, isDoctor: default, isHistoric: false, page: 0, pagesize: int.MaxValue);
                if (!scheduleResp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    throw new Exception(scheduleResp.ResponseErr);
                }

                var schedules = scheduleResp?.Response?.ToList() ?? [];
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    ScheduleTodayTot = schedules.Where(s => s.AppointmentDate.GetValueOrDefault().Date == DateTime.Today.Date).Count();

                    var pendingPayments = schedules.Where(s => s.Status == (int)ScheduleStatus.Confirmed && s.PendingPayment == (sbyte)1);
                    pendingPayments.OrderBy(p => p.AppointmentDate).ToList().ForEach(s =>
                    {
                        PendingPaymentAppointments.Add(new AsyncModel<ScheduleDto>(s));
                        PendingPayments++;
                    });

                    PendingTot = PendingApprovals + PendingPayments;
                });

                var paymentsResp = await paymentService.GetPaymentsByMonths();
                if (!paymentsResp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    throw new Exception(paymentsResp.ResponseErr);
                }

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    paymentsResp?.Response?.ToList().ForEach(s => MonthlyIncome += s.Paidprice ?? 0);
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

        [RelayCommand] private async Task ReloadPage()
        {
            if (IsReloading)
                return;
            IsReloading = true;
            await Loader.RunWithLoadingAsync(LoadAllAsync);
            IsReloading = false;
        }

        [RelayCommand] private void SwitchPage(int pageindex)
        {
            Navigator.SwitchAdminPage((AdminPage)pageindex);
        }

        private void UpdateUsersActiveCount(UserDto changedUser)
        {
            bool blocked = changedUser.Status == (int)UserStatus.Blocked;
            UsersActiveTot += blocked ? -1 : 1;

            if (changedUser.Usertype == (int)UserType.Patient)
            {
                PatientTot += blocked ? -1 : 1;
                PremiumUsersPercent = Math.Round((double)(PremiumTot * 100) / PatientTot, 2);
            }
        }

        [RelayCommand] private async Task NavigateToPremiumPage()
        {
            await Navigator.PushNavigationAsync(new AdminPremiumManagementPage());
        }

        [RelayCommand] private async Task ApproveUser(AsyncModel<UserDto> user)
        {
            if (!await Messenger.ShowQuestionMessage($"Gostaria mesmo de APROVAR o usuário {user.Model.Name}?","Aprovar Usuário"))
                return;

            await this.ApproveOrReproveUser(user, isapproving:true);
        }

        [RelayCommand] private async Task ReproveUser(AsyncModel<UserDto> user)
        {
            if (!await Messenger.ShowQuestionMessage($"Gostaria mesmo de REJEITAR o cadastro do usuário {user.Model.Name}?","Rejeitar usuário"))
                return;

            await this.ApproveOrReproveUser(user, isapproving: false);
        }

        private async Task ApproveOrReproveUser(AsyncModel<UserDto> baseuser, bool isapproving)
        {
            await baseuser.ExecuteAsync(async model =>
            {
                try
                {
                    model.Status = isapproving ? (int)UserStatus.Active : (int)UserStatus.Rejected;
                    var resp = await userService.UpdateAsync(model.IdUser, model);
                    if (!resp.WasSuccessful)
                    {
                        Master.GlobalToken.ThrowIfCancellationRequested();
                        resp.ThrowIfIsNotSucess();
                    }

                    if (baseuser is AsyncModel<UserDto> asyncUser)
                        PendingUserApprovals.Remove(asyncUser);

                    if (!isapproving && model.Usertype == (int)UserType.Doctor)
                    {
                        var docRemove = AllDoctors.FirstOrDefault(d => d.Model.IdUser == model.IdUser);
                        {
                            docRemove.IsBusy = true;

                            if (docRemove is not null)
                                AllDoctors.Remove(docRemove);
                        }
                    }
                    else if(isapproving && model.Usertype == (int)UserType.Doctor)
                    {
                        var docUpdate = AllDoctors.FirstOrDefault(d => d.Model.IdUser == model.IdUser);
                        if (docUpdate is not null)
                            docUpdate.Model.Status = (int)UserStatus.Active;

                        var avResp = await availabilityDoctorService.CreateDefaultAsync(model.IdUser);
                        if (!avResp.WasSuccessful)
                        {
                            Master.GlobalToken.ThrowIfCancellationRequested();
                            avResp.ThrowIfIsNotSucess();
                        }
                    }

                    PendingApprovals -= 1;
                    PendingTot -= 1;
                    string msg = isapproving ? "Cadastro Aprovado ✔" : "Cadastro Rejeitado ✖";
                    await Messenger.ShowToastMessage(msg);
                }
                catch (OperationCanceledException) { }
                catch(Exception ex)
                {
                    await Messenger.ShowErrorMessageAsync(ex.Message);
                }
            });
        }

        [RelayCommand] private async Task LockOrUnlockAttendOnlineDoctor(AsyncModel<DoctorDto> model)
        {
            if (model.Model.Status != (int)UserStatus.Active)
            {
                await Messenger.ShowErrorMessageAsync("Para permitir que este médico atenda online, primeiro aprove seu cadastro na lista de Aprovações Pendentes.", "Ops...");

                return;
            }

            await model.ExecuteAsync(async doctor =>
            {
                try
                {
                    doctor.Attendonline = model.IsActive ? (sbyte)0 : (sbyte)1;
                    model.IsActive = !model.IsActive;
                    var props = new DoctorProp(doctor);
                    var resp = await doctorPropsService.UpdateAsync(props.Iddoctorprops, props);
                    if (!resp.WasSuccessful)
                    {
                        Master.GlobalToken.ThrowIfCancellationRequested();
                        resp.ThrowIfIsNotSucess();
                    }
                }
                catch (OperationCanceledException) { }
                catch(Exception ex)
                {
                    await Messenger.ShowErrorMessageAsync(ex.Message);
                }
            });
        }

        [RelayCommand] private async Task ConfirmPayment(AsyncModel<ScheduleDto> schedule)
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

                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        PendingPaymentAppointments.Remove(schedule);
                        PendingPayments -= 1;
                        PendingTot -= 1;

                        var paydate = paymentresp?.Response?.Paidday ?? DateTime.MinValue;
                        if(paydate.Month == DateTime.Today.Month && paydate != DateTime.MinValue)
                            MonthlyIncome += model.AppointmentPrice ?? 0m;

                        WeakReferenceMessenger.Default.Send(new DesinstancePagesExceptOneMessage(AdminPage.Home.ToString()));
                    });
                    await Messenger.ShowToastMessage("Atendimento pago!");
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    await Messenger.ShowErrorMessageAsync(ex.Message, "Ops...");
                }
            });
        }

        [RelayCommand] private async Task CancelSchedule(AsyncModel<ScheduleDto> schedule)
        {
            ValueBunker<bool>.SavedValue = true;
            var returnValue = await PopupHelper<ScheduleDto>.PushInstanceAsync<CancelSchedulePopup>(schedule.Model);
            if (!returnValue)
                return;

            await schedule.ExecuteAsync(async model =>
            {
                try
                {
                    int? oldstatus = model.Status;
                    model.Status = (int)ScheduleStatus.Canceled;
                    model.PendingPayment = 0;
                    model.MedicalReport = null;
                    model.Rating = null;
                    model.FeedBack = PopupHelper<string>.GetValue() + "\n[CANCELADO PELO ADMINISTRADOR]";

                    var resp = await scheduleService.UpdateAsync(new ScheduleUpdateDto(model, userTypeUpd:(int)UserType.Admin, string.Empty, oldstatus));
                    if (!resp.WasSuccessful)
                    {
                        Master.GlobalToken.ThrowIfCancellationRequested();
                        resp.ThrowIfIsNotSucess();
                    }

                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        PendingPaymentAppointments.Remove(schedule);
                        PendingPayments -= 1;
                        PendingTot -= 1;
                        WeakReferenceMessenger.Default.Send(new DesinstancePagesExceptOneMessage(AdminPage.Home.ToString()));
                    });
                    await Messenger.ShowToastMessage("Atendimento cancelado!");
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
