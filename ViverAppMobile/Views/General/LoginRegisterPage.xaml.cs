using ViverAppMobile.ViewModels;
using ViverAppMobile.ViewModels.General;

namespace ViverAppMobile.Views.General
{
    public partial class LoginRegisterPage : ContentPage
    {
        private readonly List<Entry> digitEntries = [];

        public LoginRegisterPage()
        {
            InitializeComponent();
            digitEntries = [Digit1Entry, Digit2Entry, Digit3Entry, Digit4Entry];
        }

        private void DigitEntry_TextChanged(object? sender, TextChangedEventArgs e)
        {
            if (sender is not Entry senderEntry)
                return;

            int index = digitEntries.IndexOf(senderEntry);
            if (index < 0 || index == 3)
                return;

            if (!string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                var entry = digitEntries[index + 1];
                entry.IsReadOnly = false;
                entry.Focus();
            }
        }

        private void ReSendEmail(object? sender, TappedEventArgs e)
        {
            digitEntries.ForEach(e =>
            {
                e.Text = string.Empty;
                e.IsReadOnly = true;
            });

            Digit1Entry.IsReadOnly = false;
            Digit1Entry.Focus();

            if (BindingContext is not LoginRegisterViewModel vm)
                return;

            if (vm.ReSendConfirmationEmailCommand.CanExecute(null))
                vm.ReSendConfirmationEmailCommand.Execute(null);
        }
    }
}