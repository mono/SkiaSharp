using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace SkiaSharpSample.Controls;

internal sealed class FilterRailView : ContentView, IDisposable
{
    private readonly GalleryFilters filters;
    private readonly Entry search;
    private readonly Entry apiSearch;
    private readonly Label resultCount;
    private readonly VerticalStackLayout categoryRows = new() { Spacing = 2 };
    private readonly FlexLayout typePills = new() { Wrap = FlexWrap.Wrap, AlignItems = FlexAlignItems.Start };
    private readonly FlexLayout methodPills = new() { Wrap = FlexWrap.Wrap, AlignItems = FlexAlignItems.Start };
    private readonly Button typeHeading;
    private readonly Button methodHeading;
    private bool typesOpen = true;
    private bool methodsOpen;
    private bool disposed;

    public FilterRailView(GalleryFilters filters, bool popup, Action? close = null)
    {
        this.filters = filters;
        AutomationId = popup ? "gallery-filter-popup" : "gallery-filter-sidebar";
        WidthRequest = popup ? 332 : 266;
        HeightRequest = popup ? 520 : -1;
        var layout = new Grid
        {
            RowDefinitions = new RowDefinitionCollection { new(GridLength.Auto), new(GridLength.Star), new(GridLength.Auto) },
            Padding = popup ? new Thickness(4, 2, 4, 4) : new Thickness(18, 10, 12, 12)
        };
        var header = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Auto) } };
        resultCount = GalleryUi.Text("Filters", 16, true);
        resultCount.AutomationId = popup ? "gallery-filter-popup-count" : "gallery-filter-sidebar-count";
        header.Add(resultCount);
        if (popup)
        {
            var closeButton = GalleryUi.IconButton(GalleryUi.IconClose, "gallery-filter-close", "Close filters", (_, _) => close?.Invoke());
            header.Add(closeButton, 1);
        }
        layout.Add(header);

        var body = new VerticalStackLayout { Spacing = 9, Padding = new Thickness(0, 10, 0, 10) };
        search = new Entry { Placeholder = "Search samples and APIs", Text = filters.SearchText, AutomationId = popup ? "gallery-filter-popup-search" : "gallery-filter-search", FontSize = 13 };
        GalleryUi.Background(search, "CardBackground");
        GalleryUi.TextColor(search, Entry.TextColorProperty, "PrimaryText");
        GalleryUi.TextColor(search, Entry.PlaceholderColorProperty, "SecondaryText");
        search.TextChanged += (_, e) => filters.SetSearch(e.NewTextValue);
        body.Children.Add(SearchField(search));

        body.Children.Add(Heading("CATEGORIES"));
        body.Children.Add(categoryRows);

        body.Children.Add(Heading("API TAGS"));
        apiSearch = new Entry { Placeholder = "Find an API type or method", AutomationId = popup ? "gallery-filter-popup-api-search" : "gallery-filter-api-search", FontSize = 12 };
        GalleryUi.Background(apiSearch, "CardBackground");
        GalleryUi.TextColor(apiSearch, Entry.TextColorProperty, "PrimaryText");
        GalleryUi.TextColor(apiSearch, Entry.PlaceholderColorProperty, "SecondaryText");
        apiSearch.TextChanged += (_, _) => RefreshTags();
        body.Children.Add(SearchField(apiSearch));

        typeHeading = HeadingButton("Types", "gallery-types-toggle", () =>
        {
            typesOpen = !typesOpen;
            RefreshTags();
        });
        body.Children.Add(typeHeading);
        body.Children.Add(typePills);
        methodHeading = HeadingButton("Methods", "gallery-methods-toggle", () =>
        {
            methodsOpen = !methodsOpen;
            RefreshTags();
        });
        body.Children.Add(methodHeading);
        body.Children.Add(methodPills);

        var scroll = new ScrollView { Content = body, VerticalScrollBarVisibility = ScrollBarVisibility.Default };
        layout.Add(scroll, 0, 1);
        var actions = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Auto) }, Padding = new Thickness(0, 8, 0, 0) };
        var reset = GalleryUi.Pill("Reset all", "gallery-filter-reset", (_, _) => filters.Clear());
        reset.HorizontalOptions = LayoutOptions.Start;
        actions.Add(reset);
        if (popup)
        {
            var done = GalleryUi.Action("Done", "gallery-filter-done", (_, _) => close?.Invoke());
            actions.Add(done, 1);
        }
        layout.Add(actions, 0, 2);
        Content = layout;
        filters.Changed += FiltersChanged;
        RefreshView();
    }

    private static Label Heading(string name)
    {
        var heading = GalleryUi.Text(name, 11, true, "SecondaryText");
        heading.CharacterSpacing = 1.2;
        return heading;
    }

    private static Grid SearchField(Entry entry)
    {
        var field = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Auto), new(GridLength.Star) } };
        var icon = new Image
        {
            Source = new FontImageSource
            {
                Glyph = GalleryUi.IconSearch, FontFamily = "BootstrapIcons",
                Color = GalleryUi.Accent, Size = 15
            },
            WidthRequest = 24, InputTransparent = true
        };
        field.Add(icon);
        field.Add(entry, 1);
        return field;
    }

    private static Button HeadingButton(string name, string id, Action toggle)
    {
        var button = GalleryUi.Pill(name, id, (_, _) => toggle());
        button.HorizontalOptions = LayoutOptions.Fill;
        button.ImageSource = new FontImageSource
        {
            Glyph = GalleryUi.IconChevronDown, FontFamily = "BootstrapIcons",
            Color = GalleryUi.Accent, Size = 12
        };
        button.ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Right, 8);
        return button;
    }

    private void FiltersChanged(object? sender, EventArgs e) => RefreshView();

    private void RefreshView()
    {
        if (disposed) return;
        if (search.Text != filters.SearchText) search.Text = filters.SearchText;
        resultCount.Text = $"Filters  ·  {filters.Results.Length} results";
        RefreshCategories();
        RefreshTags();
    }

    private void RefreshCategories()
    {
        categoryRows.Children.Clear();
        categoryRows.Children.Add(CategoryRow(
            "All categories", "\uf3fa", filters.AllSamples.Length,
            "category-all", filters.SelectedCategory is null,
            () => filters.SelectCategory(null)));
        foreach (var category in SampleManager.GetCategories())
        {
            var count = SampleManager.GetSampleCount(category.Name, filters.AllSamples);
            if (count == 0) continue;
            categoryRows.Children.Add(CategoryRow(
                category.Name, category.UnicodeIcon, count,
                GalleryUi.StableId("category-", category.Name),
                filters.SelectedCategory == category.Name,
                () => filters.SelectCategory(category.Name)));
        }
    }

    private static Grid CategoryRow(string name, string glyph, int count, string id, bool selected, Action select)
    {
        var content = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new(new GridLength(22)), new(GridLength.Star), new(GridLength.Auto)
            },
            ColumnSpacing = 6
        };
        var icon = GalleryUi.Text(glyph, 15);
        icon.FontFamily = "BootstrapIcons";
        icon.VerticalOptions = LayoutOptions.Center;
        if (id == "category-all") GalleryUi.TextColor(icon, Label.TextColorProperty, "AccentText");
        else GalleryUi.CategoryColor(icon, name);
        content.Add(icon);
        var title = GalleryUi.Text(name, 13, selected);
        title.LineBreakMode = LineBreakMode.TailTruncation;
        title.VerticalOptions = LayoutOptions.Center;
        content.Add(title, 1);
        var tally = GalleryUi.Text(count.ToString(), 11, color: "SecondaryText");
        tally.VerticalOptions = LayoutOptions.Center;
        tally.HorizontalOptions = LayoutOptions.End;
        content.Add(tally, 2);

        var visual = GalleryUi.Card(content, 8);
        visual.Padding = new Thickness(9, 5);
        visual.MinimumHeightRequest = 37;
        GalleryUi.Background(visual, selected ? "SubtleBackground" : "CardBackground");
        visual.StrokeThickness = selected ? 1 : 0;
        if (selected) visual.Stroke = new SolidColorBrush(GalleryUi.Accent);
        visual.AutomationId = GalleryUi.StableId("category-row-", name);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => select();
        visual.GestureRecognizers.Add(tap);

        var hit = new Button
        {
            Text = name, TextColor = Colors.Transparent, BackgroundColor = Colors.Transparent,
            BorderWidth = 0, Padding = 0, MinimumHeightRequest = 37,
            AutomationId = id
        };
        SemanticProperties.SetDescription(hit, $"{name}, {count} samples; {(selected ? "selected" : "not selected")}");
        hit.Clicked += (_, _) => select();
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => GalleryUi.Background(visual, "SubtleBackground");
        pointer.PointerExited += (_, _) => GalleryUi.Background(visual, selected ? "SubtleBackground" : "CardBackground");
        hit.GestureRecognizers.Add(pointer);
        var row = new Grid();
        row.Add(visual);
        row.Add(hit);
        return row;
    }

    private void RefreshTags()
    {
        var query = apiSearch.Text?.Trim() ?? "";
        typeHeading.Text = "Types";
        methodHeading.Text = "Methods";
        SemanticProperties.SetDescription(typeHeading, $"API types, {(typesOpen ? "expanded" : "collapsed")}");
        SemanticProperties.SetDescription(methodHeading, $"API methods, {(methodsOpen ? "expanded" : "collapsed")}");
        typePills.IsVisible = typesOpen;
        methodPills.IsVisible = methodsOpen;
        FillTags(typePills, TagKind.Type, query);
        FillTags(methodPills, TagKind.Method, query);
    }

    private void FillTags(FlexLayout host, TagKind kind, string query)
    {
        host.Children.Clear();
        if (!host.IsVisible) return;
        var candidates = filters.AllTags
            .Where(t => t.Kind == kind &&
                (query.Length == 0 || t.Tag.Contains(query, StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(t => filters.Tags.Contains(t.Tag))
            .ThenByDescending(t => filters.LiveTagCounts.GetValueOrDefault(t.Tag))
            .ThenByDescending(t => t.Count)
            .ThenBy(t => t.Tag)
            .ToArray();
        foreach (var (tag, count, _) in candidates)
        {
            var label = KnownApis.GetDisplayName(tag);
            var live = filters.LiveTagCounts.GetValueOrDefault(tag);
            var pill = GalleryUi.Pill($"{label}  {live}", GalleryUi.StableId("tag-", tag), (_, _) => filters.ToggleTag(tag));
            pill.FontSize = 11;
            pill.Margin = new Thickness(0, 0, 3, 3);
            GalleryUi.SelectPill(pill, filters.Tags.Contains(tag), kind);
            SemanticProperties.SetDescription(pill, $"{tag}; {live} current results, {count} total; {(filters.Tags.Contains(tag) ? "selected" : "not selected")}");
            host.Children.Add(pill);
        }
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        filters.Changed -= FiltersChanged;
    }
}
