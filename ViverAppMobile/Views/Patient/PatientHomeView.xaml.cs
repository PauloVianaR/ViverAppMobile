using System.Collections.ObjectModel;
using ViverAppMobile.Controls;
using ViverAppMobile.Models;
using ViverAppMobile.Workers;

namespace ViverAppMobile.Views.Patient;

public partial class PatientHomeView : ContentView
{
    private bool instanced = false;
    private int next = 0;
    private CancellationTokenSource? _autoScrollToken;

    public PatientHomeView()
    {
        InitializeComponent();
        this.Loaded += async (sender, e) =>
        {
            if (BindingContext is IViewModelInstancer vm && !instanced)
            {
                await Task.Yield();
                await vm.InitializeAsync();
                instanced = true;

                _ = typeof(CarouselView);
                _ = ImageCarousel.GetType();
                await Task.Delay(100);
                ImageCarousel.IsVisible = true;
                //await Task.Delay(150);

                //Dispatcher.Dispatch(() => ImageCarousel.InvalidateMeasure());
                //this.RestartAutoScroll();
            }
        };
    }

    private void RestartAutoScroll()
    {
        _autoScrollToken?.Cancel();
        _autoScrollToken = new();

        _ = Task.Run(() => RunAutoScrollLoopAsync(_autoScrollToken.Token));
    }

    private async Task RunAutoScrollLoopAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested && !Master.GlobalToken.IsCancellationRequested)
            {
                await Task.Delay(4000, token);
                if (!token.IsCancellationRequested)
                    ScrollCarousel();
            }
        }
        catch (TaskCanceledException) { }
        catch (OperationCanceledException) { }
    }    

    private void ScrollCarousel()
    {
        if (ImageCarousel.ItemsSource is not ObservableCollection<CarouselItem> items || items.Count == 0)
            return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            next = ImageCarousel.Position + 1;

            if (next >= items.Count)
            {
                var source = ImageCarousel.ItemsSource;
                ImageCarousel.ItemsSource = null;
                ImageCarousel.ItemsSource = source;

                next = 0;
            }

            ImageCarousel.Position = next;
        });
    }

    private void NextCarouselImage(object? sender, TappedEventArgs e)
    {
        if (ImageCarousel.ItemsSource is not ObservableCollection<CarouselItem> items || items.Count == 0)
            return;

        next = (ImageCarousel.Position + 1) % items.Count;

        ScrollCarousel();
        RestartAutoScroll();
    }

    private async void PreviousCarouselImage(object? sender, EventArgs e)
    {
        if (ImageCarousel.ItemsSource is not ObservableCollection<CarouselItem> items || items.Count == 0)
            return;

        _autoScrollToken?.Cancel();

        int current = ImageCarousel.Position;
        int count = items.Count;

        if (count == 1)
        {
            RestartAutoScroll();
            return;
        }

        int desired = (current - 1 + count) % count;

        if (current > 0)
        {
            MainThread.BeginInvokeOnMainThread(() => ImageCarousel.Position = desired);
            RestartAutoScroll();
            return;
        }
        var source = ImageCarousel.ItemsSource;

        bool prevAnimated = ImageCarousel.IsScrollAnimated;
        await MainThread.InvokeOnMainThreadAsync(() => ImageCarousel.IsScrollAnimated = false);

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            ImageCarousel.ItemsSource = null;
            ImageCarousel.ItemsSource = source;
        });

        int preReset = (current - 2 + count) % count;
        if (preReset < 0) preReset += count;

        await Task.Delay(30);

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            ImageCarousel.Position = preReset;
            ImageCarousel.InvalidateMeasure();
        });

        await Task.Delay(30);

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            ImageCarousel.IsScrollAnimated = prevAnimated;
            ImageCarousel.Position = desired;
        });

        RestartAutoScroll();
    }
}