using System.ComponentModel;

namespace ViverAppMobile.Controls
{
    public partial class SkeletonControl : ContentView
    {
        public static readonly BindableProperty IsRunningProperty =
            BindableProperty.Create(nameof(IsRunning), typeof(bool), typeof(SkeletonControl), true, propertyChanged: OnIsRunningChanged);

        public static readonly BindableProperty DurationProperty =
            BindableProperty.Create(nameof(Duration), typeof(uint), typeof(SkeletonControl), (uint)1400);

        public bool IsRunning
        {
            get => (bool)GetValue(IsRunningProperty);
            set => SetValue(IsRunningProperty, value);
        }

        public uint Duration
        {
            get => (uint)GetValue(DurationProperty);
            set => SetValue(DurationProperty, value);
        }

        bool _animationRunning = false;

        public SkeletonControl()
        {
            InitializeComponent();
            SetupDefaultVisibility();

            if (Presenter != null)
            {
                Presenter.PropertyChanged += Presenter_PropertyChanged;
            }

            this.Loaded += async (s, e) =>
            {
                await Dispatcher.DispatchAsync(async () =>
                {
                    await Task.Delay(300);
                    if (this.Handler?.PlatformView == null)
                        return;

                    SetupDefaultVisibility();
                    if (IsRunning)
                        StartShimmer();
                });
            };
        }

        private void Presenter_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ContentPresenter.Content))
            {
                SetupDefaultVisibility();
            }
        }

        void SetupDefaultVisibility()
        {
            try
            {
                DefaultBox.IsVisible = (Presenter?.Content == null);
            }
            catch
            {
                DefaultBox.IsVisible = true;
            }
        }

        static void OnIsRunningChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (SkeletonControl)bindable;
            if ((bool)newValue)
                control.StartShimmer();
            else
                control.StopShimmer();
        }

        public void StartShimmer()
        {
            if (_animationRunning)
                return;

            _animationRunning = true;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                while (_animationRunning && IsRunning)
                {
                    try
                    {
                        var tcs = new TaskCompletionSource<bool>();
                        var animation = new Animation
                        {
                            { 0, 1, new Animation(v => stop1.Offset = (float)v, -1, 1.5) },
                            { 0, 1, new Animation(v => stop2.Offset = (float)v, -0.5, 1.8) },
                            { 0, 1, new Animation(v => stop3.Offset = (float)v, 0, 2.0) }
                        };

                        animation.Commit(this, "Shimmer", length: (uint)Duration, easing: Easing.Linear, finished: (v, b) => tcs.SetResult(true));

                        await tcs.Task;
                    }
                    catch { }
                }
            });
        }

        public void StopShimmer()
        {
            _animationRunning = false;
            try { this.AbortAnimation("Shimmer"); } catch { }
        }
    }
}
