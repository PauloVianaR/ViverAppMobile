using CommunityToolkit.Maui.Alerts;

namespace ViverAppMobile.Workers
{
    public static class Messenger
    {
        /// <summary>
        /// Show an Dialog with yes/no options question 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="title"></param>
        /// <param name="yes"></param>
        /// <param name="no"></param>
        /// <returns>True if <paramref name="yes"/> pressed. False if <paramref name="no"/> pressed</returns>
        public async static Task<bool> ShowQuestionMessage(string? text = "Question", string? title = "Pergunta...", string? yes = "Sim", string? no = "Não")
            => await Application.Current.Windows[0].Page.DisplayAlert(title, text, yes, no);
        public async static void ShowErrorMessage(string? text = "Mensagem", string? title = "Ops...", string? cancel = "OK") => await ShowMessage(text, title, cancel);
        public async static Task ShowErrorMessageAsync(string? text = "Mensagem", string? title = "Ops...", string? cancel = "OK") => await ShowMessage(text, title, cancel);
        public async static Task ShowMessage(string? text = "Mensagem", string? title = "Mensagem", string? cancel = "OK")
            => await Application.Current.Windows[0].Page.DisplayAlert(title ?? string.Empty, text ?? string.Empty, cancel ?? string.Empty);

        public async static Task ShowSnackMessage(string message) => await Snackbar.Make(message).Show();
        public async static Task ShowToastMessage(string message) => await Toast.Make(message).Show();
    }
}
