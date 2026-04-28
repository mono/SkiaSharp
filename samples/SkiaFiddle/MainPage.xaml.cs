using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using SkiaFiddle.Fiddle;
using SkiaSharp;

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
        // SamplesCombo.SelectedIndex = 0 (set in InitSamples) fires OnSampleChanged,
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
        foreach (var sample in SampleSnippets.All)
            SamplesCombo.Items.Add(new ComboBoxItem { Content = sample.Name, Tag = sample });
        SamplesCombo.SelectedIndex = 0;
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
            var result = await _compiler.CompileAsync(SetupEditor.Text, DrawEditor.Text);
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
