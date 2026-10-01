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
    public HashSet<string> Categories { get; } = new(StringComparer.Ordinal);
    public HashSet<string> Tags { get; } = new(StringComparer.Ordinal);
    public string SearchText { get; private set; } = "";
    public bool MatchAll { get; private set; }
    public SampleSortOrder SortOrder { get; private set; }
    public SampleBase[] Results { get; private set; } = [];
    public Dictionary<string, int> LiveTagCounts { get; private set; } = [];
    public int ActiveCount => Categories.Count + Tags.Count + (SearchText.Length > 0 ? 1 : 0);

    public void SetSearch(string? value)
    {
        var next = value ?? "";
        if (SearchText == next) return;
        SearchText = next;
        Refresh();
    }

    public void ToggleCategory(string name)
    {
        if (!Categories.Add(name)) Categories.Remove(name);
        Refresh();
    }

    public void ClearCategories()
    {
        if (Categories.Count == 0) return;
        Categories.Clear();
        Refresh();
    }

    public void ToggleTag(string tag)
    {
        if (!Tags.Add(tag)) Tags.Remove(tag);
        Refresh();
    }

    public void SetTagMode(bool all)
    {
        if (MatchAll == all) return;
        MatchAll = all;
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
        Categories.Clear();
        Tags.Clear();
        MatchAll = false;
        SortOrder = SampleSortOrder.NewestFirst;
        Refresh();
    }

    private void Refresh()
    {
        Results = SampleManager.SearchSamples(AllSamples, SearchText, Categories, Tags, MatchAll, SortOrder).ToArray();
        LiveTagCounts = SampleManager.GetLiveTagCounts(Results);
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
