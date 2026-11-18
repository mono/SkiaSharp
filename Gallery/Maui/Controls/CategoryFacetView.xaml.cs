using Microsoft.Maui.Controls;

namespace SkiaSharpSample.Controls;

public partial class CategoryFacetView : Grid
{
    public CategoryFacetView() => InitializeComponent();

    private void OnTapped(object? sender, TappedEventArgs e) =>
        (BindingContext as CategoryFacet)?.Select();

    private void OnClicked(object? sender, EventArgs e) =>
        (BindingContext as CategoryFacet)?.Select();

    private void OnPointerEntered(object? sender, PointerEventArgs e) =>
        VisualStateManager.GoToState(RowSurface, "PointerOver");

    private void OnPointerExited(object? sender, PointerEventArgs e) =>
        VisualStateManager.GoToState(RowSurface, "Normal");
}
