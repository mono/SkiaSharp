using Microsoft.Maui.Controls;

namespace SkiaSharpSample.Controls;

internal sealed class SortMenuView : ContentView
{
    public SortMenuView(GalleryFilters filters)
    {
        AutomationId = "gallery-sort-menu";
        WidthRequest = 228;
        var menu = new VerticalStackLayout { Spacing = 5 };
        var heading = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Auto) }
        };
        var title = GalleryUi.Text("Sort samples", 14, true);
        title.VerticalOptions = LayoutOptions.Center;
        heading.Add(title);
        heading.Add(GalleryUi.IconButton(GalleryUi.IconClose, "gallery-sort-close", "Close sorting menu", (_, _) => { }), 1);
        menu.Children.Add(heading);
        foreach (var (order, label, id) in new[]
        {
            (SampleSortOrder.NewestFirst, "Featured", "gallery-sort-featured"),
            (SampleSortOrder.OldestFirst, "Oldest first", "gallery-sort-oldest"),
            (SampleSortOrder.Alphabetical, "A to Z", "gallery-sort-alphabetical"),
            (SampleSortOrder.Category, "Category", "gallery-sort-category")
        })
        {
            var chosen = filters.SortOrder == order;
            var option = new Button
            {
                Text = $"{(chosen ? "✓  " : "     ")}{label}",
                AutomationId = id,
                Style = (Style)Application.Current!.Resources["GalleryRow"]
            };
            GalleryUi.SelectRow(option, chosen);
            SemanticProperties.SetDescription(option, $"{label}, {(chosen ? "selected" : "not selected")}");
            option.Clicked += (_, _) => filters.SetSort(order);
            menu.Children.Add(option);
        }
        Content = menu;
    }
}
