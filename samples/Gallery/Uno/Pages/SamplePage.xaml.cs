using System.Collections.Generic;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using SkiaSharpSample.Controls;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace SkiaSharpSample.Pages;

public sealed partial class SamplePage : Page
{
    private SampleBase? currentSample;
    private CanvasSampleBase? currentCanvasSample;
    private SampleCanvasView canvasHost = null!;

    public SamplePage()
    {
        this.InitializeComponent();
        ControlPanel.ControlChanged += OnControlChanged;
        canvasHost = SampleCanvasView.Create(OnPaint);
        CanvasBorder.Child = canvasHost;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        var title = e.Parameter as string;
        var sample = title is null ? null : App.SampleService.GetSample(title);
        if (sample is null)
        {
            TitleText.Text = "Sample not found";
            return;
        }

        TitleText.Text = sample.Title;
        DescriptionText.Text = sample.Description ?? string.Empty;

        ControlPanel.SetControls(sample.Controls);

        currentSample = sample;
        if (sample is CanvasSampleBase canvasSample)
        {
            currentCanvasSample = canvasSample;
            canvasSample.RefreshRequested += OnSampleRefreshRequested;
        }

        await sample.InitAsync();

        if (sample is DocumentSampleBase docSample)
        {
            GenerateDocument(docSample);
        }

        // Set download/sidebar visibility after init and document generation
        // so that HasDownload reflects populated DocumentBytes.
        var hasDownload = sample.HasDownload;
        DownloadButton.Visibility = hasDownload
            ? Visibility.Visible
            : Visibility.Collapsed;
        ControlsSidebar.Visibility = sample.Controls.Count > 0 || hasDownload
            ? Visibility.Visible
            : Visibility.Collapsed;

        canvasHost.InvalidateRender();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        if (currentCanvasSample is not null)
        {
            currentCanvasSample.RefreshRequested -= OnSampleRefreshRequested;
            currentCanvasSample = null;
        }
        currentSample?.Destroy();
        currentSample = null;
    }

    private void OnSampleRefreshRequested(object? sender, System.EventArgs e)
    {
        if (this.DispatcherQueue is { } dq)
            dq.TryEnqueue(() => canvasHost.InvalidateRender());
        else
            canvasHost.InvalidateRender();
    }

    private void OnPaint(SKCanvas canvas, SKImageInfo info, SKImageInfo rawInfo)
    {
        switch (currentSample)
        {
            case CanvasSampleBase cs:
                cs.DrawSample(canvas, info.Width, info.Height);
                break;
            case DocumentSampleBase ds:
                ds.DrawSample(canvas, info.Width, info.Height);
                break;
        }

        var dpi = info.Width > 0
            ? $"{(float)rawInfo.Width / info.Width:F2}x"
            : "?";
        CanvasInfoText.Text = $"Canvas: {info.Width}×{info.Height} CSS px  |  {rawInfo.Width}×{rawInfo.Height} device px  |  DPI: {dpi}";
    }

    private static void GenerateDocument(DocumentSampleBase doc)
    {
        // DocumentBytes is populated as a side effect of DrawSample.
        using var surface = SKSurface.Create(new SKImageInfo(1, 1));
        doc.DrawSample(surface.Canvas, 1, 1);
    }

    private void OnControlChanged(object? sender, (string Id, object Value) args)
    {
        if (currentSample is null) return;
        currentSample.UpdateControl(args.Id, args.Value);
        if (currentSample is DocumentSampleBase doc)
            GenerateDocument(doc);
        canvasHost.InvalidateRender();
    }

    private void OnBackClicked(object sender, RoutedEventArgs e)
    {
        MainPage.Current?.NavigateHome();
    }

    private bool controlsCollapsed;
    private void OnCollapseClicked(object sender, RoutedEventArgs e)
    {
        controlsCollapsed = !controlsCollapsed;
        if (ControlsSidebar.FindName("ControlPanel") is FrameworkElement)
        {
            ControlsSidebar.Width = controlsCollapsed ? 48 : 290;
            CollapseIcon.Text = controlsCollapsed ? "▶" : "◀";
            if (ControlsSidebar.Child is Grid g && g.RowDefinitions.Count > 1)
            {
                foreach (var child in g.Children)
                {
                    if (child is Border body && Grid.GetRow(body) == 1)
                        body.Visibility = controlsCollapsed ? Visibility.Collapsed : Visibility.Visible;
                }
            }
        }
    }

    private async void OnDownloadClicked(object sender, RoutedEventArgs e)
    {
        if (currentSample is null || !currentSample.HasDownload)
            return;

        var bytes = currentSample.DownloadBytes;
        if (bytes is not { Length: > 0 })
        {
            if (currentSample is DocumentSampleBase doc)
                GenerateDocument(doc);
            bytes = currentSample.DownloadBytes;
            if (bytes is not { Length: > 0 }) return;
        }

        var fileName = currentSample.DownloadFileName;
        var extension = System.IO.Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(extension))
            extension = ".bin";

        var picker = new FileSavePicker
        {
            SuggestedFileName = fileName,
        };
        picker.FileTypeChoices.Add("File", new List<string> { extension });

        var file = await picker.PickSaveFileAsync();
        if (file is null) return;
        await FileIO.WriteBytesAsync(file, bytes);
    }
}
