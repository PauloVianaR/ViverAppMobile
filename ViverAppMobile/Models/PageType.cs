namespace ViverAppMobile.Models
{
    public enum PageType
    {
        None,
        AdminMainPage,
        PatientMainPage,
        ManagerMainPage,
        DoctorMainPage
    }

    public enum AdminPage
    {
        Home,
        Clinic,
        Appointments,
        Analytics,
        Notification,
        UserManagement
    }

    public enum PatientPage
    {
        Home,
        Schedule,
        Agenda,
        Payment,
        Profile
    }

    public enum ManagerPage
    {
        Home,
        Agenda,
        Historic,
        Profile
    }

    public enum DoctorPage
    {
        Home,
        Agenda,
        Historic,
        Profile
    }    
}
