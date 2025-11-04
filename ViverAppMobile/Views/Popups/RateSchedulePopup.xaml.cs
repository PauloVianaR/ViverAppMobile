using Mopups.Pages;
using System.Windows.Input;
using ViverApp.Shared.DTos;
using ViverAppMobile.Controls;
using ViverAppMobile.Helpers;
using ViverAppMobile.Models;

namespace ViverAppMobile.Views.Popups;

public partial class RateSchedulePopup : PopupPage, IPopupAsync
{
    private readonly List<Label> stars = [];
    private readonly TaskCompletionSource<object?> _taskCompletionSource = new();
    public string appointmentTypeTitle = "Consulta";

    public ScheduleDto? UserSchedule { get; set; }
    public ICommand ChooseRateCommand { get; }
    private int rating = 0;

    public string AppointmentTypeTitle
    {
        get => appointmentTypeTitle;
        set
        {
            appointmentTypeTitle = value;
            OnPropertyChanged(nameof(AppointmentTypeTitle));
        }
    }

    public RateSchedulePopup()
	{
        InitializeComponent();
        UserSchedule = PopupHelper<ScheduleDto>.GetValue();        
        ChooseRateCommand = new Command<int>(ChooseRate);
        BindingContext = this;
        stars = [Star1, Star2, Star3, Star4, Star5];

        AppointmentTypeTitle = (AppointmentType)UserSchedule.AppointmentType switch 
        {
            AppointmentType.Consultation => "Consulta",
            AppointmentType.Examination => "Exame",
            AppointmentType.Surgery => "Cirurgia",
            _ => "Consulta"
        };

        Feedback.Text = UserSchedule.FeedBack ?? string.Empty;
        var currentRating = (int?)UserSchedule?.Rating ?? 0;
        this.ChooseRate(currentRating);
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
        UserSchedule.Rating = (float)rating;
        UserSchedule.FeedBack = Feedback.Text;

        PopupHelper<ScheduleDto>.SetValue(UserSchedule);
        _taskCompletionSource.TrySetResult(true);
    }

    private void ChooseRate(int index)
    {
        int paintedStars = -1;
        for (int i = 1; i <= stars.Count; i++)
        {
            if(paintedStars <= index)
                paintedStars++;

            stars[i - 1].TextColor = paintedStars >= index ? Colors.Gray : Colors.Gold;
            stars[i - 1].Text = paintedStars >= index ? "\ue834" : "\ue835";
        }

        rating = index;
    }
}