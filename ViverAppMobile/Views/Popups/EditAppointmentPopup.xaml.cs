using CommunityToolkit.Mvvm.ComponentModel;
using Mopups.Pages;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ViverApp.Shared.Models;
using ViverApp.Shared.Utils;
using ViverAppMobile.Controls;
using ViverAppMobile.Helpers;

namespace ViverAppMobile.Views.Popups;

public partial class EditAppointmentPopup : PopupPage, IPopupAsync
{
    private readonly TaskCompletionSource<object?> _taskCompletionSource = new();
    public Appointment? ThisAppointment { get; set; }
    public ObservableCollection<string> AppointmentTypes { get; set; } = [];

    public EditAppointmentPopup()
	{
		InitializeComponent();
        BindingContext = this;

        ThisAppointment = PopupHelper<Appointment>.GetValue();

        AppointmentTypes = ["\ue811  Consulta", "\ue817  Exame", "\ue806  Cirurgia"];
        AppointmentTypesPicker.ItemsSource = AppointmentTypes;
        AppointmentTypesPicker.SelectedIndex = ThisAppointment.Idappointmenttype - 1;
        AppointmentTitleEntry.Text = ThisAppointment.Title;
        AppointmentDescriptionEditor.Text = ThisAppointment.Description;
        AppointmentPriceEntry.Text = ThisAppointment.Price.ToString();
        AppointmentAverageTimeMinEntry.Text = DateTimeHelper.TotalMinutes(ThisAppointment.Averagetime).ToString();
    }

    public Task<object?> WaitForResultAsync() => _taskCompletionSource.Task;
    protected override bool OnBackButtonPressed() => ClosePopup();
    protected override bool OnBackgroundClicked() => ClosePopup();
    private void BackButtonPressed(object sender, TappedEventArgs e) => ClosePopup();

    public bool ClosePopup()
    {
        _taskCompletionSource.TrySetResult(false);
        return base.OnBackgroundClicked();
    }

    private void YesButtonPressed(object sender, TappedEventArgs e)
    {
        ThisAppointment.Idappointmenttype = AppointmentTypesPicker.SelectedIndex + 1;
        ThisAppointment.Title = AppointmentTitleEntry.Text;
        ThisAppointment.Description = AppointmentDescriptionEditor.Text;
        ThisAppointment.Price = decimal.TryParse(AppointmentPriceEntry.Text, out decimal decvalue) ? decvalue : decimal.Zero;

        double mins = double.TryParse(AppointmentAverageTimeMinEntry.Text, out double doublevalue) ? doublevalue : 0d;
        ThisAppointment.Averagetime = new TimeOnly().AddMinutes(mins);

        PopupHelper<Appointment>.SetValue(ThisAppointment);
        _taskCompletionSource.TrySetResult(true);
    }
}