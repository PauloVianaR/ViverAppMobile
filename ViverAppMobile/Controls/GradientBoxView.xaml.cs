using Microsoft.Maui.Controls.Shapes;

namespace ViverAppMobile.Controls;

public partial class GradientBoxView : ContentView
{
    public GradientBoxView()
    {
        InitializeComponent();

        this.BindingContextChanged += (s, e) => MainBorder.BindingContext = BindingContext;

        UpdateGradient();
    }

    public static readonly BindableProperty StartColorProperty =
        BindableProperty.Create(
            nameof(StartColor),
            typeof(Color),
            typeof(GradientBoxView),
            Colors.Transparent,
            propertyChanged: OnGradientPropertyChanged);

    public Color StartColor
    {
        get => (Color)GetValue(StartColorProperty);
        set => SetValue(StartColorProperty, value);
    }

    public static readonly BindableProperty EndColorProperty =
        BindableProperty.Create(
            nameof(EndColor),
            typeof(Color),
            typeof(GradientBoxView),
            Colors.Transparent,
            propertyChanged: OnGradientPropertyChanged);

    public Color EndColor
    {
        get => (Color)GetValue(EndColorProperty);
        set => SetValue(EndColorProperty, value);
    }

    public static readonly BindableProperty GradientDirectionProperty =
        BindableProperty.Create(
            nameof(GradientDirection),
            typeof(GradientDirection),
            typeof(GradientBoxView),
            GradientDirection.Horizontal,
            propertyChanged: OnGradientPropertyChanged);

    public GradientDirection GradientDirection
    {
        get => (GradientDirection)GetValue(GradientDirectionProperty);
        set => SetValue(GradientDirectionProperty, value);
    }

    public static readonly BindableProperty CornerRadiusProperty =
        BindableProperty.Create(
            nameof(CornerRadius),
            typeof(float),
            typeof(GradientBoxView),
            0f,
            propertyChanged: (b, o, n) =>
            {
                ((GradientBoxView)b).MainBorder.StrokeShape =
                    new RoundRectangle { CornerRadius = (float)n };
            });

    public float CornerRadius
    {
        get => (float)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    private static void OnGradientPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((GradientBoxView)bindable).UpdateGradient();
    }

    private void UpdateGradient()
    {
        if (GradientBrush == null)
            return;

        GradientBrush.GradientStops.Clear();
        GradientBrush.GradientStops.Add(new GradientStop(StartColor, 0f));
        GradientBrush.GradientStops.Add(new GradientStop(EndColor, 1f));

        switch (GradientDirection)
        {
            case GradientDirection.Vertical:
                GradientBrush.StartPoint = new Point(0, 0);
                GradientBrush.EndPoint = new Point(0, 1);
                break;

            case GradientDirection.Diagonal:
                GradientBrush.StartPoint = new Point(0, 0);
                GradientBrush.EndPoint = new Point(1, 1);
                break;

            default:
                GradientBrush.StartPoint = new Point(0, 0);
                GradientBrush.EndPoint = new Point(1, 0);
                break;
        }
    }
}

public enum GradientDirection
{
    Horizontal,
    Vertical,
    Diagonal
}