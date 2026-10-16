using Microsoft.Maui.Controls;
using SkiaSharpSample.Controls;
using SkiaSharpSample.Services;

namespace SkiaSharpSample;

public partial class GalleryPage : ContentPage
{
    private readonly SampleService service;
    private readonly GalleryWindowSettings windowSettings;
    private readonly SampleBase[] all;
    private readonly GalleryFilters filters;
    private GalleryCardsView gallery;
    private IReadOnlyList<ActiveFacet> activeChips = [];
    private int columns = 1;
    private bool wide;
    private bool navigating;
    private readonly SemaphoreSlim navigationGate = new(1, 1);

    public GalleryPage(SampleService service) : this(service, new GalleryWindowSettings(service)) { }

    internal GalleryPage(SampleService service, GalleryWindowSettings windowSettings)
    {
        this.service = service;
        this.windowSettings = windowSettings;
        all = service.GetAllSamples().ToArray();
        filters = new GalleryFilters(all);
        InitializeComponent();
        BindingContext = this;
        Title = "Gallery";
        NavigationPage.SetHasNavigationBar(this, false);
        SafeAreaEdges = new Microsoft.Maui.SafeAreaEdges(Microsoft.Maui.SafeAreaRegions.None);

        headerHost.Content = new GalleryHeader(windowSettings);
        railHost.Content = new FilterRailView(filters, popup: false) { WidthRequest = -1 };
        searchField.Input.TextChanged += (_, e) => filters.SetSearch(e.NewTextValue);
        GalleryPopup.SetCloseOnAction(filterTrigger, false);
        GalleryPopup.SetContentFactory(filterTrigger, () =>
            new FilterRailView(filters, popup: true, () => GalleryPopup.Dismiss(root)));
        GalleryPopup.SetCloseOnAction(sortTrigger, true);
        GalleryPopup.SetContentFactory(sortTrigger, () => new SortMenuView(filters));

        gallery = CreateGallery(columns);
        listArea.Children.Insert(0, gallery);
        SizeChanged += (_, _) => AdaptLayout();
        filters.Changed += (_, _) => RefreshResults();
        RefreshResults();
    }

    public string UniqueApiCountText => $"{filters.AllTags.Count} APIs covered";
    public string ResultCountText => $"{filters.Results.Length} of {all.Length} samples";
    public bool IsEmpty => filters.Results.Length == 0;
    public bool HasActiveFilters => filters.ActiveCount > 0;
    public string FilterBadgeText => HasActiveFilters ? filters.ActiveCount.ToString() : "";
    public string SortLabel => filters.SortOrder switch
    {
        SampleSortOrder.OldestFirst => "Oldest first",
        SampleSortOrder.Alphabetical => "A to Z",
        SampleSortOrder.Category => "Category",
        _ => "Featured"
    };
    public string SortDescription => $"Sort samples, current order {SortLabel}";
    public IReadOnlyList<ActiveFacet> ActiveChips => activeChips;

    private void ActiveChipClicked(object? sender, EventArgs e)
    {
        if (sender is Button { BindingContext: ActiveFacet item })
            item.Remove();
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
            railHost.IsVisible = wide;
            compactSearch.IsVisible = !wide;
        }
        var span = wide ? Width < 1250 ? 2 : 3 : Width < 670 ? 1 : 2;
        if (span != columns) ReplaceGallery(span);
    }

    private GalleryCardsView CreateGallery(int span)
    {
        var view = new GalleryCardsView(span, all, OpenSampleAsync)
        {
            ItemsSource = filters.Results
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
        try { await Navigation.PopAsync(); }
        finally { navigationGate.Release(); }
    }

    private void RefreshResults()
    {
        if (searchField.Input.Text != filters.SearchText)
            searchField.Input.Text = filters.SearchText;
        gallery.ItemsSource = filters.Results;
        OnPropertyChanged(nameof(ResultCountText));
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(SortLabel));
        OnPropertyChanged(nameof(SortDescription));
        OnPropertyChanged(nameof(FilterBadgeText));
        OnPropertyChanged(nameof(HasActiveFilters));

        var chips = new List<ActiveFacet>();
        if (filters.SearchText.Length > 0)
            chips.Add(new ActiveFacet($"Search: {filters.SearchText}", "gallery-active-search",
                false, () => filters.SetSearch("")));
        if (filters.SelectedCategory is { } category)
            chips.Add(new ActiveFacet(category,
                GalleryUi.StableId("gallery-active-category-", category), false,
                () => filters.SelectCategory(null)));
        foreach (var tag in SampleManager.OrderTags(filters.Tags))
        {
            var item = tag;
            chips.Add(new ActiveFacet(KnownApis.GetDisplayName(item),
                GalleryUi.StableId("gallery-active-tag-", item), true,
                () => filters.ToggleTag(item)));
        }
        if (chips.Count > 0)
            chips.Add(new ActiveFacet("Clear all", "gallery-clear", false, filters.Clear, IsReset: true));
        activeChips = chips;
        OnPropertyChanged(nameof(ActiveChips));
    }
}
