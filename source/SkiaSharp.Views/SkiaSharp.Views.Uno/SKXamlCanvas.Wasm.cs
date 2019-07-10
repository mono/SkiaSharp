#if __WASM__
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;
using Uno.Foundation;
using Windows.ApplicationModel;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace SkiaSharp.Views.UWP
{
	public partial class SKXamlCanvas : FrameworkElement
	{
		private IntPtr pixels;
		private int _pixelWidth;
		private int _pixelHeight;

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

		private SKSize GetCanvasSize() => new SKSize(_pixelWidth, _pixelHeight);

		private static bool GetIsInitialized() => true;

		private void OnDpiChanged(DisplayInformation sender, object args = null)
		{
			Dpi = sender.LogicalDpi / 96.0f;
			Invalidate();
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			var display = DisplayInformation.GetForCurrentView();
			display.DpiChanged += OnDpiChanged;

			OnDpiChanged(display);
			Invalidate();
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			var display = DisplayInformation.GetForCurrentView();
			display.DpiChanged -= OnDpiChanged;

			FreeBitmap();
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
				// Console.WriteLine($"info: BytesSize={info.BytesSize} ColorType={info.ColorType} IsOpaque:{info.IsOpaque} {info.Width}x{info.Height}");
				OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info));
			}

			WebAssemblyRuntime.InvokeJS($"SkiaSharp.SurfaceManager.invalidateCanvas({pixels}, \"{HtmlId}\", {info.Width}, {_pixelHeight});");
		}

		private unsafe void CreateBitmap(SKImageInfo info)
		{
			if (pixels == IntPtr.Zero || _pixelWidth != info.Width || _pixelHeight != info.Height)
			{
				FreeBitmap();

				if (!int.TryParse(WebAssemblyRuntime.InvokeJS($"Module._malloc({info.Width}*{info.Height}*4)"), out var ptr))
				{
					throw new InvalidOperationException($"Failed to get a CanvasKit allocation");
				}

				// Console.WriteLine($"Allocated new buffer ({ptr}, {info.Width}x{info.Height})");

				pixels = (IntPtr)ptr;
				_pixelWidth = info.Width;
				_pixelHeight = info.Height;
			}
		}

		private void FreeBitmap()
		{
			if (pixels != IntPtr.Zero)
			{
				WebAssemblyRuntime.InvokeJS($"Module._free({pixels}); true;");
			}
		}
	}
}
#endif
