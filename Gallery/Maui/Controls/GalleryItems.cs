using Microsoft.Maui.Graphics;

namespace SkiaSharpSample.Controls;

public sealed record CategoryFacet(string Name, string Icon, int Count, bool Selected, string Id, Action Select)
{
    public string CountText => Count > 0 ? Count.ToString() : "";
    public string CountId => $"{Id}-count";
    public string RowId => GalleryUi.StableId("category-row-", Name);
    public string Description => $"{Name}, {(Count > 0 ? $"{Count} matching samples" : "no matching samples")}; {(Selected ? "selected" : "not selected")}";
    public Color IconColor => Name == "All categories"
        ? GalleryUi.Accent
        : Color.FromArgb(SampleManager.GetCategoryFor(Name).Color);
}

public sealed record TagFacet(string Tag, int Count, int Total, TagKind Kind, bool Selected, Action Toggle)
{
    public string Display => $"{KnownApis.GetDisplayName(Tag)}  {Count}";
    public string Id => GalleryUi.StableId("tag-", Tag);
    public string Description => $"{Tag}; {Count} current results, {Total} total; {(Selected ? "selected" : "not selected")}";
    public bool IsType => Kind == TagKind.Type;
}

public sealed record ActiveFacet(string Display, string Id, bool IsTag, Action Remove, bool IsReset = false)
{
    public string Text => IsReset ? Display : $"{Display}  ×";
    public string Description => IsReset ? "Clear all filters" : $"Remove filter {Display}";
}

public sealed record MenuOptionItem(string Label, string Id, bool Selected, Action Select)
{
    public string Display => $"{(Selected ? "✓  " : "     ")}{Label}";
    public string Description => $"{Label}, {(Selected ? "selected" : "not selected")}";
}

public sealed record SampleTagItem(string Tag)
{
    public string Display => KnownApis.GetDisplayName(Tag);
    public string Id => GalleryUi.StableId("sample-tag-", Tag);
    public bool IsType => KnownApis.Classify(Tag) == TagKind.Type;
}

public sealed class GalleryCardItem(SampleBase sample, SampleBase[] corpus, Func<SampleBase, Task> open)
{
    public SampleBase Sample { get; } = sample;
    public string Id => GalleryUi.StableId("sample-", Sample.Title);
    public string Title => Sample.Title;
    public string Summary => Sample.Description;
    public string Category => Sample.Category;
    public string CategoryIcon => SampleManager.GetCategoryFor(Category).UnicodeIcon;
    public Color CategoryColor => Color.FromArgb(SampleManager.GetCategoryFor(Category).Color);
    public string Description => Sample.IsSupported
        ? $"{Title}. {Category}. {Summary}" : $"{Title}. Not supported on this platform.";
    public bool Supported => Sample.IsSupported;
    public double Opacity => Sample.IsSupported ? 1 : 0.65;
    public string Badge
    {
        get
        {
            if (!Sample.IsSupported) return "UNSUPPORTED ON THIS PLATFORM";
            var badges = new[]
            {
                SampleManager.IsNew(Sample, corpus) ? "NEW" : null,
                Sample is DocumentSampleBase ? "DOCUMENT" : null,
                Sample is CanvasSampleBase canvas && canvas.IsAnimated ? "ANIMATED" : null
            };
            var joined = string.Join("  ·  ", badges.Where(b => b is not null));
            return joined.Length > 0 ? joined : "VIEW SAMPLE →";
        }
    }

    public Task OpenAsync() => open(Sample);
}
