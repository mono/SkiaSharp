using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using SkiaSharpSample.Controls;

namespace SkiaSharpSample;

public sealed class SampleDetailPage : ContentPage
{
    private readonly SampleBase sample;
    private readonly Func<Task> navigateBack;
    private readonly GalleryWindowSettings windowSettings;
    private readonly Grid root;
    private bool navigatingBack;
    private readonly object sampleGate = new();
    private readonly Grid surfaceHost = new();
    private readonly Border surfaceCard;
    private readonly VerticalStackLayout controlItems = new() { Spacing = 14, Padding = 14 };
    private readonly Grid body = new();
    private readonly Grid controlPanel = new();
    private readonly ScrollView controlScroller;
    private readonly Label state;
    private readonly Label status;
    private readonly Dictionary<string, (Type Type, Action<SampleControl> Apply)> controlUpdaters = new(StringComparer.Ordinal);
    private readonly Button retry;
    private readonly Button open;
    private readonly Button share;
    private readonly ScrollView actionScroller;
    private readonly Button toggleControls;
    private SKCanvasView? cpuView;
    private SKGLView? gpuView;
    private Task? initTask;
    private Window? subscribedWindow;
    private bool active;
    private bool useGpu;
    private bool controlsExpanded = true;
    private bool userToggledControls;
    private bool syncingControls;
    private Label? downloadInfo;
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
        Title = sample.Title;
        NavigationPage.SetHasNavigationBar(this, false);
        BackgroundColor = GalleryUi.Navy;
        SafeAreaEdges = new Microsoft.Maui.SafeAreaEdges(Microsoft.Maui.SafeAreaRegions.None);

        var heading = new VerticalStackLayout { Spacing = 5, Padding = new Thickness(16, 12, 16, 8) };
        GalleryUi.Background(heading, "PageBackground");
        var title = GalleryUi.Text(sample.Title, 24, true);
        title.AutomationId = "sample-title";
        heading.Children.Add(title);
        heading.Children.Add(GalleryUi.Text(sample.Description, 13, color: "SecondaryText"));
        var tags = new HorizontalStackLayout { Spacing = 8 };
        foreach (var tag in sample.ApiTags)
        {
            var chip = GalleryUi.Text($"  {tag}  ", 11);
            GalleryUi.Background(chip, KnownApis.Classify(tag) == TagKind.Type ? "TypeBackground" : "MethodBackground");
            GalleryUi.TextColor(chip, Label.TextColorProperty, "AccentText");
            tags.Children.Add(chip);
        }
        heading.Children.Add(new ScrollView { Orientation = ScrollOrientation.Horizontal, Content = tags, HeightRequest = 27 });

        var actions = new HorizontalStackLayout { Spacing = 8, Padding = new Thickness(16, 4) };
        open = GalleryUi.Action("Open file", "sample-open", async (_, _) => await ExportAsync(openFile: true));
        share = GalleryUi.Action("Share / Save", "sample-share", async (_, _) => await ExportAsync(openFile: false));
        actions.Children.Add(open);
        actions.Children.Add(share);
        retry = GalleryUi.Action("Retry", "sample-retry", (_, _) =>
        {
            Stop();
            _ = StartAsync();
        });
        retry.IsVisible = false;
        actions.Children.Add(retry);
        actionScroller = new ScrollView { Content = actions, Orientation = ScrollOrientation.Horizontal, HeightRequest = 55 };
        GalleryUi.Background(actionScroller, "PageBackground");

        state = GalleryUi.Text("Loading sample…", 14);
        state.AutomationId = "sample-state";
        state.HorizontalTextAlignment = TextAlignment.Center;
        state.VerticalTextAlignment = TextAlignment.Center;
        status = GalleryUi.Text("Initializing…", 11, color: "SecondaryText");
        status.AutomationId = "sample-render-status";
        var statusBar = new Grid { Padding = new Thickness(16, 0, 16, 4) };
        GalleryUi.Background(statusBar, "PageBackground");
        statusBar.Add(status);
        surfaceHost.Children.Add(state);
        surfaceCard = GalleryUi.Card(surfaceHost);
        surfaceCard.Margin = new Thickness(12, 4, 12, 6);
        surfaceCard.MinimumHeightRequest = 200;
        body.Children.Add(surfaceCard);

        controlPanel.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        controlPanel.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        controlPanel.Margin = new Thickness(12, 4, 12, 6);
        toggleControls = GalleryUi.Action("Controls ▾", "sample-controls-toggle", (_, _) =>
        {
            userToggledControls = true;
            controlsExpanded = !controlsExpanded;
            UpdateLayout();
        });
        controlPanel.Add(toggleControls);
        controlScroller = new ScrollView { Content = controlItems, AutomationId = "sample-controls" };
        controlPanel.Add(controlScroller, 0, 1);
        controlPanel.IsVisible = false;
        controlPanel.IsEnabled = false;
        controlItems.Children.Add(GalleryUi.Text("Loading controls…", color: "SecondaryText"));
        body.Add(controlPanel);
        GalleryUi.Background(body, "PageBackground");

        root = new Grid
        {
            RowDefinitions = new RowDefinitionCollection
            {
                new(GridLength.Auto), new(GridLength.Auto), new(GridLength.Auto),
                new(GridLength.Star), new(GridLength.Auto)
            },
            SafeAreaEdges = new Microsoft.Maui.SafeAreaEdges(Microsoft.Maui.SafeAreaRegions.Container)
        };
        root.Add(new GalleryHeader(windowSettings, GoBackAsync));
        root.Add(heading, 0, 1);
        root.Add(actionScroller, 0, 2);
        root.Add(body, 0, 3);
        root.Add(statusBar, 0, 4);
        Content = root;
        SizeChanged += (_, _) => UpdateLayout();
        UpdateButtons();
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
        surfaceCard.MinimumHeightRequest = Height < 520 ? 110 : 200;
        var hasPanel = controlPanel.IsVisible;
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

    private void RefreshControls()
    {
        controlUpdaters.Clear();
        controlItems.Children.Clear();
        var controls = sample.Controls;
        if (controls.Count == 0)
            controlItems.Children.Add(GalleryUi.Text("No adjustable controls for this sample.", color: "SecondaryText"));
        else
            foreach (var control in controls)
                controlItems.Children.Add(RenderControl(control, ""));
        downloadInfo = GalleryUi.Text("", 12, color: "SecondaryText");
        controlItems.Children.Add(downloadInfo);
        UpdateOutputInfo(controls.Count);
        UpdateLayout();
    }

    private void SyncControls()
    {
        var controls = sample.Controls;
        var count = 0;
        var rebuild = false;
        void Sync(SampleControl control, string prefix)
        {
            var id = prefix.Length == 0 ? control.Id : $"{prefix}.{control.Id}";
            if (!controlUpdaters.TryGetValue(id, out var updater) || updater.Type != control.GetType())
            {
                rebuild = true;
                return;
            }
            updater.Apply(control);
            count++;
            if (control is GroupControl group)
                foreach (var child in group.Children) Sync(child, id);
        }

        syncingControls = true;
        try
        {
            foreach (var control in controls) Sync(control, "");
        }
        finally
        {
            syncingControls = false;
        }
        if (rebuild || count != controlUpdaters.Count)
        {
            RefreshControls();
            return;
        }
        var wasVisible = controlPanel.IsVisible;
        UpdateOutputInfo(controls.Count);
        if (wasVisible != controlPanel.IsVisible) UpdateLayout();
    }

    private void UpdateOutputInfo(int controlCount)
    {
        var canDownload = sample.HasDownload;
        if (downloadInfo is { } label)
        {
            label.Text = canDownload && sample is not DocumentSampleBase
                ? $"Output: {sample.DownloadFileName}" : "";
            label.IsVisible = canDownload && sample is not DocumentSampleBase;
        }
        controlPanel.IsVisible = controlCount > 0 || canDownload;
    }

    private View RenderControl(SampleControl control, string prefix)
    {
        var id = prefix.Length == 0 ? control.Id : $"{prefix}.{control.Id}";
        var content = new VerticalStackLayout { Spacing = 5 };
        if (control is GroupControl group)
        {
            var heading = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Auto) } };
            heading.Add(GalleryUi.Text(group.Label, 14, true));
            var enabled = new Switch { IsToggled = group.Enabled, AutomationId = GalleryUi.StableId("control-", id) };
            SemanticProperties.SetDescription(enabled, $"{group.Label}: {(group.Enabled ? "enabled" : "disabled")}. {group.Description}");
            heading.Add(enabled, 1);
            content.Children.Add(heading);
            var children = new VerticalStackLayout { Spacing = 12, Padding = new Thickness(12, 5, 0, 0), IsVisible = group.Enabled };
            foreach (var child in group.Children) children.Children.Add(RenderControl(child, id));
            content.Children.Add(children);
            controlUpdaters[id] = (typeof(GroupControl), updated =>
            {
                var value = ((GroupControl)updated).Enabled;
                if (enabled.IsToggled != value) enabled.IsToggled = value;
                children.IsVisible = value;
            });
            enabled.Toggled += (_, e) =>
            {
                if (syncingControls) return;
                children.IsVisible = e.Value;
                ChangeControl(id, e.Value);
            };
        }
        else if (control is SliderControl slider)
        {
            var label = GalleryUi.Text($"{slider.Label}: {slider.Value:G4}", 14, true);
            content.Children.Add(label);
            var fixedId = GalleryUi.StableId("control-", id);
            label.AutomationId = $"{fixedId}-value";
            var current = slider;
            var valid = float.IsFinite(slider.Min) && float.IsFinite(slider.Max) && slider.Max > slider.Min;
            var range = new Slider
            {
                Minimum = valid ? slider.Min : 0,
                Maximum = valid ? slider.Max : 1,
                Value = valid ? Math.Clamp(slider.Value, slider.Min, slider.Max) : 0,
                IsVisible = valid,
                IsEnabled = valid,
                AutomationId = fixedId
            };
            label.Text = valid ? $"{slider.Label}: {slider.Value:G4}" : $"{slider.Label}: {slider.Value:G4} (fixed)";
            SemanticProperties.SetDescription(label, valid
                ? $"{slider.Label}: {slider.Value:G4}. {slider.Description}"
                : $"{slider.Label}: {slider.Value:G4}. Fixed value. {slider.Description}");
            SemanticProperties.SetDescription(range, $"{slider.Label}: {slider.Value:G4}. {slider.Description}");
            controlUpdaters[id] = (typeof(SliderControl), updated =>
            {
                current = (SliderControl)updated;
                var adjustable = float.IsFinite(current.Min) && float.IsFinite(current.Max) && current.Max > current.Min;
                range.IsVisible = adjustable;
                range.IsEnabled = adjustable;
                if (adjustable)
                {
                    // Widen before moving the lower bound to avoid MAUI's range coercion.
                    if (range.Maximum < current.Max) range.Maximum = current.Max;
                    if (range.Minimum != current.Min) range.Minimum = current.Min;
                    if (range.Maximum != current.Max) range.Maximum = current.Max;
                    var value = Math.Clamp(current.Value, current.Min, current.Max);
                    if (range.Value != value) range.Value = value;
                }
                label.Text = adjustable ? $"{current.Label}: {current.Value:G4}" : $"{current.Label}: {current.Value:G4} (fixed)";
                SemanticProperties.SetDescription(label, adjustable
                    ? $"{current.Label}: {current.Value:G4}. {current.Description}"
                    : $"{current.Label}: {current.Value:G4}. Fixed value. {current.Description}");
                SemanticProperties.SetDescription(range, $"{current.Label}: {current.Value:G4}. {current.Description}");
            });
            range.ValueChanged += (_, e) =>
            {
                if (syncingControls || !active || !sample.IsInitialized || !range.IsEnabled) return;
                var value = Snap(current, e.NewValue);
                if (value == current.Value) return;
                // The newly evaluated model is pushed back into this same view;
                // never rebuild the panel during a drag or an automation Value update.
                ChangeControl(id, value);
            };
            content.Children.Add(range);
        }
        else if (control is PickerControl picker)
        {
            content.Children.Add(GalleryUi.Text(picker.Label, 14, true));
            var select = new Picker { AutomationId = GalleryUi.StableId("control-", id) };
            GalleryUi.StylePicker(select);
            SemanticProperties.SetDescription(select, $"{picker.Label}. {picker.Description}");
            foreach (var option in picker.Options) select.Items.Add(option);
            select.SelectedIndex = Math.Clamp(picker.SelectedIndex, -1, select.Items.Count - 1);
            controlUpdaters[id] = (typeof(PickerControl), updated =>
            {
                var value = (PickerControl)updated;
                var optionsChanged = select.Items.Count != value.Options.Length;
                for (var i = 0; !optionsChanged && i < value.Options.Length; i++)
                    optionsChanged = select.Items[i] != value.Options[i];
                if (optionsChanged)
                {
                    select.Items.Clear();
                    foreach (var option in value.Options) select.Items.Add(option);
                }
                var index = Math.Clamp(value.SelectedIndex, -1, select.Items.Count - 1);
                if (select.SelectedIndex != index) select.SelectedIndex = index;
            });
            select.SelectedIndexChanged += (_, _) =>
            {
                if (!syncingControls && select.SelectedIndex >= 0) ChangeControl(id, select.SelectedIndex);
            };
            content.Children.Add(select);
        }
        else if (control is ToggleControl toggle)
        {
            var row = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Auto) } };
            row.Add(GalleryUi.Text(toggle.Label, 14, true));
            var check = new Switch { IsToggled = toggle.Value, AutomationId = GalleryUi.StableId("control-", id) };
            SemanticProperties.SetDescription(check, $"{toggle.Label}. {toggle.Description}");
            row.Add(check, 1);
            controlUpdaters[id] = (typeof(ToggleControl), updated =>
            {
                var value = ((ToggleControl)updated).Value;
                if (check.IsToggled != value) check.IsToggled = value;
            });
            check.Toggled += (_, e) =>
            {
                if (!syncingControls) ChangeControl(id, e.Value);
            };
            content.Children.Add(row);
        }
        if (!string.IsNullOrWhiteSpace(control.Description))
            content.Children.Add(GalleryUi.Text(control.Description, 12, color: "SecondaryText"));
        var card = GalleryUi.Card(content, 10);
        card.Padding = 10;
        return card;
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
