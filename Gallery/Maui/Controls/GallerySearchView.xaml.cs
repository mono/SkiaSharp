using Microsoft.Maui.Controls;

namespace SkiaSharpSample.Controls;

internal sealed class GallerySearchEntry : Entry { }

public partial class GallerySearchView : Border
{
    public GallerySearchView()
    {
        InitializeComponent();
        BindingContext = this;
    }

    public GallerySearchView(string id, string placeholder) : this()
    {
        SearchAutomationId = id;
        SearchPlaceholder = placeholder;
    }

    public static readonly BindableProperty SearchAutomationIdProperty =
        BindableProperty.Create(nameof(SearchAutomationId), typeof(string), typeof(GallerySearchView), "");

    public static readonly BindableProperty SearchPlaceholderProperty =
        BindableProperty.Create(nameof(SearchPlaceholder), typeof(string), typeof(GallerySearchView), "");

    public string SearchAutomationId
    {
        get => (string)GetValue(SearchAutomationIdProperty);
        set => SetValue(SearchAutomationIdProperty, value);
    }

    public string SearchPlaceholder
    {
        get => (string)GetValue(SearchPlaceholderProperty);
        set => SetValue(SearchPlaceholderProperty, value);
    }

    internal GallerySearchEntry Input => SearchInput;
}
