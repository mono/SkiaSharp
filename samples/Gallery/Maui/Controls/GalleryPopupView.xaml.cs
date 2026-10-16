using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace SkiaSharpSample.Controls;

public partial class GalleryPopupView : ContentView
{
    public static readonly BindableProperty PopupBoundsProperty = BindableProperty.Create(
        nameof(PopupBounds), typeof(Rect), typeof(GalleryPopupView), new Rect(0, 0, 0, 0));

    private Border? panel;

    public GalleryPopupView() => InitializeComponent();

    public event EventHandler? DismissRequested;

    public Rect PopupBounds
    {
        get => (Rect)GetValue(PopupBoundsProperty);
        set => SetValue(PopupBoundsProperty, value);
    }

    public View? MenuContent
    {
        get => Content as View;
        set => Content = value;
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        panel = GetTemplateChild("PART_Panel") as Border;
    }

    public Size MeasureMenu(double width, double height) =>
        panel?.Measure(width, height) ?? MenuContent?.Measure(width, height) ?? new Size(width, height);

    public void SetMenuPosition(Rect bounds)
    {
        if (PopupBounds != bounds)
            PopupBounds = bounds;
    }

    private void BackdropTapped(object? sender, TappedEventArgs e) =>
        DismissRequested?.Invoke(this, EventArgs.Empty);
}
