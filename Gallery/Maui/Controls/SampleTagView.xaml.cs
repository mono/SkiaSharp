using Microsoft.Maui.Controls;

namespace SkiaSharpSample.Controls;

public partial class SampleTagView : Border
{
    public SampleTagView()
    {
        InitializeComponent();
        BindingContextChanged += (_, _) =>
        {
            if (BindingContext is SampleTagItem item)
                ToolTipProperties.SetText(this, item.Tag);
        };
    }
}
