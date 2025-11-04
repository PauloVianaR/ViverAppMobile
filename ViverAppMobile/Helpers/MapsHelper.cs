using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using System;
using System.Threading.Tasks;
using ViverAppMobile.Workers;

namespace ViverAppMobile.Helpers
{

    public static class MapsHelper
    {
        public static async Task OpenRouteByCepAsync(string? cep, string? number)
        {
            if (string.IsNullOrWhiteSpace(cep) || string.IsNullOrWhiteSpace(number))
            {
                await Messenger.ShowErrorMessageAsync("CEP ou número inválido.");
                return;
            }

            try
            {
                var destination = Uri.EscapeDataString($"{cep}, {number}");

                if (DeviceInfo.Platform == DevicePlatform.Android)
                {
                    var uri = new Uri($"geo:0,0?q={destination}");
                    await Launcher.OpenAsync(uri);
                }
                else if (DeviceInfo.Platform == DevicePlatform.iOS)
                {
                    var uri = new Uri($"http://maps.apple.com/?daddr={destination}");
                    await Launcher.OpenAsync(uri);
                }
                else if (DeviceInfo.Platform == DevicePlatform.WinUI)
                {
                    var uri = new Uri($"https://www.google.com/maps/dir/?api=1&destination={destination}&travelmode=driving");
                    await Launcher.OpenAsync(uri);
                }
                else
                {
                    await Messenger.ShowMessage(
                        "Essa plataforma não suporta abrir rotas diretamente.",
                        "Rotas");
                }
            }
            catch (Exception ex)
            {
                await Messenger.ShowErrorMessageAsync(
                    $"Não foi possível abrir a rota.\n\nDetalhes: {ex.Message}");
            }
        }
    }
}
