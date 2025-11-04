using System.Net;
using System.Net.Http.Json;
using ViverApp.Shared.DTos;
using ViverApp.Shared.Models;
using ViverApp.Shared.Utils;
using ViverAppMobile.Handlers;
using ViverAppMobile.Helpers;
using ViverAppMobile.Workers;

namespace ViverAppMobile.Services
{
    public class ScheduleService : Service<Schedule>
    {
        public const string endPoint = $"{baseApiPoint}/Schedule";
        public ScheduleService() : base(endPoint) { }

        /// <summary>
        /// Get all schedules with pagination.
        /// For ignore user and doctor set <paramref name="id"/> = 0
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isDoctor"></param>
        /// <param name="isHistoric"></param>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <param name="filterStatus"></param>
        /// <param name="filterString"></param>
        /// <returns></returns>
        public async Task<ResponseHandler<IEnumerable<ScheduleDto>>> GetScheduleAsync(int id, bool isDoctor, bool isHistoric, int page = 0, int pagesize = 10, int filterStatus = default, string filterString = "", DateTime initialdate = default, DateTime finaldate = default, int modalityfilter = 0, int appointmenttypefilter = 0)
        {
            ResponseHandler <IEnumerable<ScheduleDto>> resp = new();
            try
            {
                string endPointCompletion = isHistoric ? "historic" : "nextSchedules";

                string URI = $"{endPoint}/{endPointCompletion}/{id}?" +
                    $"isDoctor={isDoctor}" +
                    $"&page={page}" +
                    $"&pagesize={pagesize}" +
                    $"&filterStatus={filterStatus}" +
                    $"&filterString={filterString}" +
                    $"&initialdate={initialdate:yyyy-MM-dd}" +
                    $"&finaldate={finaldate:yyyy-MM-dd}" +
                    $"&modalityfilter={modalityfilter}" +
                    $"&appointmenttypefilter={appointmenttypefilter}";

                var httpResp = await HttpClient.GetAsync(URI);
                if (!httpResp.IsSuccessStatusCode)
                    throw new Exception(await httpResp.Content.ReadAsStringAsync());

                var data = await httpResp.Content.ReadFromJsonAsync<IEnumerable<ScheduleDto>>() ?? [];
                resp.IsSuccess(data);
            }
            catch (Exception ex)
            {
                resp.IsNotSuccess(ex.Message);
            }
            return resp;
        }

        public async Task<ResponseHandler<IEnumerable<ScheduleDto>>> GetScheduleByMonthsAsync(int id, bool isDoctor, bool isHistoric, int months, bool isMonthsBefore = false)
        {
            var startDate = DateTimeHelper.GetFirstDayThisMonth().AddMonths(-months + (months == 1 ? 1 : 0));
            var endDate = DateTime.Today.AddDays(1);

            if (isMonthsBefore)
            {
                startDate = startDate.AddMonths(-months + (months == 1 ? 1 : 0));

                var date = DateTimeHelper.GetTodayLastTime();
                endDate = DateTimeHelper.GetLastDayOfMonth(date.Year, date.Month);
            }

            return await this.GetScheduleAsync(id, isDoctor, isHistoric, page:0, pagesize:int.MaxValue, filterStatus:(int)ScheduleStatus.Concluded, filterString:string.Empty, startDate, endDate);
        }

        public async Task<ResponseHandler<int>> GetScheduleCountAsync(int id, bool isDoctor = false, bool countingHistoric = false, int filterStatus = 0, string filterString = "", DateTime initialdate = default, DateTime finaldate = default)
        {
            ResponseHandler<int> resp = new();
            try
            {
                string URI = $"{endPoint}/count/{id}?" +
                    $"isDoctor={isDoctor}" +
                    $"&countingHistoric={countingHistoric}" +
                    $"&filterStatus={filterStatus}" +
                    $"&filterString={filterString}" +
                    $"&initialdate={initialdate:yyyy-MM-dd}" +
                    $"&finaldate={finaldate:yyyy-MM-dd}";

                var httpResp = 
                    await HttpClient.GetAsync(URI);
                if (!httpResp.IsSuccessStatusCode)
                    throw new Exception(await httpResp.Content.ReadAsStringAsync());

                var data = await httpResp.Content.ReadFromJsonAsync<int>();
                resp.IsSuccess(data);
            }
            catch(Exception ex)
            {
                resp.IsNotSuccess(ex.Message);
            }

            return resp;
        }

        public async override Task<ResponseHandler<Schedule>> CreateAsync(Schedule entity)
        {
            ResponseHandler<Schedule> resp = new();
            try
            {
                var httpResp = await HttpClient.PostAsJsonAsync(endPoint, new ScheduleCreateDto
                {
                    Idappointment = entity.Idappointment,
                    Idclinic = entity.Idclinic,
                    Iddoctor = entity.Iddoctor,
                    Iduser = entity.Iduser,
                    Status = entity.Status,
                    AppointmentDate = entity.Appointmentdate,
                    Obs = entity.Obs,
                    Rescheduled = entity.Rescheduled,
                    IsOnline = entity.Isonline,
                    OriginalDate = entity.Originaldate,
                    PendingPayment = entity.Pendingpayment
                });
                if (!httpResp.IsSuccessStatusCode)
                    throw new Exception(await httpResp.Content.ReadAsStringAsync());

                var data = await httpResp.Content.ReadFromJsonAsync<Schedule>() ?? new();
                resp.IsSuccess(data);
            }
            catch (Exception ex)
            {
                resp.IsNotSuccess(ex.Message);
            }

            return resp;
        }

        public async Task<ResponseHandler<bool>> UpdateAsync(ScheduleUpdateDto schedule)
        {
            ResponseHandler<bool> resp = new();
            try
            {
                var httpResp = await HttpClient.PatchAsJsonAsync($"{endPoint}/{schedule.Idschedule}", schedule);
                if (!httpResp.IsSuccessStatusCode)
                    throw new Exception(await httpResp.Content.ReadAsStringAsync());

                resp.IsSuccess(true);

                this.SendUpdateNotification(schedule);
            }
            catch(Exception ex)
            {
                resp.IsNotSuccess(ex.Message);
            }

            return resp;
        }

        public async Task<ResponseHandler<bool>> UpdateManyAsync(IEnumerable<ScheduleUpdateDto> schedules)
        {
            ResponseHandler<bool> resp = new();
            try
            {
                var httpResp = await HttpClient.PatchAsJsonAsync($"{endPoint}/updatemany", schedules);
                if (!httpResp.IsSuccessStatusCode)
                    throw new Exception(await httpResp.Content.ReadAsStringAsync());

                resp.IsSuccess(true);

                schedules.ToList().ForEach(s => this.SendUpdateNotification(s));
            }
            catch (Exception ex)
            {
                resp.IsNotSuccess(ex.Message);
            }

            return resp;
        }

        private void SendUpdateNotification(ScheduleUpdateDto schedule)
        {
            if (string.IsNullOrWhiteSpace(schedule.DoctorName) || string.IsNullOrWhiteSpace(schedule.PacientName))
                return;

            if (schedule.Status == schedule.OldStatus || schedule.OldStatus is null)
                return;

            //this case oldstatus --> newstatus
            if (schedule.Rescheduled == 1 && schedule.OldStatus == (int)ScheduleStatus.Rescheduled)
            {
                string title = EnumTranslator.TranslateNotificationType(NotificationType.RescheduledAppointment)
                    + " pelo " + EnumTranslator.TranslateUserType((UserType)schedule.UserTypeUpdated).ToLower();

                if (schedule.UserTypeUpdated == (int)UserType.Manager && !string.IsNullOrWhiteSpace(schedule.UserNameUpdated))
                    title += $" {schedule.UserNameUpdated}";

                string description = $"Médico: {schedule.DoctorName}\nPaciente: {schedule.PacientName}\nData Original: {schedule.Appointmentdate:dd/MM/yyyy} às {schedule.Appointmentdate:HH:mm}\nNova Data: {schedule.OriginalDate:dd/MM/yyyy} às {schedule.OriginalDate:HH:mm}";

                if (schedule.UserNameUpdated != schedule.PacientName)
                {
                    string pushdescription = $"{schedule.PacientName}, seu atendimento com {schedule.DoctorName} que estava marcado para {schedule.OriginalDate:dd/MM/yyyy' às 'HH:mm} foi reagendado para {schedule.Appointmentdate:dddd', 'dd/MM/yyyy' às 'HH:mm}. Acesse o aplicativo Viver para mais detalhes.";

                    Notificator.Send(NotificationType.RescheduledAppointment, description, title, schedule.IdPacient, pushdescription);
                }
                else
                    Notificator.Send(NotificationType.RescheduledAppointment, description, title);

                return;
            }

            if (schedule.Status == (int)ScheduleStatus.Confirmed && schedule.PendingPayment == 1)
            {
                string description = $"Médico: {schedule.DoctorName}\nPaciente: {schedule.PacientName}\nData: {schedule.Appointmentdate:dd/MM/yyyy} às {schedule.Appointmentdate:HH:mm} \nTipo: {schedule.AppointmentTitle} ({schedule.AppointmentPrice:c2})";

                string pushdescription = $"{schedule.PacientName}, seu atendimento foi confirmado com sucesso, porém o pagamento ainda está pendente.\nNão se esqueça de pagá-lo na recepção no dia marcado 😉";

                Notificator.Send(NotificationType.PendingPayment, description, string.Empty, schedule.IdPacient, pushdescription);

                return;
            }

            if (schedule.Status == (int)ScheduleStatus.Canceled)
            {
                string title = EnumTranslator.TranslateNotificationType(NotificationType.CanceledAppointment)
                    + " pelo " + EnumTranslator.TranslateUserType((UserType)schedule.UserTypeUpdated).ToLower();

                if (schedule.UserTypeUpdated == (int)UserType.Manager && !string.IsNullOrWhiteSpace(schedule.UserNameUpdated))
                    title += $" {schedule.UserNameUpdated}";

                string isonlineinfo = schedule.IsOnline == 1 ? " (Online)" : string.Empty;

                string description = $"Médico: {schedule.DoctorName}\nPaciente: {schedule.PacientName}\nAtendimento:[{EnumTranslator.TranslateAppointmentType(schedule.AppointmentType)}] {schedule.AppointmentTitle} {isonlineinfo}\nMotivo Cancelamento: {schedule.Feedback}";

                if (schedule.UserNameUpdated != schedule.PacientName)
                {
                    string pushdescription = $"{schedule.PacientName}, lamentamos em informar que seu atendimento marcado para o dia {schedule.Appointmentdate:dd/MM/yyy} com {schedule.DoctorName} foi cancelado.\nConsulte o aplicativo para saber mais informações.";

                    Notificator.Send(NotificationType.CanceledAppointment, description, title, schedule.IdPacient, pushdescription);                    
                }
                else
                    Notificator.Send(NotificationType.CanceledAppointment, description, title);

                return;
            }

            if (schedule.Status == (int)ScheduleStatus.Concluded)
            {
                string title = EnumTranslator.TranslateNotificationType(NotificationType.FinishedAppointment)
                    + " pelo " + EnumTranslator.TranslateUserType((UserType)schedule.UserTypeUpdated).ToLower();

                if (schedule.UserTypeUpdated == (int)UserType.Manager && !string.IsNullOrWhiteSpace(schedule.UserNameUpdated))
                    title += $" {schedule.UserNameUpdated}";

                string description = $"Médico: {schedule.DoctorName}\nPaciente: {schedule.PacientName}\nAtendimento:[{EnumTranslator.TranslateAppointmentType(schedule.AppointmentType)}]";

                string pushdescription = $"Parabéns ⭐⭐, {schedule.PacientName}, seu atendimento {EnumTranslator.TranslateAppointmentType(schedule.AppointmentType)} com {schedule.DoctorName} foi concluído com sucesso! Pedimos que siga as orientações passadas pelo especialista e, em caso de dúvidas, ligue para nós! Agradeçemos a preferência e esperamos que fique bem! 🤩";

                Notificator.Send(NotificationType.FinishedAppointment, description, title, schedule.IdPacient, pushdescription);
            }
        }
    }
}
