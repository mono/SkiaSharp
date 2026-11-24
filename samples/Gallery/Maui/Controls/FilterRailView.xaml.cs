using Microsoft.Maui.Controls;

namespace SkiaSharpSample.Controls;

public partial class FilterRailView : ContentView, IDisposable
{
    private readonly GalleryFilters filters;
    private readonly Action? close;
    private bool typesOpen = true;
    private bool methodsOpen;
    private bool disposed;
    private IReadOnlyList<CategoryFacet> categories = [];
    private IReadOnlyList<TagFacet> types = [];
    private IReadOnlyList<TagFacet> methods = [];

    internal FilterRailView(GalleryFilters filters, bool popup, Action? close = null)
    {
        this.filters = filters;
        this.close = close;
        IsPopup = popup;
        InitializeComponent();
        BindingContext = this;
        AutomationId = popup ? "gallery-filter-popup" : "gallery-filter-sidebar";
        WidthRequest = popup ? 332 : 266;
        HeightRequest = popup ? 520 : -1;
        SearchField.SearchAutomationId = popup ? "gallery-filter-popup-search" : "gallery-filter-search";
        SearchField.Input.Text = filters.SearchText;
        SearchField.Input.TextChanged += SearchChanged;
        filters.Changed += FiltersChanged;
        RefreshView();
    }

    public bool IsPopup { get; }
    public string CountAutomationId => IsPopup ? "gallery-filter-popup-count" : "gallery-filter-sidebar-count";
    public string ResultCountText => $"Filters  ·  {filters.Results.Length} results";
    public bool TypesOpen => typesOpen;
    public bool MethodsOpen => methodsOpen;
    public string TypesDescription => $"API types, {(typesOpen ? "expanded" : "collapsed")}";
    public string MethodsDescription => $"API methods, {(methodsOpen ? "expanded" : "collapsed")}";
    public IReadOnlyList<CategoryFacet> Categories => categories;
    public IReadOnlyList<TagFacet> Types => types;
    public IReadOnlyList<TagFacet> Methods => methods;

    private void SearchChanged(object? sender, TextChangedEventArgs e) =>
        filters.SetSearch(e.NewTextValue);

    private void FiltersChanged(object? sender, EventArgs e) => RefreshView();

    private void RefreshView()
    {
        if (disposed) return;
        if (SearchField.Input.Text != filters.SearchText)
            SearchField.Input.Text = filters.SearchText;
        OnPropertyChanged(nameof(ResultCountText));

        var choices = new List<CategoryFacet>
        {
            new("All categories", "\uf3fa", filters.Results.Length,
                filters.SelectedCategory is null, "category-all", () => filters.SelectCategory(null))
        };
        foreach (var category in SampleManager.GetCategories())
        {
            var count = filters.LiveCategoryCounts.GetValueOrDefault(category.Name);
            if (count == 0) continue;
            choices.Add(new CategoryFacet(category.Name, category.UnicodeIcon, count,
                filters.SelectedCategory == category.Name,
                GalleryUi.StableId("category-", category.Name),
                () => filters.SelectCategory(category.Name)));
        }
        categories = choices;
        OnPropertyChanged(nameof(Categories));
        types = CreateTags(TagKind.Type);
        methods = CreateTags(TagKind.Method);
        OnPropertyChanged(nameof(Types));
        OnPropertyChanged(nameof(Methods));
    }

    private IReadOnlyList<TagFacet> CreateTags(TagKind kind) =>
        filters.AllTags
            .Where(t => t.Kind == kind && filters.LiveTagCounts.GetValueOrDefault(t.Tag) > 0)
            .Select(t => new TagFacet(t.Tag, filters.LiveTagCounts[t.Tag], t.Count,
                kind, filters.Tags.Contains(t.Tag), () => filters.ToggleTag(t.Tag)))
            .ToArray();

    private void TypesClicked(object? sender, EventArgs e)
    {
        typesOpen = !typesOpen;
        OnPropertyChanged(nameof(TypesOpen));
        OnPropertyChanged(nameof(TypesDescription));
    }

    private void MethodsClicked(object? sender, EventArgs e)
    {
        methodsOpen = !methodsOpen;
        OnPropertyChanged(nameof(MethodsOpen));
        OnPropertyChanged(nameof(MethodsDescription));
    }

    private void ResetClicked(object? sender, EventArgs e) => filters.Clear();
    private void CloseClicked(object? sender, EventArgs e) => close?.Invoke();

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        filters.Changed -= FiltersChanged;
        SearchField.Input.TextChanged -= SearchChanged;
    }
}
