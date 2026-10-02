using Microsoft.Maui.Controls;

namespace SkiaSharpSample.Controls;

public partial class TagFacetView : ContentView
{
    public TagFacetView()
    {
        InitializeComponent();
        BindingContextChanged += (_, _) =>
        {
            if (BindingContext is TagFacet facet)
                ToolTipProperties.SetText(this, facet.Tag);
        };
    }

    private void OnClicked(object? sender, EventArgs e) =>
        (BindingContext as TagFacet)?.Toggle();
}
