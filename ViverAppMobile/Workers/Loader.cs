using ViverAppMobile.Helpers;

namespace ViverAppMobile.Workers
{
    public static class Loader
    {
        public const int LoadTime = 500;

        public static async Task RunWithLoadingAsync(Func<Task<string?>> loadFunc, bool ispopup = false)
        {
            string? errs = null;

            try
            {
                if (!ispopup)
                {
                    await Task.Delay(LoadTime);
                    await PopupHelper.PopAllPopUpAsync();
                }

                await PopupHelper.PushLoadingAsync();                
                errs = await Task.Run(loadFunc);

                if (ispopup)
                    await PopupHelper.PopLastPopUpAsync();
                else
                    await PopupHelper.PopAllPopUpAsync();

                if (!string.IsNullOrWhiteSpace(errs))
                    await Messenger.ShowErrorMessageAsync(errs);
            }
            catch (Exception ex)
            {
                await Messenger.ShowErrorMessageAsync(ex.Message);
                try { await PopupHelper.PopAllPopUpAsync(); }
                catch { }
            }
        }
    }
}
