namespace SkiaSharpSample.Controls;

internal sealed class SortMenuView : OptionMenuView
{
    public SortMenuView(GalleryFilters filters)
        : base("Sort samples", "gallery-sort-menu", 228,
        [
            new MenuOptionItem("Featured", "gallery-sort-featured",
                filters.SortOrder == SampleSortOrder.NewestFirst,
                () => filters.SetSort(SampleSortOrder.NewestFirst)),
            new MenuOptionItem("Oldest first", "gallery-sort-oldest",
                filters.SortOrder == SampleSortOrder.OldestFirst,
                () => filters.SetSort(SampleSortOrder.OldestFirst)),
            new MenuOptionItem("A to Z", "gallery-sort-alphabetical",
                filters.SortOrder == SampleSortOrder.Alphabetical,
                () => filters.SetSort(SampleSortOrder.Alphabetical)),
            new MenuOptionItem("Category", "gallery-sort-category",
                filters.SortOrder == SampleSortOrder.Category,
                () => filters.SetSort(SampleSortOrder.Category))
        ])
    {
    }
}
