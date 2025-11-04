using Mopups.Pages;
using ViverApp.Shared.Models;
using ViverAppMobile.Controls;
using ViverAppMobile.Helpers;
using ViverAppMobile.Workers;

namespace ViverAppMobile.Views.Popups;

public partial class ConfirmPaymentPopup : PopupPage, IPopupAsync
{
    private readonly TaskCompletionSource<object?> _taskCompletionSource = new();

    public ConfirmPaymentPopup()
	{
		InitializeComponent();
        BindingContext = this;
        var array = PopupHelper<string[]>.GetValue();
        if(array.Length == 2)
        {
            string pacientname = array[0];
            string appointmentprice = array[1];

            AppointmentInfoLabel.Text = $"Confirme o pagamento do atendimento de {pacientname} no valor de {appointmentprice}";
        }

        List<string> paymethods =
        [
            "Selecione a forma de pagamento",
            "Cartão de Crédito",
            "Cartão de Débito",
            "PIX",
            "Dinheiro",
        ];

        PayMethodPicker.ItemsSource = paymethods;
        PayMethodPicker.SelectedIndex = 0;
        PayDatePicker.Date = DateTime.Today;
        PayTimePicker.Time = TimeOnly.FromDateTime(DateTime.Now);
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
        int paymethodindex = PayMethodPicker.SelectedIndex;
        if (paymethodindex <= 0)
            return;

        try
        {
            string? last4 = string.IsNullOrWhiteSpace(Last4CardDigitsEntry.Text) 
                ? null : Last4CardDigitsEntry.Text;
            string? cardauthorization = string.IsNullOrWhiteSpace(CardAuthorizationEntry.Text)
                ? null : CardAuthorizationEntry.Text;

            bool iscard = paymethodindex == 1 || paymethodindex == 2;
            if (iscard && string.IsNullOrWhiteSpace(last4))
                throw new Exception("Necessário informar os últimos dígitos do cartão\nObs: No canhoto do pagamento tem essa informação");
            if (iscard && !ValidationHelper.IsNumber(last4))
                throw new Exception("Os últimos 4 dígitos do cartão só podem ser números!");
            if (iscard && string.IsNullOrWhiteSpace(cardauthorization))
                throw new Exception("Necessário informar o número de autorização da transação.\nObs: No canhoto do pagamento tem essa informação");

            var dateselected = DateOnly.FromDateTime(PayDatePicker.Date);
            var timeselected = PayTimePicker.Time ?? TimeOnly.MinValue;
            DateTime dateTimeSelected = new(dateselected, timeselected);
            var selectedPayMethod = (PayMethod)PayMethodPicker.SelectedIndex;
            string?[] arrayToBack = 
            [
                selectedPayMethod.ToString(),
                last4,
                cardauthorization,
                dateTimeSelected.ToString()
            ];

            PopupHelper<string?[]>.SetValue(arrayToBack);
            _taskCompletionSource.TrySetResult(true);

        }
        catch(Exception ex)
        {
            Messenger.ShowErrorMessage(ex.Message,"Ops...");
        }
    }

    private void PayMethodPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        int paymethodindex = PayMethodPicker.SelectedIndex;
        ConfirmButton.Opacity = paymethodindex > 0 ? 1 : 0.5;

        bool iscard = paymethodindex == 1 || paymethodindex == 2;
        CardInfosLayout.IsVisible = iscard;
        MasterBorder.HeightRequest = iscard ? 430 : 300;
    }
}