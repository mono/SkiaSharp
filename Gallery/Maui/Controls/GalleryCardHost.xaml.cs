using Microsoft.Maui.Controls;

namespace SkiaSharpSample.Controls;

public partial class GalleryCardHost : ContentView
{
    private readonly SampleBase[] corpus;
    private readonly Func<SampleBase, Task> open;
    private SampleBase? boundSample;

    internal GalleryCardHost(SampleBase[] corpus, Func<SampleBase, Task> open)
    {
        this.corpus = corpus;
        this.open = open;
        InitializeComponent();
        BindingContextChanged += (_, _) =>
        {
            if (BindingContext is not SampleBase sample)
            {
                Content = null;
                boundSample = null;
                return;
            }
            if (ReferenceEquals(boundSample, sample) && Content is not null) return;
            boundSample = sample;
            Content = new GallerySampleCard { BindingContext = new GalleryCardItem(sample, corpus, open) };
        };
    }
}
