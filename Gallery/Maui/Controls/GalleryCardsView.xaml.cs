using Microsoft.Maui.Controls;

namespace SkiaSharpSample.Controls;

public partial class GalleryCardsView : CollectionView
{
    internal GalleryCardsView(int span, SampleBase[] corpus, Func<SampleBase, Task> open)
    {
        InitializeComponent();
        ItemTemplate = new DataTemplate(() => new GalleryCardHost(corpus, open));
        ItemsLayout = new GridItemsLayout(span, ItemsLayoutOrientation.Vertical)
        {
            VerticalItemSpacing = 12, HorizontalItemSpacing = 12
        };
    }
}
