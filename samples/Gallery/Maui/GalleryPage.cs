using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using SkiaSharpSample.Controls;
using SkiaSharpSample.Services;

namespace SkiaSharpSample;

public sealed class GalleryPage : ContentPage
{
    private readonly SampleService service;
    private readonly GalleryWindowSettings windowSettings;
    private readonly SampleBase[] all;
    private readonly GalleryFilters filters;
    private readonly Grid root;
    private readonly Grid main;
    private readonly FilterRailView rail;
    private readonly Grid compactSearch;
    private readonly HorizontalStackLayout activeChips = new() { Spacing = 6 };
    private readonly ScrollView activeStrip;
    private readonly SearchBar search;
    private readonly Button sortTrigger;
    private readonly Label filterBadge;
    private readonly Grid listArea;
    private CollectionView gallery;
    private readonly Label count;
    private readonly Label empty;
    private int columns = 1;
    private bool wide;
    private bool navigating;
    private readonly SemaphoreSlim navigationGate = new(1, 1);

    public GalleryPage(SampleService service)
        : this(service, new GalleryWindowSettings(service))
    {
    }

    internal GalleryPage(SampleService service, GalleryWindowSettings windowSettings)
    {
        this.service = service;
        this.windowSettings = windowSettings;
        all = service.GetAllSamples().ToArray();
        filters = new GalleryFilters(all);
        Title = "Gallery";
        NavigationPage.SetHasNavigationBar(this, false);
        BackgroundColor = GalleryUi.Navy;
        SafeAreaEdges = new Microsoft.Maui.SafeAreaEdges(Microsoft.Maui.SafeAreaRegions.None);
        var hero = new GalleryHeader(windowSettings);

        var info = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Auto) },
            Padding = new Thickness(16, 8)
        };
        GalleryUi.Background(info, "PageBackground");
        info.Add(GalleryUi.Text("Explore samples by API type, rendering technique, or concept.", 12, color: "SecondaryText"));
        info.Add(GalleryUi.Text($"{filters.AllTags.Count} APIs covered", 12, true, "AccentText"), 1);

        main = new Grid();
        GalleryUi.Background(main, "PageBackground");
        rail = new FilterRailView(filters, popup: false);
        rail.WidthRequest = -1;
        rail.IsVisible = false;
        main.Add(rail);

        var results = new Grid
        {
            RowDefinitions = new RowDefinitionCollection { new(GridLength.Auto), new(GridLength.Auto), new(GridLength.Star) }
        };
        compactSearch = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Auto) },
            Padding = new Thickness(12, 6, 12, 2), ColumnSpacing = 6
        };
        search = new SearchBar { Placeholder = "Search samples and APIs", AutomationId = "gallery-search", FontSize = 13 };
        GalleryUi.Background(search, "CardBackground");
        GalleryUi.TextColor(search, SearchBar.TextColorProperty, "PrimaryText");
        GalleryUi.TextColor(search, SearchBar.PlaceholderColorProperty, "SecondaryText");
        search.TextChanged += (_, e) => filters.SetSearch(e.NewTextValue);
        compactSearch.Add(search);
        var filterTrigger = GalleryUi.IconButton(GalleryUi.IconFunnel, "gallery-filter-trigger", "Open gallery filters", (_, _) => { });
        GalleryPopup.SetCloseOnAction(filterTrigger, false);
        GalleryPopup.SetContentFactory(filterTrigger, () => new FilterRailView(filters, popup: true, () => GalleryPopup.Dismiss(root)));
        var filterArea = new Grid { WidthRequest = 50 };
        filterArea.Add(filterTrigger);
        filterBadge = GalleryUi.Text("", 10, true, "AccentText");
        filterBadge.BackgroundColor = Colors.White;
        filterBadge.HorizontalOptions = LayoutOptions.End;
        filterBadge.VerticalOptions = LayoutOptions.Start;
        filterBadge.InputTransparent = true;
        filterArea.Add(filterBadge);
        compactSearch.Add(filterArea, 1);
        results.Add(compactSearch);

        var status = new Grid
        {
            Padding = new Thickness(14, 5), ColumnSpacing = 8,
            ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Auto) }
        };
        count = GalleryUi.Text("", 13, true);
        count.AutomationId = "gallery-result-count";
        count.VerticalOptions = LayoutOptions.Center;
        status.Add(count);
        var sortArea = new Grid { WidthRequest = 150 };
        sortTrigger = GalleryUi.Pill("Featured", "gallery-sort-trigger", (_, _) => { });
        sortTrigger.Padding = new Thickness(30, 3, 6, 3);
        sortArea.Add(sortTrigger);
        sortArea.Add(new Image
        {
            Source = new FontImageSource { Glyph = GalleryUi.IconSort, FontFamily = "BootstrapIcons", Size = 15, Color = GalleryUi.Accent },
            WidthRequest = 18, Margin = new Thickness(8, 0, 0, 0), HorizontalOptions = LayoutOptions.Start,
            InputTransparent = true
        });
        GalleryPopup.SetCloseOnAction(sortTrigger, true);
        GalleryPopup.SetContentFactory(sortTrigger, () => new SortMenuView(filters));
        status.Add(sortArea, 1);
        results.Add(status, 0, 1);

        gallery = CreateGallery(columns);
        empty = GalleryUi.Text("No samples match these filters. Try a different query or clear filters.", 16);
        empty.HorizontalTextAlignment = TextAlignment.Center;
        empty.VerticalTextAlignment = TextAlignment.Center;
        empty.AutomationId = "gallery-empty";
        listArea = new Grid();
        listArea.Children.Add(gallery);
        listArea.Children.Add(empty);
        results.Add(listArea, 0, 2);
        main.Add(results);
        activeStrip = new ScrollView
        {
            Content = activeChips, Orientation = ScrollOrientation.Horizontal,
            HeightRequest = 44, Padding = new Thickness(12, 2),
            AutomationId = "gallery-active-filters",
            IsVisible = false
        };
        GalleryUi.Background(activeStrip, "PageBackground");
        root = new Grid
        {
            SafeAreaEdges = new Microsoft.Maui.SafeAreaEdges(Microsoft.Maui.SafeAreaRegions.Container),
            RowDefinitions = new RowDefinitionCollection
            {
                new(GridLength.Auto), new(GridLength.Auto), new(GridLength.Auto), new(GridLength.Star)
            }
        };
        root.Add(hero);
        root.Add(info, 0, 1);
        root.Add(activeStrip, 0, 2);
        root.Add(main, 0, 3);
        Content = root;
        SizeChanged += (_, _) => AdaptLayout();
        filters.Changed += (_, _) => RefreshResults();
        RefreshResults();
    }

    protected override bool OnBackButtonPressed() =>
        GalleryPopup.Dismiss(root) || base.OnBackButtonPressed();

    protected override void OnDisappearing()
    {
        GalleryPopup.Dismiss(root);
        base.OnDisappearing();
    }

    private void AdaptLayout()
    {
        if (Width <= 0) return;
        var nextWide = Width >= 920;
        if (wide != nextWide)
        {
            GalleryPopup.Dismiss(root);
            wide = nextWide;
            main.ColumnDefinitions.Clear();
            if (wide)
            {
                main.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(270)));
                main.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                main.SetColumn(main.Children[0], 0);
                main.SetColumn(main.Children[1], 1);
            }
            else
            {
                main.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                main.SetColumn(main.Children[0], 0);
                main.SetColumn(main.Children[1], 0);
            }
            rail.IsVisible = wide;
            compactSearch.IsVisible = !wide;
        }
        var span = wide ? Width < 1250 ? 2 : 3 : Width < 670 ? 1 : 2;
        if (span != columns) ReplaceGallery(span);
    }

    private CollectionView CreateGallery(int span)
    {
        var view = new CollectionView
        {
            AutomationId = "gallery-cards",
            ItemsSource = filters.Results,
            ItemsLayout = new GridItemsLayout(span, ItemsLayoutOrientation.Vertical)
            {
                VerticalItemSpacing = 12, HorizontalItemSpacing = 12
            },
            SelectionMode = SelectionMode.Single,
            ItemTemplate = new DataTemplate(CreateCard),
            Margin = new Thickness(12, 3, 12, 0),
            RemainingItemsThreshold = 4
        };
        view.SelectionChanged += OnGallerySelectionChanged;
        return view;
    }

    private async void OnGallerySelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!ReferenceEquals(sender, gallery)) return;
        if (e.CurrentSelection.FirstOrDefault() is SampleBase selected)
            await OpenSampleAsync(selected);
        if (ReferenceEquals(sender, gallery)) gallery.SelectedItem = null;
    }

    private void ReplaceGallery(int span)
    {
        columns = span;
        var previous = gallery;
        previous.SelectionChanged -= OnGallerySelectionChanged;
        listArea.Children.Remove(previous);
        previous.Handler?.DisconnectHandler();
        gallery = CreateGallery(span);
        listArea.Children.Insert(0, gallery);
    }

    private async Task OpenSampleAsync(SampleBase selected)
    {
        if (navigating || !selected.IsSupported) return;
        navigating = true;
        await navigationGate.WaitAsync();
        try
        {
            if (Navigation.NavigationStack.LastOrDefault() != this) return;
            var instance = service.CreateSample(selected.Title)
                ?? throw new InvalidOperationException($"Sample '{selected.Title}' is not available.");
            await Navigation.PushAsync(new SampleDetailPage(instance, ReturnToGalleryAsync, windowSettings));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
            await DisplayAlertAsync("Navigation failed", ex.Message, "OK");
        }
        finally
        {
            navigationGate.Release();
            navigating = false;
            gallery.SelectedItem = null;
        }
    }

    private async Task ReturnToGalleryAsync()
    {
        await navigationGate.WaitAsync();
        try
        {
            await Navigation.PopAsync();
        }
        finally
        {
            navigationGate.Release();
        }
    }

    private View CreateCard()
    {
        var cell = new ContentView();
        SampleBase? boundSample = null;
        cell.BindingContextChanged += (_, _) =>
        {
            if (cell.BindingContext is not SampleBase s)
            {
                cell.Content = null;
                boundSample = null;
                return;
            }
            if (ReferenceEquals(boundSample, s) && cell.Content is not null) return;
            boundSample = s;

            // CollectionView recycles cells, but MAUI AutomationIds are immutable.
            var title = GalleryUi.Text(s.Title, 19, true);
            var summary = GalleryUi.Text(s.Description, 12, color: "SecondaryText");
            summary.MaxLines = 2;
            var category = SampleManager.GetCategoryFor(s.Category);
            var categoryIcon = GalleryUi.Text(category.UnicodeIcon, 17);
            categoryIcon.FontFamily = "BootstrapIcons";
            GalleryUi.CategoryColor(categoryIcon, category.Name);
            var metadata = GalleryUi.Text(s.Category, 11, true);
            GalleryUi.CategoryColor(metadata, category.Name);
            var categoryRow = new HorizontalStackLayout { Spacing = 6 };
            categoryRow.Children.Add(categoryIcon);
            categoryRow.Children.Add(metadata);
            var badge = GalleryUi.Text("", 12, true);
            badge.Text = !s.IsSupported ? "UNSUPPORTED ON THIS PLATFORM" :
                string.Join("  ·  ", new[] { SampleManager.IsNew(s, all) ? "NEW" : null,
                    s is DocumentSampleBase ? "DOCUMENT" : null,
                    s is CanvasSampleBase canvas && canvas.IsAnimated ? "ANIMATED" : null }
                    .Where(x => x is not null)) is { Length: > 0 } b ? b : "VIEW SAMPLE →";
            badge.Style = (Style)Application.Current!.Resources[s.IsSupported ? "AccentLabel" : "SecondaryLabel"];
            var content = new VerticalStackLayout { Spacing = 7, Padding = 15, MinimumHeightRequest = 155 };
            content.Children.Add(categoryRow);
            content.Children.Add(title);
            content.Children.Add(summary);
            content.Children.Add(badge);
            var card = GalleryUi.Card(content);
            card.Margin = new Thickness(1, 1, 1, 4);
            card.AutomationId = GalleryUi.StableId("sample-", s.Title);
            SemanticProperties.SetDescription(card, s.IsSupported
                ? $"{s.Title}. {s.Category}. {s.Description}"
                : $"{s.Title}. Not supported on this platform.");
            card.Opacity = s.IsSupported ? 1 : 0.65;
            card.IsEnabled = s.IsSupported;
            var tap = new TapGestureRecognizer();
            tap.Tapped += async (_, _) => await OpenSampleAsync(s);
            card.GestureRecognizers.Add(tap);
            cell.Content = card;
        };
        return cell;
    }

    private void RefreshResults()
    {
        if (search.Text != filters.SearchText) search.Text = filters.SearchText;
        gallery.ItemsSource = filters.Results;
        count.Text = $"{filters.Results.Length} of {all.Length} samples";
        empty.IsVisible = filters.Results.Length == 0;
        gallery.IsVisible = filters.Results.Length > 0;
        sortTrigger.Text = filters.SortOrder switch
        {
            SampleSortOrder.OldestFirst => "Oldest first",
            SampleSortOrder.Alphabetical => "A to Z",
            SampleSortOrder.Category => "Category",
            _ => "Featured"
        };
        SemanticProperties.SetDescription(sortTrigger, $"Sort samples, current order {sortTrigger.Text}");
        filterBadge.Text = filters.ActiveCount > 0 ? filters.ActiveCount.ToString() : "";
        filterBadge.IsVisible = filters.ActiveCount > 0;
        activeChips.Children.Clear();
        if (filters.SearchText.Length > 0)
            activeChips.Children.Add(ActiveChip($"Search: {filters.SearchText}", "gallery-active-search", () => filters.SetSearch("")));
        if (filters.SelectedCategory is { } category)
        {
            activeChips.Children.Add(ActiveChip(category,
                GalleryUi.StableId("gallery-active-category-", category), () => filters.SelectCategory(null)));
        }
        foreach (var tag in filters.Tags.Order(StringComparer.Ordinal))
        {
            var item = tag;
            activeChips.Children.Add(ActiveChip(KnownApis.GetDisplayName(item),
                GalleryUi.StableId("gallery-active-tag-", item), () => filters.ToggleTag(item)));
        }
        if (filters.ActiveCount > 0)
        {
            var clear = GalleryUi.Pill("Clear all", "gallery-clear", (_, _) => filters.Clear());
            activeChips.Children.Add(clear);
        }
        activeStrip.IsVisible = filters.ActiveCount > 0;
    }

    private static Button ActiveChip(string label, string id, Action remove)
    {
        var button = GalleryUi.Pill($"{label}  ×", id, (_, _) => remove());
        SemanticProperties.SetDescription(button, $"Remove filter {label}");
        return button;
    }
}
