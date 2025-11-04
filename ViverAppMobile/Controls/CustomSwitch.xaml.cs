using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace ViverAppMobile.Controls;

public partial class CustomSwitch : ContentView
{
    public static readonly BindableProperty IsToggledProperty =
        BindableProperty.Create(nameof(IsToggled), typeof(bool), typeof(CustomSwitch), false,
            BindingMode.TwoWay, propertyChanged: OnIsToggledChanged);

    public static readonly BindableProperty OnColorProperty =
        BindableProperty.Create(nameof(OnColor), typeof(Color), typeof(CustomSwitch), Colors.MediumSeaGreen);

    public static readonly BindableProperty OffColorProperty =
        BindableProperty.Create(nameof(OffColor), typeof(Color), typeof(CustomSwitch), Colors.White);

    public static readonly BindableProperty ThumbColorProperty =
        BindableProperty.Create(nameof(ThumbColor), typeof(Color), typeof(CustomSwitch), Colors.Black);

    public static readonly BindableProperty SwitchWidthProperty =
        BindableProperty.Create(nameof(SwitchWidth), typeof(double), typeof(CustomSwitch), 50.0);

    public static readonly BindableProperty SwitchHeightProperty =
        BindableProperty.Create(nameof(SwitchHeight), typeof(double), typeof(CustomSwitch), 30.0);

    public static readonly BindableProperty ThumbSizeProperty =
        BindableProperty.Create(nameof(ThumbSize), typeof(double), typeof(CustomSwitch), 26.0);

    public static readonly BindableProperty AnimationLengthProperty =
        BindableProperty.Create(nameof(AnimationLength), typeof(uint), typeof(CustomSwitch), (uint)120);

    public bool IsToggled
    {
        get => (bool)GetValue(IsToggledProperty);
        set => SetValue(IsToggledProperty, value);
    }

    public Color OnColor
    {
        get => (Color)GetValue(OnColorProperty);
        set => SetValue(OnColorProperty, value);
    }

    public Color OffColor
    {
        get => (Color)GetValue(OffColorProperty);
        set => SetValue(OffColorProperty, value);
    }

    public Color ThumbColor
    {
        get => (Color)GetValue(ThumbColorProperty);
        set => SetValue(ThumbColorProperty, value);
    }

    public double SwitchWidth
    {
        get => (double)GetValue(SwitchWidthProperty);
        set => SetValue(SwitchWidthProperty, value);
    }

    public double SwitchHeight
    {
        get => (double)GetValue(SwitchHeightProperty);
        set => SetValue(SwitchHeightProperty, value);
    }

    public double ThumbSize
    {
        get => (double)GetValue(ThumbSizeProperty);
        set => SetValue(ThumbSizeProperty, value);
    }

    public uint AnimationLength
    {
        get => (uint)GetValue(AnimationLengthProperty);
        set => SetValue(AnimationLengthProperty, value);
    }

    private double TrackCornerRadius => SwitchHeight / 2.0;
    private double ThumbCornerRadius => ThumbSize / 2.0;

    public Color CurrentTrackColor => IsToggled ? OnColor : OffColor;

    public bool IsOn { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public Color TrackColor => throw new NotImplementedException();

    public event EventHandler<ToggledEventArgs>? Toggled;

    public CustomSwitch()
    {
        InitializeComponent();

        var tap = new TapGestureRecognizer();
        tap.Tapped += (s, e) => ToggleSwitch();
        Root.GestureRecognizers.Add(tap);

        UpdateUI(false);
    }

    private static void OnIsToggledChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is CustomSwitch control)
            control.UpdateUI(true);
    }

    private void ToggleSwitch()
    {
        IsToggled = !IsToggled;
        Toggled?.Invoke(this, new ToggledEventArgs(IsToggled));
    }

    private void UpdateUI(bool animate)
    {
        Track.BackgroundColor = CurrentTrackColor;
        Thumb.BackgroundColor = ThumbColor;
        Thumb.StrokeShape = new RoundRectangle
        {
            CornerRadius = new CornerRadius(ThumbCornerRadius)
        };

        double targetX = IsToggled
            ? SwitchWidth - ThumbSize - 2
            : 2;

        if (animate)
            Thumb.TranslateTo(targetX, 0, AnimationLength, Easing.CubicInOut);
        else
            Thumb.TranslationX = targetX;
    }
}
