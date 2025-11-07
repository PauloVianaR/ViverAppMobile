using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using System.Diagnostics;

//[assembly: XamlCompilation(XamlCompilationOptions.Skip)]
namespace ViverAppMobile.Controls
{
    public partial class CarouselItemView : ContentView
    {
        public CarouselItemView()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                try
                {
#if WINDOWS
                    var path = "D:\\erros.txt";
#else
                    var path = Path.Combine(FileSystem.AppDataDirectory, "erros.txt");
#endif

                    string? dir = Path.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    File.AppendAllText(path,
                        $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Erro no InitializeComponent:{Environment.NewLine}{ex}{Environment.NewLine}{new string('-', 80)}{Environment.NewLine}");
                }
                catch (Exception ex2)
                {
                    Debug.WriteLine(ex2);
                }
            }
        }
    }
}
