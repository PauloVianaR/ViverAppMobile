using Mopups.Pages;
using ViverAppMobile.Controls;
using ViverAppMobile.Helpers;

namespace ViverAppMobile.Views.Popups;

public partial class ChangePasswordPopup : PopupPage, IPopupAsync
{
    private readonly TaskCompletionSource<object?> _taskCompletionSource = new();

    private string eyeIcon = "\ue836";
    public string EyeIcon
    {
        get => eyeIcon;
        set
        {
            eyeIcon = value;
            OnPropertyChanged(nameof(EyeIcon));
        }
    }

    private bool ispasswordHidden = true;
    public bool IspasswordHidden
    {
        get => ispasswordHidden;
        set
        {
            ispasswordHidden = value;
            OnPropertyChanged(nameof(IspasswordHidden));
        }
    }

    private bool canChangePass = false;
    public bool CanChangePass
    {
        get => canChangePass;
        set
        {
            canChangePass = value;
            OnPropertyChanged(nameof(CanChangePass));
        }
    }

    private bool isValidNewPass = false;

    public bool IsValidNewPass
    {
        get => isValidNewPass;
        set
        {
            isValidNewPass = value;
            OnPropertyChanged(nameof(IsValidNewPass));
        }
    }

    private bool isValidConfirmPass = true;

    public bool IsValidConfirmPass
    {
        get => isValidConfirmPass;
        set
        {
            isValidConfirmPass = value;
            OnPropertyChanged(nameof(IsValidConfirmPass));
        }
    }


    public ChangePasswordPopup()
    {
        InitializeComponent();
        BindingContext = this;
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
        if (!CanChangePass)
            return;

        PopupHelper<string[]>.SetValue([CurrentPasswordEntry.Text,NewPasswordEntry.Text]);
        _taskCompletionSource.TrySetResult(true);
    }

    private void TogglepasswordVisibility(object sender, TappedEventArgs e)
    {
        IspasswordHidden = !IspasswordHidden;
        EyeIcon = IspasswordHidden ? "\ue836" : "\ue837";
    }

    private void EntryTextChanged(object sender, TextChangedEventArgs e)
    {
        IsValidNewPass = ValidationHelper.IsPasswordValid(NewPasswordEntry.Text);
        IsValidConfirmPass = ConfirmPasswordEntry.Text == NewPasswordEntry.Text;

        CanChangePass = IsValidNewPass && IsValidConfirmPass && !string.IsNullOrWhiteSpace(CurrentPasswordEntry.Text);
    }
}