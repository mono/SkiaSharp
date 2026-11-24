using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample;

public sealed partial class SampleDetailPage : ContentPage
{
    private readonly SampleBase sample;
    private readonly Func<Task> navigateBack;
    private readonly GalleryWindowSettings windowSettings;
    private bool navigatingBack;
    private readonly object sampleGate = new();
    private readonly Dictionary<string, ControlItem> controlUpdaters = new(StringComparer.Ordinal);
    private IReadOnlyList<ControlItem> controlItemsData = [];
    private SKCanvasView? cpuView;
    private SKGLView? gpuView;
    private Task? initTask;
    private Window? subscribedWindow;
    private bool active;
    private bool useGpu;
    private bool controlsExpanded = true;
    private bool userToggledControls;
    private bool hasControlPanel;
    private bool toggleOverCanvas;
    private bool showOutputInfo;
    private string outputText = "";
    private int generation;
    private int lastWidth = -1;
    private int lastHeight = -1;

    public SampleDetailPage(SampleBase sample, Func<Task> navigateBack)
        : this(sample, navigateBack, new GalleryWindowSettings())
    {
    }

    internal SampleDetailPage(SampleBase sample, Func<Task> navigateBack, GalleryWindowSettings windowSettings)
    {
        this.sample = sample;
        this.navigateBack = navigateBack;
        this.windowSettings = windowSettings;
        useGpu = windowSettings.UseGpu;
        SampleTags = SampleManager.OrderTags(sample.ApiTags).Select(tag => new SampleTagItem(tag)).ToArray();
        InitializeComponent();
        BindingContext = this;
        Title = sample.Title;
        NavigationPage.SetHasNavigationBar(this, false);
        SafeAreaEdges = new Microsoft.Maui.SafeAreaEdges(Microsoft.Maui.SafeAreaRegions.None);
        headerHost.Content = new GalleryHeader(windowSettings, GoBackAsync);
        SizeChanged += (_, _) => UpdateLayout();
        UpdateButtons();
        UpdateLayout();
    }

    public string SampleTitle => sample.Title;
    public string SampleDescription => sample.Description;
    public IReadOnlyList<SampleTagItem> SampleTags { get; }
    public IReadOnlyList<ControlItem> ControlItems => controlItemsData;
    public bool NoControls => controlItemsData.Count == 0;
    public bool ShowOutputInfo => showOutputInfo;
    public string OutputText => outputText;

    private async void OpenClicked(object? sender, EventArgs e) => await ExportAsync(openFile: true);
    private async void ShareClicked(object? sender, EventArgs e) => await ExportAsync(openFile: false);
    private void RetryClicked(object? sender, EventArgs e)
    {
        Stop();
        _ = StartAsync();
    }
    private void ToggleControlsClicked(object? sender, EventArgs e)
    {
        userToggledControls = true;
        controlsExpanded = !controlsExpanded;
        UpdateLayout();
    }

    protected override bool OnBackButtonPressed()
    {
        if (GalleryPopup.Dismiss(root)) return true;
        _ = GoBackAsync();
        return true;
    }

    private async Task GoBackAsync()
    {
        if (navigatingBack) return;
        navigatingBack = true;
        try
        {
            await navigateBack();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Gallery navigation failed: {ex}");
            await DisplayAlertAsync("Navigation failed", ex.Message, "OK");
        }
        finally
        {
            navigatingBack = false;
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        useGpu = windowSettings.UseGpu;
        windowSettings.BackendChanged += OnBackendChanged;
        if (Window is { } window && subscribedWindow != window)
        {
            subscribedWindow = window;
            window.Stopped += WindowStopped;
            window.Resumed += WindowResumed;
            window.Destroying += WindowDestroying;
        }
        _ = StartAsync();
    }

    protected override void OnDisappearing()
    {
        GalleryPopup.Dismiss(root);
        windowSettings.BackendChanged -= OnBackendChanged;
        Stop();
        if (subscribedWindow is { } window)
        {
            window.Stopped -= WindowStopped;
            window.Resumed -= WindowResumed;
            window.Destroying -= WindowDestroying;
            subscribedWindow = null;
        }
        base.OnDisappearing();
    }

    private void WindowStopped(object? sender, EventArgs e) => Stop();
    private void WindowDestroying(object? sender, EventArgs e) => Stop();
    private void WindowResumed(object? sender, EventArgs e) => _ = StartAsync();

    private async Task StartAsync()
    {
        if (active) return;
        active = true;
        var epoch = ++generation;
        state.Text = "Loading sample…";
        state.IsVisible = true;
        status.Text = "Initializing…";
        retry.IsVisible = false;
        UpdateButtons();
        controlPanel.IsEnabled = false;
        if (sample is CanvasSampleBase canvas)
        {
            canvas.RefreshRequested += OnRefreshRequested;
            canvas.AnimationFailed += OnAnimationFailed;
        }
        try
        {
            // A stopped page may still be completing initialization; never initialize
            // the same sample twice concurrently.
            if (initTask is { IsCompleted: false }) await initTask;
            if (!active || epoch != generation) return;
            initTask = sample.InitAsync();
            await initTask;
            if (!active || epoch != generation)
            {
                if (!active)
                    lock (sampleGate) sample.Destroy();
                return;
            }
            RefreshControls();
            controlPanel.IsEnabled = true;
            if (sample is DocumentSampleBase document) GenerateDocument(document);
            lastWidth = lastHeight = -1;
            if (sample is not DocumentSampleBase) status.Text = "Waiting for first frame…";
            InstallRenderer();
            UpdateButtons();
            state.IsVisible = sample is DocumentSampleBase;
            if (sample is DocumentSampleBase)
                UpdateDocumentState();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Gallery sample {sample.Title} initialization failed: {ex}");
            if (!active || epoch != generation) return;
            Stop();
            state.Text = $"Could not load {sample.Title}: {ex.Message}";
            state.IsVisible = true;
            status.Text = "Sample failed · retry or return to gallery";
            retry.IsVisible = true;
            UpdateButtons();
        }
    }

    private void Stop()
    {
        if (!active) return;
        active = false;
        ++generation;
        if (sample is CanvasSampleBase canvas)
        {
            canvas.RefreshRequested -= OnRefreshRequested;
            canvas.AnimationFailed -= OnAnimationFailed;
        }
        RemoveRenderer();
        controlPanel.IsEnabled = false;
        UpdateButtons();
        // InitAsync has no cancellation token. Defer destruction until completion,
        // and only if no newer page activation has taken ownership.
        if (initTask is { IsCompleted: false } pending)
        {
            var epoch = generation;
            _ = DestroyAfterInitAsync(pending, epoch);
        }
        else
        {
            lock (sampleGate) sample.Destroy();
        }
    }

    private async Task DestroyAfterInitAsync(Task pending, int epoch)
    {
        try { await pending; }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex); }
        if (generation == epoch && !active)
        {
            lock (sampleGate) sample.Destroy();
        }
    }

    private void InstallRenderer()
    {
        RemoveRenderer();
        if (!active || sample is DocumentSampleBase) return;
        if (useGpu)
        {
            gpuView = new SKGLView { IgnorePixelScaling = true, HasRenderLoop = false, AutomationId = "sample-canvas-gpu" };
            gpuView.PaintSurface += PaintGpu;
            gpuView.HandlerChanged += RendererReady;
            gpuView.SizeChanged += RendererReady;
            surfaceHost.Insert(0, gpuView);
        }
        else
        {
            cpuView = new SKCanvasView { IgnorePixelScaling = true, AutomationId = "sample-canvas-cpu" };
            cpuView.PaintSurface += PaintCpu;
            cpuView.HandlerChanged += RendererReady;
            cpuView.SizeChanged += RendererReady;
            surfaceHost.Insert(0, cpuView);
        }
    }

    private void RendererReady(object? sender, EventArgs e)
    {
        if (!active || sender is not VisualElement view || view.Width <= 0 || view.Height <= 0) return;
        if (sender is SKCanvasView canvas && ReferenceEquals(canvas, cpuView)) canvas.InvalidateSurface();
        if (sender is SKGLView gl && ReferenceEquals(gl, gpuView)) gl.InvalidateSurface();
    }

    private void RemoveRenderer()
    {
        if (gpuView is { } oldGpu)
        {
            gpuView = null;
            oldGpu.HasRenderLoop = false;
            oldGpu.PaintSurface -= PaintGpu;
            oldGpu.HandlerChanged -= RendererReady;
            oldGpu.SizeChanged -= RendererReady;
            surfaceHost.Children.Remove(oldGpu);
            oldGpu.Handler?.DisconnectHandler();
        }
        if (cpuView is { } oldCpu)
        {
            cpuView = null;
            oldCpu.PaintSurface -= PaintCpu;
            oldCpu.HandlerChanged -= RendererReady;
            oldCpu.SizeChanged -= RendererReady;
            surfaceHost.Children.Remove(oldCpu);
            oldCpu.Handler?.DisconnectHandler();
        }
    }

    private void ChangeBackend(bool gpuRequested)
    {
        if (useGpu == gpuRequested) return;
        useGpu = gpuRequested;
        lastWidth = lastHeight = -1;
        if (sample is DocumentSampleBase)
        {
            status.Text = sample.HasDownload
                ? "Document preview · renderer choice applies to canvas samples"
                : "Document generation unavailable";
            return;
        }
        status.Text = useGpu ? "GPU · preparing surface…" : "CPU · preparing surface…";
        InstallRenderer();
    }

    private void OnBackendChanged(object? sender, EventArgs e) => ChangeBackend(windowSettings.UseGpu);

    private void OnRefreshRequested(object? sender, EventArgs e)
    {
        var epoch = generation;
        Dispatcher.Dispatch(() =>
        {
            if (!active || generation != epoch) return;
            cpuView?.InvalidateSurface();
            gpuView?.InvalidateSurface();
        });
    }

    private void OnAnimationFailed(object? sender, Exception error)
    {
        var epoch = generation;
        Dispatcher.Dispatch(() =>
        {
            if (!active || generation != epoch) return;
            Stop();
            state.Text = $"Animation failed: {error.Message}";
            state.IsVisible = true;
            status.Text = "Animation failed · retry or return to gallery";
            retry.IsVisible = true;
            UpdateButtons();
        });
    }

    private void PaintCpu(object? sender, SKPaintSurfaceEventArgs e) => Paint(sender, e.Surface.Canvas, e.Info, e.RawInfo, false);
    private void PaintGpu(object? sender, SKPaintGLSurfaceEventArgs e) => Paint(sender, e.Surface.Canvas, e.Info, e.RawInfo, true);

    private void Paint(object? renderer, SKCanvas canvas, SKImageInfo info, SKImageInfo raw, bool gpuFrame)
    {
        if (!active || gpuFrame != useGpu || !ReferenceEquals(renderer, gpuFrame ? gpuView : cpuView)) return;
        try
        {
            lock (sampleGate)
            {
                canvas.Clear(SKColors.White);
                if (sample is CanvasSampleBase s) s.DrawSample(canvas, info.Width, info.Height);
            }
            if (info.Width == lastWidth && info.Height == lastHeight) return;
            lastWidth = info.Width;
            lastHeight = info.Height;
            var backend = !gpuFrame ? "CPU · raster" :
#if MACCATALYST
                "GPU · Metal";
#elif WINDOWS
                "GPU · ANGLE / DirectX";
#else
                "GPU · OpenGL ES";
#endif
            Dispatcher.Dispatch(() =>
            {
                if (active && useGpu == gpuFrame && ReferenceEquals(renderer, gpuFrame ? gpuView : cpuView))
                    status.Text = $"{backend}  |  {info.Width}×{info.Height} DIP  |  {raw.Width}×{raw.Height} device px  |  {raw.Width / (double)Math.Max(info.Width, 1):F2}× DPI";
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Gallery draw failed: {ex}");
            Dispatcher.Dispatch(() =>
            {
                if (!active) return;
                Stop();
                state.Text = $"Rendering failed: {ex.Message}";
                state.IsVisible = true;
                retry.IsVisible = true;
                status.Text = "Rendering failed · retry or return to gallery";
                UpdateButtons();
            });
        }
    }

    private void GenerateDocument(DocumentSampleBase document)
    {
        lock (sampleGate)
        {
            using var surface = SKSurface.Create(new SKImageInfo(1, 1));
            if (surface is null) throw new InvalidOperationException("Could not create the document generation surface.");
            document.DrawSample(surface.Canvas, 1, 1);
        }
    }

    private void UpdateDocumentState()
    {
        state.IsVisible = true;
        state.Text = sample.HasDownload
            ? "Document generated. Open or share the native file."
            : "Document preview unavailable. This platform cannot generate the document.";
        status.Text = sample.HasDownload
            ? "Document preview · native open/share available"
            : "Document generation unavailable";
    }

    private void UpdateButtons()
    {
        open.IsVisible = sample is DocumentSampleBase;
        open.IsEnabled = active && sample.HasDownload;
        share.IsVisible = sample is DocumentSampleBase || sample.HasDownload;
        share.IsEnabled = active && sample.HasDownload;
        actionScroller.IsVisible = open.IsVisible || share.IsVisible || retry.IsVisible;
    }

    private void UpdateLayout()
    {
        if (Width <= 0 || Height <= 0) return;
        var wide = Width >= 850;
        if (!userToggledControls) controlsExpanded = wide || Height >= 520;
        PlaceControlsToggle(wide);
        toggleControls.IsVisible = hasControlPanel;
        surfaceCard.MinimumHeightRequest = Height < 520 ? 110 : 200;
        var hasPanel = hasControlPanel && (!wide || controlsExpanded);
        controlPanel.IsVisible = hasPanel;
        body.RowDefinitions.Clear();
        body.ColumnDefinitions.Clear();
        if (wide)
        {
            body.RowDefinitions.Add(new RowDefinition(GridLength.Star));
            body.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            if (hasPanel) body.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(330)));
            body.SetRow(body.Children[0], 0); body.SetColumn(body.Children[0], 0);
            body.SetRow(body.Children[1], 0); body.SetColumn(body.Children[1], hasPanel ? 1 : 0);
            controlScroller.HeightRequest = -1;
        }
        else
        {
            body.RowDefinitions.Add(new RowDefinition(GridLength.Star));
            if (hasPanel)
                body.RowDefinitions.Add(new RowDefinition(controlsExpanded ? new GridLength(Math.Min(260, Math.Max(125, Height * 0.36))) : new GridLength(50)));
            body.SetRow(body.Children[0], 0); body.SetColumn(body.Children[0], 0);
            body.SetRow(body.Children[1], hasPanel ? 1 : 0); body.SetColumn(body.Children[1], 0);
            controlScroller.HeightRequest = -1;
        }
        controlScroller.IsVisible = controlsExpanded;
        toggleControls.Text = controlsExpanded ? "Controls ▾" : "Controls ▸";
    }

    private void PlaceControlsToggle(bool wide)
    {
        if (toggleOverCanvas == wide) return;
        toggleOverCanvas = wide;
        if (wide)
        {
            controlPanel.Children.Remove(toggleControls);
            toggleControls.HorizontalOptions = LayoutOptions.End;
            toggleControls.VerticalOptions = LayoutOptions.Start;
            toggleControls.Margin = new Thickness(8);
            toggleControls.ZIndex = 10;
            surfaceHost.Children.Add(toggleControls);
        }
        else
        {
            surfaceHost.Children.Remove(toggleControls);
            toggleControls.Margin = new Thickness(0);
            toggleControls.HorizontalOptions = LayoutOptions.Fill;
            toggleControls.VerticalOptions = LayoutOptions.Fill;
            toggleControls.ZIndex = 0;
            controlPanel.Add(toggleControls, 0, 0);
        }
    }

    private void RefreshControls()
    {
        controlUpdaters.Clear();
        var controls = sample.Controls;
        controlItemsData = controls.Select(control => MakeControl(control, "")).ToArray();
        OnPropertyChanged(nameof(ControlItems));
        OnPropertyChanged(nameof(NoControls));
        UpdateOutputInfo(controls.Count);
        UpdateLayout();
    }

    private ControlItem MakeControl(SampleControl control, string prefix)
    {
        var id = prefix.Length == 0 ? control.Id : $"{prefix}.{control.Id}";
        var item = new ControlItem(id, control, ChangeControl);
        controlUpdaters.Add(id, item);
        if (control is GroupControl group)
            foreach (var child in group.Children)
                item.Children.Add(MakeControl(child, id));
        return item;
    }

    private void SyncControls()
    {
        var controls = sample.Controls;
        var count = 0;
        var rebuild = false;
        void Sync(SampleControl control, string prefix)
        {
            var id = prefix.Length == 0 ? control.Id : $"{prefix}.{control.Id}";
            if (!controlUpdaters.TryGetValue(id, out var updater) || updater.Kind != control.GetType())
            {
                rebuild = true;
                return;
            }
            updater.Update(control);
            count++;
            if (control is GroupControl group)
                foreach (var child in group.Children) Sync(child, id);
        }

        foreach (var control in controls) Sync(control, "");
        if (rebuild || count != controlUpdaters.Count)
        {
            RefreshControls();
            return;
        }
        var hadPanel = hasControlPanel;
        UpdateOutputInfo(controls.Count);
        if (hadPanel != hasControlPanel) UpdateLayout();
    }

    private void UpdateOutputInfo(int controlCount)
    {
        var canDownload = sample.HasDownload;
        var nextVisible = canDownload && sample is not DocumentSampleBase;
        var nextText = nextVisible ? $"Output: {sample.DownloadFileName}" : "";
        if (showOutputInfo != nextVisible)
        {
            showOutputInfo = nextVisible;
            OnPropertyChanged(nameof(ShowOutputInfo));
        }
        if (outputText != nextText) { outputText = nextText; OnPropertyChanged(nameof(OutputText)); }
        hasControlPanel = controlCount > 0 || canDownload;
    }

    internal static float Snap(SliderControl slider, double value)
    {
        if (!double.IsFinite(value) || !float.IsFinite(slider.Min) ||
            !float.IsFinite(slider.Max) || slider.Max <= slider.Min)
            return slider.Value;
        var bounded = Math.Clamp(value, slider.Min, slider.Max);
        var step = slider.Step > 0 ? slider.Step : (slider.Max - slider.Min) / 1000f;
        if (float.IsFinite(step) && step > 0)
            bounded = slider.Min + Math.Round((bounded - slider.Min) / step, MidpointRounding.AwayFromZero) * step;
        return (float)Math.Clamp(bounded, slider.Min, slider.Max);
    }

    private void ChangeControl(string id, object value)
    {
        if (!active || !sample.IsInitialized) return;
        try
        {
            lock (sampleGate)
            {
                sample.UpdateControl(id, value);
                if (sample is DocumentSampleBase document) GenerateDocument(document);
            }
            SyncControls();
            UpdateButtons();
            if (sample is DocumentSampleBase) UpdateDocumentState();
            cpuView?.InvalidateSurface();
            gpuView?.InvalidateSurface();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Gallery control {id} failed: {ex}");
            state.Text = $"Could not update {id}: {ex.Message}";
            state.IsVisible = true;
            status.Text = "Control update failed · retry or return to gallery";
            retry.IsVisible = true;
            UpdateButtons();
        }
    }

    private async Task ExportAsync(bool openFile)
    {
        if (!active) return;
        try
        {
            byte[] bytes;
            string name;
            lock (sampleGate)
            {
                if (sample is DocumentSampleBase doc && !doc.HasDownload) GenerateDocument(doc);
                bytes = sample.DownloadBytes?.ToArray() ?? throw new InvalidOperationException("This sample has no generated file to export.");
                name = Path.GetFileName(sample.DownloadFileName);
            }
            if (string.IsNullOrWhiteSpace(name)) name = "gallery-output.bin";
            var file = Path.Combine(FileSystem.CacheDirectory, name);
            await File.WriteAllBytesAsync(file, bytes);
            if (!active) return;
            if (openFile)
            {
                var launched = await Launcher.Default.OpenAsync(new OpenFileRequest
                {
                    Title = $"Open {name}",
                    File = new ReadOnlyFile(file, sample.DownloadMimeType)
                });
                if (!launched) throw new InvalidOperationException("No app is available to open this file.");
            }
            else
            {
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = $"Save or share {name}",
                    File = new ShareFile(file, sample.DownloadMimeType)
                });
            }
            if (active) status.Text = openFile ? $"Opened {name}" : $"Opened share sheet for {name}";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Gallery export failed: {ex}");
            if (active)
            {
                status.Text = $"Export failed: {ex.Message}";
                await DisplayAlertAsync("File unavailable", $"Unable to {(openFile ? "open" : "share")} this sample: {ex.Message}", "OK");
            }
        }
    }
}
