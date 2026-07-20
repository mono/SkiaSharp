using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace SkiaSharpSample.Controls;

public sealed partial class SampleCard : UserControl
{
    public event System.EventHandler? Invoked;

    public SampleCard()
    {
        this.InitializeComponent();
        // handledEventsToo: child TextBlocks mark Pointer events Handled under SkiaRenderer.
        this.AddHandler(
            UIElement.PointerReleasedEvent,
            new PointerEventHandler((_, _) => Invoked?.Invoke(this, System.EventArgs.Empty)),
            handledEventsToo: true);
    }

    public static readonly DependencyProperty SampleProperty =
        DependencyProperty.Register(nameof(Sample), typeof(SampleBase), typeof(SampleCard),
            new PropertyMetadata(null, OnAnyChanged));

    public SampleBase? Sample
    {
        get => (SampleBase?)GetValue(SampleProperty);
        set => SetValue(SampleProperty, value);
    }

    public static readonly DependencyProperty CategoryColorProperty =
        DependencyProperty.Register(nameof(CategoryColor), typeof(SolidColorBrush), typeof(SampleCard),
            new PropertyMetadata(null, OnAnyChanged));

    public SolidColorBrush? CategoryColor
    {
        get => (SolidColorBrush?)GetValue(CategoryColorProperty);
        set => SetValue(CategoryColorProperty, value);
    }

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(string), typeof(SampleCard),
            new PropertyMetadata(string.Empty, OnAnyChanged));

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly DependencyProperty SupportedProperty =
        DependencyProperty.Register(nameof(Supported), typeof(bool), typeof(SampleCard),
            new PropertyMetadata(true, OnAnyChanged));

    public bool Supported
    {
        get => (bool)GetValue(SupportedProperty);
        set => SetValue(SupportedProperty, value);
    }

    private static void OnAnyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SampleCard card) card.ApplyBindings();
    }

    private void ApplyBindings()
    {
        var sample = Sample;
        if (sample is null) return;

        IconText.Text = Icon;
        TitleText.Text = sample.Title;
        DescriptionText.Text = Truncate(sample.Description);
        CategoryText.Text = sample.Category;

        var color = CategoryColor?.Color ?? Color.FromArgb(0xFF, 0x54, 0x6E, 0x7A);

        IconText.Foreground = Supported
            ? new SolidColorBrush(color)
            : new SolidColorBrush(Color.FromArgb(0xFF, 0xBB, 0xBB, 0xBB));

        AccentBar.Fill = new SolidColorBrush(color);
        CardRoot.Background = new SolidColorBrush(Color.FromArgb(0x0D, color.R, color.G, color.B));
        CategoryBadge.Background = new SolidColorBrush(Color.FromArgb(0x1F, color.R, color.G, color.B));
        CategoryText.Foreground = new SolidColorBrush(color);

        (string label, Color bg, Color fg) = sample switch
        {
            DocumentSampleBase => ("Document", Color.FromArgb(0xFF, 0xEC, 0xEF, 0xF1), Color.FromArgb(0xFF, 0x60, 0x7D, 0x8B)),
            CanvasSampleBase { IsAnimated: true } => ("Animated", Color.FromArgb(0xFF, 0xFF, 0xF3, 0xE0), Color.FromArgb(0xFF, 0xE6, 0x51, 0x00)),
            _ => ("Canvas", Color.FromArgb(0xFF, 0xE3, 0xF2, 0xFD), Color.FromArgb(0xFF, 0x15, 0x65, 0xC0)),
        };
        TypeText.Text = label;
        TypeBadge.Background = new SolidColorBrush(bg);
        TypeText.Foreground = new SolidColorBrush(fg);

        UnsupportedBadge.Visibility = Supported ? Visibility.Collapsed : Visibility.Visible;
        Opacity = Supported ? 1.0 : 0.5;
    }

    private static string Truncate(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        return text.Length > 120 ? text[..117] + "..." : text;
    }
}
