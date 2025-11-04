using CommunityToolkit.Mvvm.Messaging.Messages;
using ViverApp.Shared.DTos;

namespace ViverAppMobile.Models
{
    public class NavigateTabMessage(string tabName) : ValueChangedMessage<string>(tabName) { }
    public class NavigateTabIndex(int index) : ValueChangedMessage<int>(index) { }
    public class UserChangedMessage(bool changed) : ValueChangedMessage<bool>(changed) { }
    public class DesinstancePagesExceptOneMessage(string pageExcept) : ValueChangedMessage<string>(pageExcept) { }
    public class DesinstanceAllPages(bool value) : ValueChangedMessage<bool>(value) { }
    public class ShowSchedulePageSelectAppointmentMessage(int idappointment) : ValueChangedMessage<int>(idappointment) { }
    public class ShowSchedulePageSelectAppointmentByMainMessage(int idappointment) : ValueChangedMessage<int>(idappointment) { }
    public class ShowProfilePageSelectTabMessage(string tabindex) : ValueChangedMessage<string>(tabindex) { }
    public class ShowProfilePageSelectTabByMainMessage(string tabindex) : ValueChangedMessage<string>(tabindex) { }
    public class ShowPaymentPageSelectScheduleToPayMessage(int idschedule) : ValueChangedMessage<int>(idschedule) { }
    public class ShowPaymentPageSelectScheduleToPayByMainMessage(int idschedule) : ValueChangedMessage<int>(idschedule) { }
    public class UsersActiveChanged(UserDto changedUser) : ValueChangedMessage<UserDto>(changedUser) { }
    public class OnlineConcluded(ScheduleDto onlineSchedule) : ValueChangedMessage<ScheduleDto>(onlineSchedule) { }
}
