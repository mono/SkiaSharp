#nullable enable

using System;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Uno.Foundation;

namespace SkiaSharpSample;

public partial class App
{
    partial void StartBrowserSplashRendering()
    {
        var window = MainWindow ?? throw new InvalidOperationException("The application window has not been created.");
        var timeout = window.DispatcherQueue.CreateTimer();
        timeout.Interval = TimeSpan.FromSeconds(60);
        timeout.IsRepeating = false;

        // Uno 6.7 can request only a present while splash removal waits for a recorded frame.
        // Keep requesting records until Uno dismisses its own loader (unoplatform/uno#24260).
        CompositionTarget.Rendering += OnRendering;
        window.Closed += OnClosed;
        timeout.Tick += OnTimeout;
        timeout.Start();

        void OnRendering(object? sender, object args)
        {
            if (WebAssemblyRuntime.InvokeJS("document.querySelector('.uno-loader') === null") == "true")
                Stop();
        }

        void OnClosed(object sender, WindowEventArgs args) => Stop();

        void OnTimeout(DispatcherQueueTimer sender, object args)
        {
            Stop();
            Console.Error.WriteLine("Uno did not dismiss its splash screen within 60 seconds.");
        }

        void Stop()
        {
            timeout.Stop();
            timeout.Tick -= OnTimeout;
            window.Closed -= OnClosed;
            CompositionTarget.Rendering -= OnRendering;
        }
    }
}
