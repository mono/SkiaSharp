using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using SkiaFiddle.Fiddle;
using SkiaSharp;
using Uno.Foundation;

namespace SkiaFiddle;

public sealed partial class MainPage : Page
{
    private IFiddleCompiler _compiler = new RoslynFiddleCompiler();
    private bool _samplesInitialized;

    public MainPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        InitSamples();
        StatusText.Text = $"engine: {_compiler.Name}";
        OutputCanvas.FpsUpdated += (_, fps) => DispatcherQueue.TryEnqueue(() =>
        {
            FpsText.Text = $"{fps:0.0} fps";
        });
        // SamplesCombo.SelectedIndex (set in InitSamples) fires OnSampleChanged,
        // which seeds both editors and runs.
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
            SetupEditor.Text = sample.Setup;
            DrawEditor.Text = sample.Draw;
            _ = RunAsync();
        }
    }

    private async void OnRunClicked(object sender, RoutedEventArgs e) => await RunAsync();

    // Monaco's onDidChangeContent → managed CodeEditor.Text round-trip is unreliable
    // under Uno WASM (the property lags behind keystrokes), so we ask Monaco directly
    // for its current model values right before compiling. DOM order matches XAML
    // declaration order: SetupEditor first (top), DrawEditor second (bottom).
    private (string setup, string draw) GetEditorTexts()
    {
        try
        {
            var json = WebAssemblyRuntime.InvokeJS("globalThis.skiaFiddleGetMonacoValues ? globalThis.skiaFiddleGetMonacoValues() : '[]'");
            var values = JsonSerializer.Deserialize<string[]>(json) ?? Array.Empty<string>();
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
        PauseLabel.Text = OutputCanvas.IsAnimating ? "⏸ Pause" : "▶ Play";
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
                PauseLabel.Text = "⏸ Pause";
                MessageText.Foreground = (Microsoft.UI.Xaml.Media.Brush)Resources["TextSecondary"];
                MessageText.Text = result.Diagnostics ?? "Compiled in " + result.ElapsedMs + " ms";
            }
            else
            {
                OutputCanvas.SetError(result.Diagnostics ?? "Unknown error");
                PauseLabel.Text = "▶ Play";
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
}
