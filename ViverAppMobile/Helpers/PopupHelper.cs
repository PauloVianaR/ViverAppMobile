using Mopups.Pages;
using Mopups.Services;
using ViverAppMobile.Controls;
using ViverAppMobile.Views.Popups;
using ViverAppMobile.Workers;

namespace ViverAppMobile.Helpers
{
    public static class PopupHelper<T>
    {
        private static T? savedValue;
        public static void SetValue(T? value)
        {
            ClearValue();
            savedValue = value;
        }
        public static T? GetValue() => savedValue;
        public static void ClearValue() => savedValue = (T?)(object?)null;

        public static async Task<bool> PushInstanceAsync<TPopup>(T? valueToSave) where TPopup : PopupPage, IPopupAsync, new()
        {
            SetValue(valueToSave);

            var popup = new TPopup();
            await PopupHelper.PushAsync(popup);

            var returnValue = await popup.WaitForResultAsync();
            await MopupService.Instance.PopAllAsync();

            return returnValue is bool value && value;
        }

        public static async Task PushInstanceAsync<TPopup>() where TPopup : PopupPage, IPopupAsync, new()
        {
            var popup = new TPopup();
            await PopupHelper.PushAsync(popup);

            _ = await popup.WaitForResultAsync();
            await MopupService.Instance.PopAllAsync();
        }
    }

    public static class PopupHelper
    {
        public static bool CanActivateSecondLoadingPopup = true;
        private static readonly LoadingPopup loading = new();
        private static bool wasLoadingPopupPushed = false;

        public async static Task PushAsync(PopupPage popup) => await MopupService.Instance.PushAsync(popup);

        public async static Task PopAllPopUpAsync()
        {
            await MopupService.Instance.PopAllAsync();
            wasLoadingPopupPushed = false;
        }

        public async static Task PopLastPopUpAsync()
        {
            await MopupService.Instance.PopAsync();

            if (!MopupService.Instance.PopupStack.Contains(loading))
                wasLoadingPopupPushed = false;
        }
        public async static Task PushLoadingAsync()
        {
            if (wasLoadingPopupPushed)
                return;

            await Navigator.WaitTransition();
            await MopupService.Instance.PushAsync(loading);
            wasLoadingPopupPushed = true;
        }

        public async static Task PopLoadingAsync()
        {
            if (!wasLoadingPopupPushed)
                return;

            if (MopupService.Instance.PopupStack.Contains(loading))
            {
                await MopupService.Instance.RemovePageAsync(loading);
                wasLoadingPopupPushed = false;
            }
        }
    }
}
