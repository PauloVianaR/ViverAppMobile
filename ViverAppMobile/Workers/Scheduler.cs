using ViverApp.Shared.DTos;
using ViverApp.Shared.Models;
using AppointmentType = ViverAppMobile.Models.AppointmentType;

namespace ViverAppMobile.Workers
{
    public class DateTimeSlot
    {
        public DateTime Date { get; set; }
        public List<TimeSlot> AvailableTimes { get; set; } = [];
    }
    public class TimeSlot
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }

    /// <summary>
    /// Classe responsável por calcular os dias e horários possíveis para o agendamento considerando a disponibilidade do médico, da clínica (se não for consulta online) considerando feriados nacionais e sempre verificando se não existe algo já agendado.
    /// </summary>
    /// <param name="availabilityDoctors"></param>
    /// <param name="availabilityClinic"></param>
    /// <param name="scheduleDtos"></param>
    /// <param name="_holidays"></param>
    /// <param name="minConsultation"></param>
    /// <param name="minExamination"></param>
    /// <param name="minSurgery"></param>
    /// <param name="intervalMinAppnt"></param>
    public class Scheduler(IEnumerable<AvailabilityDoctor> availabilityDoctors, IEnumerable<AvailabilityClinic> availabilityClinic, IEnumerable<ScheduleDto> scheduleDtos, IEnumerable<Holiday> _holidays,int minConsultation, int minExamination, int minSurgery, TimeOnly intervalMinAppnt)
    {
        public List<AvailabilityDoctor> DoctorsAvailabilities { get; set; } = availabilityDoctors.ToList();
        public List<AvailabilityClinic> AvailabilityClinic { get; set; } = availabilityClinic.ToList();
        public List<ScheduleDto> ExistingAppointments { get; set; } = scheduleDtos.ToList();
        public List<Holiday> Holidays { get; set; } = _holidays.ToList();
        public int MinConsultationTime { get; set; } = minConsultation;
        public int MinExaminationTime { get; set; } = minExamination;
        public int MinSurgeryTime { get; set; } = minSurgery;
        public TimeOnly IntervalMin { get; set; } = intervalMinAppnt;

        private DateTime GetNextClinicOpenDay(DateTime fromDate)
        {
            var date = fromDate.Date.AddDays(1);
            var limit = fromDate.Date.AddYears(1);
            var clinicOpenDays = AvailabilityClinic.Select(c => c.Daytype).Distinct().ToHashSet();

            while (date <= limit)
            {
                if (clinicOpenDays.Contains((int)date.DayOfWeek))
                    return date;
                date = date.AddDays(1);
            }
            return DateTime.MinValue;
        }

        public List<TimeSlot> GetDayAvailableTimes(DateTime date,Appointment appnt,AvailabilityDoctor doctorAvailability,AvailabilityClinic clinicAvailability)
        {
            var availableSlots = new List<TimeSlot>();

            int averageTime = (AppointmentType)appnt.Idappointmenttype switch
            {
                AppointmentType.Consultation => MinConsultationTime,
                AppointmentType.Examination => MinExaminationTime,
                AppointmentType.Surgery => MinSurgeryTime,
                _ => MinConsultationTime
            };

            if (appnt.Averagetime != TimeOnly.MinValue)
            {
                averageTime = appnt.Averagetime.GetValueOrDefault().Hour * 60
                            + appnt.Averagetime.GetValueOrDefault().Minute;
            }

            int intervalMinutes = IntervalMin.Hour * 60 + IntervalMin.Minute;

            DateTime start = date.Date.Add(doctorAvailability?.Starttime.GetValueOrDefault().ToTimeSpan() ?? TimeSpan.MinValue);
            DateTime end = date.Date.Add(doctorAvailability?.Endtime.GetValueOrDefault().ToTimeSpan() ?? TimeSpan.MinValue);

            if (!(appnt.Idappointmenttype == (int)AppointmentType.Consultation && doctorAvailability.Isonline == 1))
            {
                if (clinicAvailability == null)
                    return availableSlots;

                DateTime clinicStart = date.Date.Add(clinicAvailability?.Starttime.GetValueOrDefault().ToTimeSpan() ?? TimeSpan.MinValue);
                DateTime clinicEnd = date.Date.Add(clinicAvailability?.Endtime.GetValueOrDefault().ToTimeSpan() ?? TimeSpan.MinValue);

                if (clinicStart > start) start = clinicStart;
                if (clinicEnd < end) end = clinicEnd;
            }

            var dayAppointments = ExistingAppointments
                .Where(a => a.AppointmentDate == date.Date)
                .OrderBy(a => a.AppointmentDate)
                .ToList();

            DateTime current = start;

            foreach (var appt in dayAppointments)
            {
                DateTime apptStart = appt.AppointmentDate ?? DateTime.MinValue;
                DateTime apptEnd = apptStart.AddMinutes(
                    appt.AverageTime.GetValueOrDefault().Hour * 60 +
                    appt.AverageTime.GetValueOrDefault().Minute +
                    intervalMinutes);

                while (current.AddMinutes(averageTime) <= apptStart)
                {
                    availableSlots.Add(new TimeSlot
                    {
                        Start = current,
                        End = current.AddMinutes(averageTime)
                    });

                    current = current.AddMinutes(averageTime + intervalMinutes);
                }

                if (current < apptEnd)
                    current = apptEnd;
            }

            while (current.AddMinutes(averageTime) <= end)
            {
                availableSlots.Add(new TimeSlot
                {
                    Start = current,
                    End = current.AddMinutes(averageTime)
                });

                current = current.AddMinutes(averageTime + intervalMinutes);
            }

            return availableSlots;
        }

        public List<DateTimeSlot> GetAvailableSlots(int idDoctor, DateTime startDate, Appointment appointment, int monthsToCalculate = 1)
        {
            var result = new List<DateTimeSlot>();
            DateTime datePointer = new(startDate.Year, startDate.Month, 1);

            var doctorSlotsAll = DoctorsAvailabilities.Where(doc => doc.Iddoctor == idDoctor).ToList();

            for (int m = 0; m < monthsToCalculate; m++)
            {
                int daysInMonth = DateTime.DaysInMonth(datePointer.Year, datePointer.Month);

                for (int d = 1; d <= daysInMonth; d++)
                {
                    var date = new DateTime(datePointer.Year, datePointer.Month, d);
                    int dayOfWeek = (int)date.DayOfWeek;

                    var doctorSlots = doctorSlotsAll.Where(doc => doc.Daytype == dayOfWeek);
                    if (!doctorSlots.Any()) continue;

                    var clinicSlots = AvailabilityClinic.Where(c => c.Daytype == dayOfWeek);
                    var daySlot = new DateTimeSlot { Date = date };

                    foreach (var docSlot in doctorSlots)
                    {
                        bool isOnline = docSlot.Isonline == 1;
                        if (!isOnline && !clinicSlots.Any())
                            continue;

                        foreach (var clinicSlot in isOnline ? [] : clinicSlots)
                        {
                            var slots = GetDayAvailableTimes(date, appointment, docSlot, clinicSlot);
                            daySlot.AvailableTimes.AddRange(slots);
                        }
                    }

                    if (daySlot.AvailableTimes.Count != 0)
                        result.Add(daySlot);
                }

                datePointer = datePointer.AddMonths(1);
            }

            return result;
        }

        public List<DateTime> GetUnavailableDays(int idDoctor, bool isOnline, DateTime startDate)
        {
            var unavailableDays = new List<DateTime>();
            DateTime endDate = startDate.Date.AddYears(1);
            DateTime currentDate = startDate.AddDays(1);

            var doctorAvailabilities = DoctorsAvailabilities
                .Where(a => a.Iddoctor == idDoctor && a.Isonline == 1 == isOnline)
                .ToList();

            var doctorDaysOfWeek = doctorAvailabilities.Select(d => d.Daytype).Distinct().ToHashSet();

            while (currentDate <= endDate)
            {
                bool isUnavailable = false;

                if (!doctorDaysOfWeek.Contains((int)currentDate.DayOfWeek))
                    isUnavailable = true;

                if (!isUnavailable)
                {
                    var holiday = Holidays.FirstOrDefault(h =>
                        h.Holidaydate.HasValue &&
                        h.Holidaydate.Value.Day == currentDate.Day &&
                        h.Holidaydate.Value.Month == currentDate.Month);

                    if (holiday != null && holiday.Canschedule != 1)
                        isUnavailable = true;
                }

                if (!isOnline && !isUnavailable)
                {
                    var clinicDaySlots = AvailabilityClinic.Where(c => c.Daytype == (int)currentDate.DayOfWeek).ToList();
                    if (clinicDaySlots.Count == 0)
                        isUnavailable = true;
                }

                if (isUnavailable)
                    unavailableDays.Add(currentDate);

                currentDate = currentDate.AddDays(1);
            }

            return unavailableDays;
        }

        public DateTime GetFirstValidDay(int idDoctor, bool isOnline, DateTime startDate)
        {
            DateTime candidateDate = startDate.Date.AddDays(1);
            DateTime maxDate = startDate.Date.AddYears(1);

            var doctorAvailabilities = DoctorsAvailabilities
                .Where(d => d.Iddoctor == idDoctor && d.Isonline == 1 == isOnline)
                .ToList();

            var doctorDaysOfWeek = doctorAvailabilities.Select(d => d.Daytype).Distinct().ToHashSet();

            while (candidateDate <= maxDate)
            {
                int dayOfWeek = (int)candidateDate.DayOfWeek;
                if (!doctorDaysOfWeek.Contains(dayOfWeek))
                {
                    candidateDate = candidateDate.AddDays(1);
                    continue;
                }

                if (!isOnline)
                {
                    var clinicDaySlots = AvailabilityClinic.Where(c => c.Daytype == dayOfWeek).ToList();
                    if (clinicDaySlots.Count == 0)
                    {
                        candidateDate = candidateDate.AddDays(1);
                        continue;
                    }
                }

                var slots = GetAvailableSlots(idDoctor, candidateDate,
                    new Appointment { Idappointmenttype = (int)AppointmentType.Consultation, Averagetime = TimeOnly.MinValue },
                    monthsToCalculate: 1);

                var daySlot = slots.FirstOrDefault(s => s.Date == candidateDate);
                if (daySlot != null && daySlot.AvailableTimes.Count > 0)
                    return candidateDate;

                candidateDate = candidateDate.AddDays(1);
            }

            return DateTime.MinValue;
        }

    }
}
