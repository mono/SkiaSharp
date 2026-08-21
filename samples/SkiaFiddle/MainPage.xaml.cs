using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using SkiaFiddle.Fiddle;
using SkiaSharp;
using Uno.Foundation;

namespace SkiaFiddle;

public sealed partial class MainPage : Page
{
    private IFiddleCompiler _compiler = new RoslynFiddleCompiler();
    private bool _samplesInitialized;
    private bool _selectorsInitialized;

    public MainPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        InitSelectors();
        InitSamples();
        StatusText.Text = $"engine: {_compiler.Name}";
        OutputCanvas.FpsUpdated += (_, fps) => DispatcherQueue.TryEnqueue(() =>
        {
            FpsText.Text = $"{fps:0.0} fps";
        });
        // SamplesCombo.SelectedIndex (set in InitSamples) fires OnSampleChanged,
        // which seeds both editors and runs.
    }

    private void InitSelectors()
    {
        if (_selectorsInitialized)
            return;
        _selectorsInitialized = true;

        foreach (var font in FiddleAssets.Fonts)
            FontCombo.Items.Add(new ComboBoxItem { Content = font.Name });
        FontCombo.SelectedIndex = FiddleAssets.SelectedFontIndex;

        ImageStrip.SelectedIndex = FiddleAssets.SelectedImageIndex;
        ImageStrip.SelectionChanged += OnImageStripChanged;
    }

    private void OnFontChanged(object sender, SelectionChangedEventArgs e)
    {
        if (FontCombo.SelectedIndex < 0)
            return;
        FiddleAssets.SelectedFontIndex = FontCombo.SelectedIndex;
        OutputCanvas.RequestRedraw();
    }

    private void OnImageStripChanged(object? sender, int index)
    {
        FiddleAssets.SelectedImageIndex = index;
        OutputCanvas.RequestRedraw();
    }

    private void OnImageStripPressed(object sender, PointerRoutedEventArgs e)
    {
        var x = e.GetCurrentPoint(ImageStripHost).Position.X;
        ImageStrip.SelectFromPointer(x, ImageStripHost.ActualWidth);
    }

    public void SetCompiler(IFiddleCompiler compiler)
    {
        _compiler = compiler;
        StatusText.Text = $"engine: {_compiler.Name}";
    }

    private void InitSamples()
    {
        if (_samplesInitialized)
            return;
        _samplesInitialized = true;
        var defaultIndex = 0;
        for (var i = 0; i < SampleSnippets.All.Count; i++)
        {
            var sample = SampleSnippets.All[i];
            SamplesCombo.Items.Add(new ComboBoxItem { Content = sample.Name, Tag = sample });
            if (sample.Name == "Animated · Orbits")
                defaultIndex = i;
        }
        SamplesCombo.SelectedIndex = defaultIndex;
    }

    private void OnSampleChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SamplesCombo.SelectedItem is ComboBoxItem item && item.Tag is FiddleSample sample)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                ApplySampleAssets(sample);
                SetupEditor.Text = sample.Setup;
                DrawEditor.Text = sample.Draw;
                _ = RunAsync();
            });
        }
    }

    // Presets the font/image selectors when a sample declares which asset its
    // `typeface`/`image` variables expect (e.g. the variable-font demo wants Inter).
    private void ApplySampleAssets(FiddleSample sample)
    {
        if (sample.Font is { } fontName)
        {
            var index = FiddleAssets.IndexOfFont(fontName);
            if (index >= 0)
                FontCombo.SelectedIndex = index; // OnFontChanged updates FiddleAssets
        }

        if (sample.Image is { } imageIndex)
        {
            FiddleAssets.SelectedImageIndex = imageIndex;
            ImageStrip.SelectedIndex = imageIndex;
        }
    }

    private void OnRunClicked(object sender, RoutedEventArgs e) => DispatcherQueue.TryEnqueue(() => _ = RunAsync());

    // Monaco's onDidChangeContent → managed CodeEditor.Text round-trip is unreliable
    // under Uno WASM (the property lags behind keystrokes), so we ask Monaco directly
    // for its current model values right before compiling. DOM order matches XAML
    // declaration order: SetupEditor first (top), DrawEditor second (bottom).
    private (string setup, string draw) GetEditorTexts()
    {
        try
        {
            var json = WebAssemblyRuntime.InvokeJS("globalThis.skiaFiddleGetMonacoValues ? globalThis.skiaFiddleGetMonacoValues() : '[]'");
            var values = JsonSerializer.Deserialize(json, FiddleJsonContext.Default.StringArray) ?? Array.Empty<string>();
            var setup = values.Length > 0 ? values[0] : SetupEditor.Text ?? string.Empty;
            var draw = values.Length > 1 ? values[1] : DrawEditor.Text ?? string.Empty;
            return (setup, draw);
        }
        catch
        {
            return (SetupEditor.Text ?? string.Empty, DrawEditor.Text ?? string.Empty);
        }
    }

    private void OnPauseClicked(object sender, RoutedEventArgs e)
    {
        OutputCanvas.TogglePause();
        PauseLabel.Text = OutputCanvas.IsAnimating ? "Stop" : "Play";
        if (!OutputCanvas.IsAnimating)
            FpsText.Text = "paused";
    }

    private async Task RunAsync()
    {
        RunButton.IsEnabled = false;
        MessageText.Foreground = (Microsoft.UI.Xaml.Media.Brush)Resources["TextSecondary"];
        MessageText.Text = "Compiling…";
        FpsText.Text = "";
        try
        {
            var (setup, draw) = GetEditorTexts();
            var result = await _compiler.CompileAsync(setup, draw);
            if (result.Draw is not null)
            {
                OutputCanvas.SetDrawDelegate(result.Draw);
                PauseLabel.Text = "Stop";
                MessageText.Foreground = (Microsoft.UI.Xaml.Media.Brush)Resources["TextSecondary"];
                MessageText.Text = result.Diagnostics ?? "Compiled in " + result.ElapsedMs + " ms";
            }
            else
            {
                OutputCanvas.SetError(result.Diagnostics ?? "Unknown error");
                PauseLabel.Text = "Play";
                FpsText.Text = "";
                MessageText.Foreground = (Microsoft.UI.Xaml.Media.Brush)Resources["ErrorBrush"];
                MessageText.Text = (result.Diagnostics ?? "Compile failed").Split('\n')[0];
            }
        }
        catch (Exception ex)
        {
            OutputCanvas.SetError(ex.ToString());
            MessageText.Foreground = (Microsoft.UI.Xaml.Media.Brush)Resources["ErrorBrush"];
            MessageText.Text = ex.Message;
        }
        finally
        {
            RunButton.IsEnabled = true;
        }
    }

    [JsonSerializable(typeof(string[]))]
    internal partial class FiddleJsonContext : JsonSerializerContext;
}
