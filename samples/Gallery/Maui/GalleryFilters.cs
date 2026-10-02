namespace SkiaSharpSample;

internal sealed class GalleryFilters
{
    public GalleryFilters(SampleBase[] samples)
    {
        AllSamples = samples;
        AllTags = SampleManager.GetAllTags(samples);
        Refresh();
    }

    public event EventHandler? Changed;

    public SampleBase[] AllSamples { get; }
    public IReadOnlyList<(string Tag, int Count, TagKind Kind)> AllTags { get; }
    public string? SelectedCategory { get; private set; }
    public HashSet<string> Tags { get; } = new(StringComparer.Ordinal);
    public string SearchText { get; private set; } = "";
    public SampleSortOrder SortOrder { get; private set; }
    public SampleBase[] Results { get; private set; } = [];
    public Dictionary<string, int> LiveCategoryCounts { get; private set; } = [];
    public Dictionary<string, int> LiveTagCounts { get; private set; } = [];
    public int ActiveCount => (SelectedCategory is null ? 0 : 1) + Tags.Count + (SearchText.Length > 0 ? 1 : 0);

    public void SetSearch(string? value)
    {
        var next = value ?? "";
        if (SearchText == next) return;
        SearchText = next;
        Refresh();
    }

    public void SelectCategory(string? name)
    {
        if (SelectedCategory == name) return;
        SelectedCategory = name;
        Refresh();
    }

    public void ToggleTag(string tag)
    {
        if (!Tags.Add(tag)) Tags.Remove(tag);
        Refresh();
    }

    public void SetSort(SampleSortOrder order)
    {
        if (SortOrder == order) return;
        SortOrder = order;
        Refresh();
    }

    public void Clear()
    {
        SearchText = "";
        SelectedCategory = null;
        Tags.Clear();
        SortOrder = SampleSortOrder.NewestFirst;
        Refresh();
    }

    private void Refresh()
    {
        ISet<string>? categories = SelectedCategory is { } name ? new HashSet<string>(StringComparer.Ordinal) { name } : null;
        Results = SampleManager.SearchSamples(AllSamples, SearchText, categories, Tags, tagModeAll: true, sort: SortOrder).ToArray();
        LiveCategoryCounts = SampleManager.GetCategoryCounts(Results);
        LiveTagCounts = SampleManager.GetLiveTagCounts(Results);
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
