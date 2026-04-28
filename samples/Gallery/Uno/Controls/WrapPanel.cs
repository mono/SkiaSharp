using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace SkiaSharpSample.Controls;

// WinUI 3 doesn't ship a WrapPanel.
public sealed class WrapPanel : Panel
{
    public static readonly DependencyProperty HorizontalSpacingProperty =
        DependencyProperty.Register(nameof(HorizontalSpacing), typeof(double), typeof(WrapPanel),
            new PropertyMetadata(0.0, OnSpacingChanged));

    public static readonly DependencyProperty VerticalSpacingProperty =
        DependencyProperty.Register(nameof(VerticalSpacing), typeof(double), typeof(WrapPanel),
            new PropertyMetadata(0.0, OnSpacingChanged));

    public double HorizontalSpacing
    {
        get => (double)GetValue(HorizontalSpacingProperty);
        set => SetValue(HorizontalSpacingProperty, value);
    }

    public double VerticalSpacing
    {
        get => (double)GetValue(VerticalSpacingProperty);
        set => SetValue(VerticalSpacingProperty, value);
    }

    private static void OnSpacingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WrapPanel p) p.InvalidateMeasure();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var childAvailable = new Size(availableSize.Width, double.PositiveInfinity);
        double rowWidth = 0, rowHeight = 0, totalHeight = 0, maxRowWidth = 0;

        foreach (var child in Children)
        {
            child.Measure(childAvailable);
            var ds = child.DesiredSize;
            if (rowWidth + ds.Width > availableSize.Width && rowWidth > 0)
            {
                maxRowWidth = System.Math.Max(maxRowWidth, rowWidth - HorizontalSpacing);
                totalHeight += rowHeight + VerticalSpacing;
                rowWidth = 0;
                rowHeight = 0;
            }
            rowWidth += ds.Width + HorizontalSpacing;
            rowHeight = System.Math.Max(rowHeight, ds.Height);
        }
        maxRowWidth = System.Math.Max(maxRowWidth, rowWidth - HorizontalSpacing);
        totalHeight += rowHeight;
        return new Size(System.Math.Max(0, maxRowWidth), totalHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double x = 0, y = 0, rowHeight = 0;

        foreach (var child in Children)
        {
            var ds = child.DesiredSize;
            if (x + ds.Width > finalSize.Width && x > 0)
            {
                x = 0;
                y += rowHeight + VerticalSpacing;
                rowHeight = 0;
            }
            child.Arrange(new Rect(x, y, ds.Width, ds.Height));
            x += ds.Width + HorizontalSpacing;
            rowHeight = System.Math.Max(rowHeight, ds.Height);
        }
        return finalSize;
    }
}
