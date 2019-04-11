using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;
using Windows.ApplicationModel;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace SkiaSharp.Views.Uno
{
	public class SKXamlCanvas : FrameworkElement
	{
		private bool ignorePixelScaling;
		private bool isVisible;
		private IntPtr pixels;
		private int _pixelWidth;
		private int _pixelHeight;
		private static bool isInitialized;

		private static bool designMode = DesignMode.DesignModeEnabled;

		public SKXamlCanvas()
			: base("canvas")
		{
			Loaded += OnLoaded;
			Unloaded += OnUnloaded;
			SizeChanged += OnSizeChanged;

			RegisterPropertyChangedCallback(VisibilityProperty, (s, e) => OnVisibilityChanged(s));
			OnVisibilityChanged(this);
		}

		public bool IgnorePixelScaling
		{
			get => ignorePixelScaling;
			set
			{
				ignorePixelScaling = value;
				Invalidate();
			}
		}

		public double Dpi { get; private set; } = 1;

		public static bool IsInitialized
		{
			get
			{
				if (!isInitialized)
				{
					if (WebAssemblyRuntime.InvokeJS($"typeof CanvasKit !== 'undefined'") == "true")
					{
						isInitialized = true;
					}
				}

				return isInitialized;
			}
		}

		public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

		protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		private static void OnVisibilityChanged(DependencyObject d)
		{
			if (d is SKXamlCanvas canvas)
			{
				canvas.isVisible = canvas.Visibility == Visibility.Visible;
				canvas.Invalidate();
			}
		}

		private void OnDpiChanged(DisplayInformation sender, object args = null)
		{
			Dpi = sender.LogicalDpi / 96.0f;
			Invalidate();
		}

		private void OnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			Invalidate();
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			//var display = DisplayInformation.GetForCurrentView();
			//display.DpiChanged += OnDpiChanged;

			//OnDpiChanged(display);
			Invalidate();
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			var display = DisplayInformation.GetForCurrentView();
			display.DpiChanged -= OnDpiChanged;

			FreeBitmap();
		}

		public void Invalidate()
		{
			if (Dispatcher.HasThreadAccess)
			{
				DoInvalidate();
			}
			else
			{
				Dispatcher.RunAsync(CoreDispatcherPriority.Normal, DoInvalidate);
			}
		}

		private void DoInvalidate()
		{
			if (!IsInitialized)
			{
				var _ = Dispatcher.RunAsync(
					CoreDispatcherPriority.Normal,
					async () =>
					{
						await Task.Delay(500);
						DoInvalidate();
					});
				return;
			}

			if (designMode)
				return;

			if (!(ActualWidth > 0 && ActualHeight > 0) || !isVisible)
			{
				Console.WriteLine($"Ignore Invalidate {ActualWidth}x{ActualHeight} isVisible:{isVisible}");

				return;
			}

			int width, height;
			if (IgnorePixelScaling)
			{
				width = (int)ActualWidth;
				height = (int)ActualHeight;
			}
			else
			{
				width = (int)(ActualWidth * Dpi);
				height = (int)(ActualHeight * Dpi);
			}

			var info = new SKImageInfo(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Opaque);
			CreateBitmap(info);
			using (var surface = SKSurface.Create(info, pixels, info.RowBytes))
			{
				OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info));
			}

			WebAssemblyRuntime.InvokeJS($"SkiaSharp.SurfaceManager.invalidateCanvas({pixels}, \"{HtmlId}\", {info.Width}, {_pixelHeight});");
		}

		private unsafe void CreateBitmap(SKImageInfo info)
		{
			if (pixels == IntPtr.Zero || _pixelWidth != info.Width || _pixelHeight != info.Height)
			{
				FreeBitmap();

				if (!int.TryParse(WebAssemblyRuntime.InvokeJS($"CanvasKit._malloc({info.Width}*{info.Height}*4)"), out var ptr))
				{
					throw new InvalidOperationException($"Failed to get a CanvasKit allocation");
				}

				Console.WriteLine($"Allocated new buffer ({ptr}, {info.Width}x{info.Height})");

				pixels = (IntPtr)ptr;
				_pixelWidth = info.Width;
				_pixelHeight = info.Height;
			}
		}

		private void FreeBitmap()
		{
			if (pixels != IntPtr.Zero)
			{
				WebAssemblyRuntime.InvokeJS($"CanvasKit._free({pixels}); true;");
			}
		}
	}
}
