using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ViverAppMobile.Behaviors
{
    public partial class CollectionViewDismissBehavior : Behavior<Border>
    {
        const double HORIZONTAL_THRESHOLD = 10;
        double xInicial;
        Border? attachedBorder;
        PanGestureRecognizer? panGesture;
        bool isHorizontalPan;

        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(
                nameof(ItemsSource),
                typeof(IList),
                typeof(CollectionViewDismissBehavior),
                default(IList));

        public IList? ItemsSource
        {
            get => (IList?)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly BindableProperty ItemDismissedCommandProperty =
            BindableProperty.Create(
                nameof(ItemDismissedCommand),
                typeof(ICommand),
                typeof(CollectionViewDismissBehavior),
                default(ICommand));

        public ICommand? ItemDismissedCommand
        {
            get => (ICommand?)GetValue(ItemDismissedCommandProperty);
            set => SetValue(ItemDismissedCommandProperty, value);
        }

        protected override void OnAttachedTo(Border bindable)
        {
            base.OnAttachedTo(bindable);
            attachedBorder = bindable;

            panGesture = new PanGestureRecognizer();
            panGesture.PanUpdated += OnPanUpdated;
            bindable.GestureRecognizers.Add(panGesture);
        }

        protected override void OnDetachingFrom(Border bindable)
        {
            base.OnDetachingFrom(bindable);

            if (panGesture != null)
            {
                panGesture.PanUpdated -= OnPanUpdated;
                if (attachedBorder != null && attachedBorder.GestureRecognizers.Contains(panGesture))
                    attachedBorder.GestureRecognizers.Remove(panGesture);
            }

            attachedBorder = null;
            panGesture = null;
        }

        async void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
        {
            if (attachedBorder == null) return;

            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    xInicial = attachedBorder.TranslationX;
                    isHorizontalPan = false;
                    break;

                case GestureStatus.Running:
                    var dx = e.TotalX;
                    var dy = e.TotalY;

                    if (!isHorizontalPan && Math.Abs(dx) > Math.Abs(dy) && Math.Abs(dx) > HORIZONTAL_THRESHOLD)
                    {
                        isHorizontalPan = true;
                        DisallowParentIntercept(true);
                    }

                    if (isHorizontalPan)
                        attachedBorder.TranslationX = xInicial + dx;

                    break;

                case GestureStatus.Canceled:
                case GestureStatus.Completed:
                    if (isHorizontalPan)
                    {
                        if (attachedBorder.TranslationX < -100)
                        {
                            await attachedBorder.TranslateTo(-500, 0, 250, Easing.CubicIn);

                            var item = attachedBorder.BindingContext;
                            if (item != null && ItemsSource != null && ItemsSource.Contains(item))
                            {
                                attachedBorder.Dispatcher?.Dispatch(() =>
                                {
                                    ItemsSource.Remove(item);
                                    ItemDismissedCommand?.Execute(item);
                                });
                            }
                        }
                        else
                        {
                            await attachedBorder.TranslateTo(0, 0, 250, Easing.CubicOut);
                        }

                        DisallowParentIntercept(false);
                    }
                    isHorizontalPan = false;
                    break;
            }
        }

        void DisallowParentIntercept(bool disallow)
        {
#if ANDROID
        try
        {
            var nativeView = attachedBorder?.Handler?.PlatformView as Android.Views.View;
            if (nativeView == null)
                return;

            var parent = nativeView.Parent;
            while (parent is Android.Views.ViewGroup vg)
            {
                vg.RequestDisallowInterceptTouchEvent(disallow);
                parent = vg.Parent;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DisallowParentIntercept failed: {ex}");
        }
#endif
        }
    }
}
