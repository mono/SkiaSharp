using Microsoft.Maui.Controls;

namespace SkiaSharpSample.Controls;

public partial class GallerySampleCard : Border
{
    public GallerySampleCard() => InitializeComponent();

    private async void CardTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is GalleryCardItem { Supported: true } card)
            await card.OpenAsync();
    }
}
