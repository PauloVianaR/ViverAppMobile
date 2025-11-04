using System;
using Microsoft.Maui.Controls;
using MauiTimePicker = Microsoft.Maui.Controls.TimePicker;

#if ANDROID
using Android.Widget;
using Microsoft.Maui.Handlers;
using NativeTimePickerAndroid = Android.Widget.TimePicker;
#endif

#if IOS
using UIKit;
using Foundation;
#endif

#if WINDOWS
using Microsoft.UI.Xaml.Controls;
using NativeTimePickerWindows = Microsoft.UI.Xaml.Controls.TimePicker;
#endif

namespace ViverAppMobile.Controls;

public partial class CustomTimePicker : ContentView
{
    public static readonly BindableProperty TimeProperty =
        BindableProperty.Create(
            nameof(Time),
            typeof(TimeOnly?),
            typeof(CustomTimePicker),
            null,
            BindingMode.TwoWay,
            propertyChanged: OnTimeChanged);

    public TimeOnly? Time
    {
        get => (TimeOnly?)GetValue(TimeProperty);
        set => SetValue(TimeProperty, value);
    }

    private static readonly TimeOnly DefaultTime = new TimeOnly(8, 0);

    public CustomTimePicker()
    {
        InitializeComponent();

        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += (s, e) =>
        {
            Apply24HourFormat();
            NativeTimePicker?.Focus();
        };
        this.GestureRecognizers.Add(tapGesture);

        UpdateLabel();

        NativeTimePicker.HandlerChanged += (s, e) =>
        {
            Apply24HourFormat();
        };

        NativeTimePicker.Focused += (s, e) =>
        {
            Apply24HourFormat();
        };
    }

    private static void OnTimeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not CustomTimePicker control) return;

        control.UpdateLabel();
        control.UpdateNativeTimePicker();
    }

    private void UpdateLabel()
    {
        if (TimeLabel is null) return;

        TimeLabel.Text = Time.HasValue ? Time.Value.ToString("HH:mm") : "--:--";
    }

    private void UpdateNativeTimePicker()
    {
        if (NativeTimePicker is null) return;

        var timeToSet = Time ?? DefaultTime;

        try
        {
            NativeTimePicker.Time = timeToSet.ToTimeSpan();
        }
        catch
        {
            NativeTimePicker.Time = DefaultTime.ToTimeSpan();
        }
    }

    private void NativeTimePicker_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(MauiTimePicker.Time)) return;

        if (NativeTimePicker is null) return;

        try
        {
            Time = TimeOnly.FromTimeSpan(NativeTimePicker.Time);
        }
        catch
        {
            Time = DefaultTime;
        }
    }

    private void Apply24HourFormat()
    {
#if ANDROID
        if (NativeTimePicker.Handler?.PlatformView is NativeTimePickerAndroid nativeAndroid)
        {
            nativeAndroid.SetIs24HourView(Java.Lang.Boolean.True!);
        }
#endif

#if IOS
        if (NativeTimePicker.Handler?.PlatformView is UIDatePicker nativeiOS)
        {
            nativeiOS.Locale = new NSLocale("en_GB"); // 24h
        }
#endif

#if WINDOWS
        if (NativeTimePicker.Handler?.PlatformView is NativeTimePickerWindows nativeWin)
        {
            nativeWin.ClockIdentifier = "24HourClock";
        }
#endif
    }
}
