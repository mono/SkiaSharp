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
    private readonly Button mode;
    private bool typesOpen = true;
    private bool methodsOpen;
    private bool allTypes;
    private bool allMethods;
    private bool disposed;

    public FilterRailView(GalleryFilters filters, bool popup, Action? close = null)
    {
        this.filters = filters;
        AutomationId = popup ? "gallery-filter-popup" : "gallery-filter-sidebar";
        WidthRequest = popup ? 332 : 266;
        HeightRequest = popup ? 520 : -1;
        var layout = new Grid
        {
            RowDefinitions = new RowDefinitionCollection { new(GridLength.Auto), new(GridLength.Star), new(GridLength.Auto) }
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

        var body = new VerticalStackLayout { Spacing = 12, Padding = new Thickness(0, 10, 4, 10) };
        search = new Entry { Placeholder = "Search samples and APIs", Text = filters.SearchText, AutomationId = popup ? "gallery-filter-popup-search" : "gallery-filter-search", FontSize = 13 };
        GalleryUi.Background(search, "CardBackground");
        GalleryUi.TextColor(search, Entry.TextColorProperty, "PrimaryText");
        GalleryUi.TextColor(search, Entry.PlaceholderColorProperty, "SecondaryText");
        search.TextChanged += (_, e) => filters.SetSearch(e.NewTextValue);
        body.Children.Add(SearchField(search));

        body.Children.Add(Heading("CATEGORIES"));
        body.Children.Add(categoryRows);

        var tagHeading = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Auto) } };
        tagHeading.Add(Heading("API TAGS"));
        mode = GalleryUi.Pill("ANY", "gallery-tag-mode", (_, _) => filters.SetTagMode(!filters.MatchAll));
        mode.MinimumHeightRequest = 34;
        SemanticProperties.SetDescription(mode, "Match selected tags using any or all");
        tagHeading.Add(mode, 1);
        body.Children.Add(tagHeading);
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
        mode.Text = filters.MatchAll ? "ALL" : "ANY";
        GalleryUi.SelectPill(mode, filters.MatchAll);
        RefreshCategories();
        RefreshTags();
    }

    private void RefreshCategories()
    {
        categoryRows.Children.Clear();
        var clear = new Button
        {
            Text = $"All categories  ·  {filters.AllSamples.Length}",
            AutomationId = "category-all",
            Style = (Style)Application.Current!.Resources["GalleryRow"]
        };
        GalleryUi.SelectRow(clear, filters.Categories.Count == 0);
        clear.Clicked += (_, _) => filters.ClearCategories();
        categoryRows.Children.Add(clear);
        foreach (var category in SampleManager.GetCategories())
        {
            var count = SampleManager.GetSampleCount(category.Name, filters.AllSamples);
            if (count == 0) continue;
            var row = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection { new(new GridLength(20)), new(GridLength.Star) }
            };
            var icon = GalleryUi.Text(category.UnicodeIcon, 15);
            icon.FontFamily = "BootstrapIcons";
            icon.VerticalOptions = LayoutOptions.Center;
            icon.InputTransparent = true;
            GalleryUi.CategoryColor(icon, category.Name);
            row.Add(icon);
            var button = new Button
            {
                Text = $"{category.Name}  ·  {count}",
                AutomationId = GalleryUi.StableId("category-", category.Name),
                Style = (Style)Application.Current!.Resources["GalleryRow"]
            };
            GalleryUi.SelectRow(button, filters.Categories.Contains(category.Name));
            SemanticProperties.SetDescription(button, $"{category.Name}, {count} samples; {(filters.Categories.Contains(category.Name) ? "selected" : "not selected")}");
            button.Clicked += (_, _) => filters.ToggleCategory(category.Name);
            row.Add(button, 1);
            categoryRows.Children.Add(row);
        }
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
        FillTags(typePills, TagKind.Type, query, allTypes, () => { allTypes = true; RefreshTags(); });
        FillTags(methodPills, TagKind.Method, query, allMethods, () => { allMethods = true; RefreshTags(); });
    }

    private void FillTags(FlexLayout host, TagKind kind, string query, bool showAll, Action expand)
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
        var limit = showAll || query.Length > 0 ? candidates.Length : kind == TagKind.Type ? 18 : 12;
        foreach (var (tag, count, _) in candidates.Take(limit))
        {
            var label = KnownApis.GetDisplayName(tag);
            var live = filters.LiveTagCounts.GetValueOrDefault(tag);
            var pill = GalleryUi.Pill($"{label}  {live}", GalleryUi.StableId("tag-", tag), (_, _) => filters.ToggleTag(tag));
            pill.FontSize = 11;
            pill.MinimumHeightRequest = 38;
            pill.Margin = new Thickness(0, 0, 5, 5);
            GalleryUi.SelectPill(pill, filters.Tags.Contains(tag), kind);
            SemanticProperties.SetDescription(pill, $"{tag}; {live} current results, {count} total; {(filters.Tags.Contains(tag) ? "selected" : "not selected")}");
            host.Children.Add(pill);
        }
        if (limit < candidates.Length)
        {
            var more = GalleryUi.Pill($"Show all {candidates.Length}…", $"gallery-{kind.ToString().ToLowerInvariant()}-more", (_, _) => expand());
            more.Margin = new Thickness(0, 0, 5, 5);
            host.Children.Add(more);
        }
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        filters.Changed -= FiltersChanged;
    }
}
