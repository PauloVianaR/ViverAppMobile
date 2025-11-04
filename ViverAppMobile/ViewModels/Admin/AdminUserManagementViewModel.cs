using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections;
using System.Collections.ObjectModel;
using ViverApp.Shared.DTos;
using ViverApp.Shared.Models;
using ViverAppMobile.Controls;
using ViverAppMobile.Models;
using ViverAppMobile.Services;
using ViverAppMobile.Workers;

namespace ViverAppMobile.ViewModels.Admin
{
    public partial class AdminUserManagementViewModel : ObservableObject, IViewModelInstancer
    {
        private readonly UserService userService;
        private readonly DoctorPropsService doctorPropService;
        private readonly ScheduleService scheduleService;
        private readonly AvailabilityDoctorService availabilityDoctorService;
        private bool isLoading = false;
        private UserDto[] allusers = [];
        private DoctorProp[] alldoctorprops = [];
        private List<ScheduleDto> schedulePendingOrConfirmed = [];
        private readonly Dictionary<UserType, IList> typesCollections = [];

        [ObservableProperty] private int usersCount = 0;
        [ObservableProperty] private int usersActive = 0;
        [ObservableProperty] private int pendingApprovals = 0;
        [ObservableProperty] private int premiumUsers = 0;
        [ObservableProperty] private int patientCount = 0;
        [ObservableProperty] private int doctorCount = 0;
        [ObservableProperty] private int managerCount = 0;
        [ObservableProperty] private int rejectedCount = 0;
        [ObservableProperty] private bool isReloading = false;
        [ObservableProperty] private string selectedUserStatus = string.Empty;
        [ObservableProperty] private string filterString = string.Empty;
        [ObservableProperty] private string selectedTab = "0";

        public List<string> UserStatusList { get; set; } = ["Todos os status", "Ativo", "Pendente Aprovação", "Rejeitado", "Bloqueado"];
        public ObservableCollection<AsyncModel<UserDto>> PendingUserApprovals { get; set; } = [];
        public ObservableCollection<AsyncModel<UserDto>> PatientsCollection { get; set; } = [];
        public ObservableCollection<AsyncModel<DoctorDto>> DoctorsCollection { get; set; } = [];
        public ObservableCollection<AsyncModel<UserDto>> ManagersCollection { get; set; } = [];
        public ObservableCollection<AsyncModel<UserDto>> RejectedUsers { get; set; } = [];

        public AdminUserManagementViewModel()
        {
            userService = new();
            doctorPropService = new();
            scheduleService = new();
            availabilityDoctorService = new();

            typesCollections.Add(UserType.Patient, PatientsCollection);
            typesCollections.Add(UserType.Manager, ManagersCollection);
            typesCollections.Add(UserType.Doctor, DoctorsCollection);

            SelectedUserStatus = UserStatusList[0];
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
                var userresp = await userService.GetAllAsync(getAll:true);
                if (!userresp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    userresp.ThrowIfIsNotSucess();
                }

                allusers = userresp?.Response?.ToArray() ?? [];

                var doctorPropsResp = await doctorPropService.GetAllAsync();
                if (!doctorPropsResp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    doctorPropsResp.ThrowIfIsNotSucess();
                }

                alldoctorprops = doctorPropsResp?.Response?.ToArray() ?? [];

                var scheduleResp = await scheduleService.GetScheduleAsync(id: 0, isDoctor: default, isHistoric: false, page: 0, pagesize: int.MaxValue);

                if (!scheduleResp.WasSuccessful)
                {
                    Master.GlobalToken.ThrowIfCancellationRequested();
                    scheduleResp.ThrowIfIsNotSucess();
                }

                schedulePendingOrConfirmed = scheduleResp?.Response?.ToList() ?? [];

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    UsersCount = allusers.Count(u => u.Status != (int)UserStatus.Rejected);
                    UsersActive = allusers.Count(u => u.Status == (int)UserStatus.Active);
                    PremiumUsers = allusers.Count(u => u.IsPremium == 1 && u.Status == (int)UserStatus.Active);

                    this.PopulateCollectionsFiltering(UserStatus.All);
                });

                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                isLoading = false;
            }
        }

        private void PopulateCollectionsFiltering(UserStatus status, string? filterString = null)
        {
            RejectedUsers.Clear();
            PendingUserApprovals.Clear();
            PatientsCollection.Clear();
            ManagersCollection.Clear();
            DoctorsCollection.Clear();

            static string NormalizeDigits(string? input)=> string.Concat(input?.Where(char.IsDigit) ?? []);

            bool MatchesFilter(UserDto u)
            {
                if (string.IsNullOrWhiteSpace(filterString))
                    return true;

                var f = filterString.Trim();
                var fDigits = NormalizeDigits(f);

                if (!string.IsNullOrEmpty(fDigits))
                {
                    return (NormalizeDigits(u.Cpf).Contains(fDigits))
                        || (NormalizeDigits(u.Fone).Contains(fDigits));
                }

                return (u.Name?.Contains(f, StringComparison.InvariantCultureIgnoreCase) ?? false)
                    || (u.Email?.Contains(f, StringComparison.InvariantCultureIgnoreCase) ?? false);
            }

            foreach (var u in allusers)
            {
                if(status != UserStatus.All)
                    if (u.Status != (int)status || !MatchesFilter(u))
                        continue;

                if (u.Status == (int)UserStatus.Rejected)
                {
                    RejectedUsers.Add(new AsyncModel<UserDto>(u));
                    continue;
                }

                if (u.Status == (int)UserStatus.PendingApproval)
                {
                    PendingUserApprovals.Add(new AsyncModel<UserDto>(u));
                    continue;
                }

                switch ((UserType)u.Usertype)
                {
                    case UserType.Patient:
                        PatientsCollection.Add(new AsyncModel<UserDto>(u) { IsActive = u.Status == (int)UserStatus.Active });
                        break;

                    case UserType.Manager:
                        ManagersCollection.Add(new AsyncModel<UserDto>(u) { IsActive = u.Status == (int)UserStatus.Active });
                        break;

                    case UserType.Doctor:
                        var docProp = alldoctorprops.FirstOrDefault(dp => dp.Iddoctor == u.IdUser);
                        if (docProp != null)
                        {
                            DoctorsCollection.Add(new AsyncModel<DoctorDto>(new DoctorDto(u, docProp))
                            {
                                IsActive = u.Status == (int)UserStatus.Active
                            });
                        }
                        break;
                }
            }

            PendingApprovals = allusers.Count(u => u.Status == (int)UserStatus.PendingApproval);
            RejectedCount = allusers.Count(u => u.Status == (int)UserStatus.Rejected);

            PatientCount = allusers.Count(u =>
                u.Usertype == (int)UserType.Patient
                && (u.Status == (int)UserStatus.Active || u.Status == (int)UserStatus.Blocked));

            DoctorCount = allusers.Count(u =>
                u.Usertype == (int)UserType.Doctor
                && (u.Status == (int)UserStatus.Active || u.Status == (int)UserStatus.Blocked));

            ManagerCount = allusers.Count(u =>
                u.Usertype == (int)UserType.Manager
                && (u.Status == (int)UserStatus.Active || u.Status == (int)UserStatus.Blocked));
        }

        [RelayCommand] private void SwitchSelectedTab(string tab) => SelectedTab = tab;
        [RelayCommand] private async Task ReloadPage()
        {
            if (IsReloading)
                return;
            IsReloading = true;
            await Loader.RunWithLoadingAsync(LoadAllAsync);
            IsReloading = false;
        }

        [RelayCommand] private void FilterByString()
        {
            int index = UserStatusList.IndexOf(SelectedUserStatus);
            UserStatus status = index <= 0 ? UserStatus.All : (UserStatus)index;

            this.PopulateCollectionsFiltering(status,FilterString);
        }

        [RelayCommand] private async Task ShowUsertTotInfo()
        {
            string msg = "Entram na contagem de 'Total' todos os usuários onde o cadastro NÃO foi REJEITADO.\nOu seja, os status BLOQUEADO e PENDENTE DE APROVAÇÃO são considerados neste total.";

            await Messenger.ShowMessage(msg, "Observações");
        }

        [RelayCommand] private async Task ShowUsersActiveTotInfo()
        {
            string msg = "Entram na contagem de 'Ativos' todos os usuários, desde que seu status NÃO seja BLOQUEADO, REJEITADO ou PENDENTE DE APROVAÇÃO";
            await Messenger.ShowMessage(msg, "Observações");
        }

        [RelayCommand]
        private async Task ApproveUser(AsyncModel<UserDto> user)
        {
            string rejectedMsg = user.Model.Status == (int)UserStatus.Rejected ? ", o qual havia sido REJEITADO anteriormente" : string.Empty;

            if (!await Messenger.ShowQuestionMessage($"Gostaria mesmo de APROVAR o usuário '{user.Model.Name}'{rejectedMsg}?", "Aprovar Usuário"))
                return;

            await this.ApproveOrReproveUser(user, isapproving: true);
        }

        [RelayCommand]
        private async Task ReproveUser(AsyncModel<UserDto> user)
        {
            if (!await Messenger.ShowQuestionMessage($"Gostaria mesmo de REJEITAR o cadastro do usuário '{user.Model.Name}'?", "Rejeitar Cadastro"))
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

                    if (PendingUserApprovals.Contains(baseuser))
                    {
                        PendingApprovals--;
                        PendingUserApprovals.Remove(baseuser);
                    }

                    string msg = isapproving ? "Cadastro Aprovado ✔" : "Cadastro Rejeitado ✖";

                    if (!isapproving)
                    {
                        UsersCount--;
                        RejectedCount++;
                        RejectedUsers.Add(baseuser);

                        await Messenger.ShowToastMessage(msg);
                        return;
                    }

                    if (RejectedUsers.Contains(baseuser))
                    {
                        RejectedCount--;
                        RejectedUsers.Remove(baseuser);
                    }

                    baseuser.IsActive = true;
                    var usertype = (UserType)model.Usertype;

                    if (usertype == UserType.Manager)
                    {
                        ManagersCollection.Add(baseuser);
                        ManagerCount++;
                    }
                    else if (usertype == UserType.Doctor)
                    {
                        var docProp = alldoctorprops.FirstOrDefault(dp => dp.Iddoctor == model.IdUser);
                        if (docProp is not null)
                        {
                            DoctorsCollection.Add(new AsyncModel<DoctorDto>(new DoctorDto(model, docProp)));
                            DoctorCount++;
                        }

                        var avResp = await availabilityDoctorService.CreateDefaultAsync(model.IdUser);
                        if (!avResp.WasSuccessful)
                        {
                            Master.GlobalToken.ThrowIfCancellationRequested();
                            avResp.ThrowIfIsNotSucess();
                        }
                    }

                    await Messenger.ShowToastMessage(msg);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    await Messenger.ShowErrorMessageAsync(ex.Message);
                }
            });
        }

        [RelayCommand] private async Task BlockOrUnblockUser(AsyncModel<UserDto> user)
        {
            await user.ExecuteAsync(async model =>
            {
                try
                {
                    bool blocking = model.Status != (int)UserStatus.Blocked;

                    if ((UserType)model.Usertype == UserType.Patient && blocking)
                    {
                        int scheduleCount = schedulePendingOrConfirmed.Count(s => s.IdPatient == model.IdUser);
                        if (scheduleCount > 0)
                        {
                            int scheduleConfirmedPaid = schedulePendingOrConfirmed.Count(s => s.IdPatient == model.IdUser && s.PendingPayment == 0 && s.Status == (int)ScheduleStatus.Confirmed);
                            string schedulepaidmsg = scheduleConfirmedPaid > 0 ? $", sendo que {scheduleConfirmedPaid} destes já estão PAGOS.\n(Obs: estes que já estão pagos serão estornados)" : string.Empty;

                            if (!await Messenger.ShowQuestionMessage($"Tem certeza que deseja bloquear o cadastro do paciente {model.Name}?\n\nIsso irá cancelar, de forma automática, todos os seus {scheduleCount} agendamentos pendentes ou confirmados{schedulepaidmsg}", "Confirmação"))
                                return;
                        }                        
                    }

                    model.Status = blocking ? (int)UserStatus.Blocked : (int)UserStatus.Active;
                    var resp = await userService.UpdateAsync(model.IdUser, model);
                    if (!resp.WasSuccessful)
                    {
                        Master.GlobalToken.ThrowIfCancellationRequested();
                        resp.ThrowIfIsNotSucess();
                    }

                    user.IsActive = !blocking;

                    if ((UserType)model.Usertype == UserType.Doctor)
                        return;

                    int indexStatusFilter = UserStatusList.IndexOf(SelectedUserStatus);
                    if (blocking && (indexStatusFilter == 1)
                    || (!blocking && (indexStatusFilter == 2)))
                    {
                        typesCollections[(UserType)model.Usertype].Remove(user);
                    }

                    UsersActive += blocking ? -1 : 1;
                    WeakReferenceMessenger.Default.Send(new UsersActiveChanged(model));

                    await this.CancelSchedule(model.IdUser, isdoctor: false);
                }
                catch (OperationCanceledException) { }
                catch(Exception ex)
                {
                    await Messenger.ShowErrorMessageAsync(ex.Message);
                }
            });
        }

        [RelayCommand] private async Task BlockOrUnblockDoctor(AsyncModel<DoctorDto> doctor)
        {
            await doctor.ExecuteAsync(async model =>
            {
                try
                {
                    var userdoc = allusers.FirstOrDefault(u => u.IdUser == model.IdUser);
                    if (userdoc is null)
                        return;

                    if ((UserType)model.Usertype != UserType.Doctor)
                        return;

                    bool blocking = userdoc.Status == (int)UserStatus.Blocked;

                    int scheduleCount = schedulePendingOrConfirmed.Count(s => s.Iddoctor == userdoc.IdUser);
                    int patientsScheduleCount = schedulePendingOrConfirmed
                        .Where(s => s.Iddoctor == model.IdUser)
                        .DistinctBy(s => s.IdPatient)
                        .Count();

                    if (scheduleCount > 0 && patientsScheduleCount > 0 && blocking)
                    {
                        if (!await Messenger.ShowQuestionMessage($"Tem certeza que deseja bloquear o usuário de {model.Title} {model.Name}?" +
                            $"\n\nAo fazer isso você irá, de forma automática, cancelar todas os atendimentos pendentes e confirmados para este especialista, sendo um total de {scheduleCount}, o que irá afetar a agenda de {patientsScheduleCount} paciente(s)", "Confirmação"))
                            return;
                    }

                    userdoc.Status = blocking ? (int)UserStatus.Blocked : (int)UserStatus.Active;
                    var resp = await userService.UpdateAsync(model.IdUser, userdoc);
                    if (!resp.WasSuccessful)
                    {
                        Master.GlobalToken.ThrowIfCancellationRequested();
                        resp.ThrowIfIsNotSucess();
                    }

                    doctor.IsActive = !blocking;

                    int indexStatusFilter = UserStatusList.IndexOf(SelectedUserStatus);
                    if (blocking && (indexStatusFilter == 1)
                    || (!blocking && (indexStatusFilter == 2)))
                    {
                        typesCollections[UserType.Doctor].Remove(doctor);
                    }

                    UsersActive += blocking ? -1 : 1;
                    WeakReferenceMessenger.Default.Send(new UsersActiveChanged(userdoc));

                    await this.CancelSchedule(userdoc.IdUser, isdoctor:true);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    await Messenger.ShowErrorMessageAsync(ex.Message);
                }
            });
        }

        private async Task CancelSchedule(int id, bool isdoctor)
        {
            var schedules = schedulePendingOrConfirmed.Where(s => (isdoctor && s.Iddoctor == id) || (!isdoctor && s.IdPatient == id)).ToList();
            if (schedules.Count == 0)
                return;

            List<ScheduleUpdateDto> schedulesUpdate = [];
            schedules.ForEach(s =>
            {
                var docprop = alldoctorprops.FirstOrDefault(dp => dp.Iddoctor == id);
                string docTitle = docprop is null ? "Dr(a)" : docprop?.Title ?? "Dr(a)";

                string referenceMsg = isdoctor
                ? $"do especialista {docTitle} {s.DoctorName} foi bloqueado e, por isso, todos os atendimentos dele(a) foram cancelados, incluindo este."
                : $"do paciente {s.UserName} foi bloqueado e, por isso, todos os agendamentos feitos por ele(a) foram cancelados, incluindo este.";

                int? oldstatus = s.Status;
                s.FeedBack = $"O cadastro {referenceMsg}";
                s.Status = (int)ScheduleStatus.Canceled;
                s.PendingPayment = 0;
                ScheduleUpdateDto scheduleUpdate = new(s, (int)UserType.Admin, string.Empty, oldstatus);
                schedulesUpdate.Add(scheduleUpdate);
                schedulePendingOrConfirmed.Remove(s);
            });

            var resp = await scheduleService.UpdateManyAsync(schedulesUpdate);
            if (!resp.WasSuccessful)
            {
                Master.GlobalToken.ThrowIfCancellationRequested();
                resp.ThrowIfIsNotSucess();
            }
        }

        partial void OnSelectedUserStatusChanged(string value)
        {
            int index = UserStatusList.IndexOf(value);
            if (index < 0)
                return;

            this.PopulateCollectionsFiltering((UserStatus)index);
        }
    }
}
