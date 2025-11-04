using ViverAppMobile.Workers;

namespace ViverAppMobile.Helpers
{
    public static class PhoneHelper
    {
        public static async Task CallAsync(string? phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                await Messenger.ShowErrorMessageAsync("Número de telefone inválido.");
                return;
            }

            try
            {
                if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
                {
                    PhoneDialer.Open(phoneNumber);
                }
                else if (DeviceInfo.Platform == DevicePlatform.WinUI)
                {
                    var digitsOnly = new string(phoneNumber.Where(char.IsDigit).ToArray());

                    var whatsappDesktopUri = new Uri($"whatsapp://send?phone={digitsOnly}");
                    bool openedDesktop = await Launcher.TryOpenAsync(whatsappDesktopUri);

                    if (!openedDesktop)
                    {
                        var whatsappWebUri = new Uri($"https://web.whatsapp.com/send?phone={digitsOnly}");
                        bool openedWeb = await Launcher.TryOpenAsync(whatsappWebUri);

                        if (!openedWeb)
                        {
                            await Clipboard.SetTextAsync(phoneNumber);
                            await Messenger.ShowMessage(
                                "Não foi possível abrir o WhatsApp. O número foi copiado para a área de transferência.",
                                "WhatsApp não encontrado");
                        }
                    }
                }
                else
                {
                    await Clipboard.SetTextAsync(phoneNumber);
                    await Messenger.ShowMessage(
                        "Essa plataforma não suporta ligação direta. O número foi copiado para a área de transferência.",
                        "Ligação");
                }
            }
            catch (Exception ex)
            {
                await Clipboard.SetTextAsync(phoneNumber);
                await Messenger.ShowErrorMessageAsync(
                    $"Não foi possível iniciar a ligação. O número foi copiado para a área de transferência.\n\nDetalhes: {ex.Message}");
            }
        }
    }
}
