#if __WASM__
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;
using Uno.Extensions;
using Uno.Foundation;
using Uno.Logging;
using Windows.ApplicationModel;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Uno.UI.Runtime.WebAssembly;

namespace SkiaSharp.Views.UWP
{
	[HtmlElement("canvas")]
	public partial class SKXamlCanvas : FrameworkElement
	{
		private byte[] _pixels;
		private GCHandle _pixelsHandle;
		private int _pixelWidth;
		private int _pixelHeight;

		private static bool designMode = DesignMode.DesignModeEnabled;

		public SKXamlCanvas()
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
				if (this.Log().IsEnabled(Microsoft.Extensions.Logging.LogLevel.Trace))
				{
					this.Log().Trace($"Ignore Invalidate {ActualWidth}x{ActualHeight} isVisible:{isVisible}");
				}

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
			using (var surface = SKSurface.Create(info, _pixelsHandle.AddrOfPinnedObject(), info.RowBytes))
			{
				if (this.Log().IsEnabled(Microsoft.Extensions.Logging.LogLevel.Trace))
				{
					this.Log().Trace($"DoInvalidate: BytesSize={info.BytesSize} ColorType={info.ColorType} IsOpaque:{info.IsOpaque} {info.Width}x{info.Height}");
				}

				OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info));
			}

			WebAssemblyRuntime.InvokeJS($"SkiaSharp.SurfaceManager.invalidateCanvas({_pixelsHandle.AddrOfPinnedObject()}, \"{this.GetHtmlId()}\", {info.Width}, {_pixelHeight});");
		}

		private unsafe void CreateBitmap(SKImageInfo info)
		{
			if (_pixels == null || _pixelWidth != info.Width || _pixelHeight != info.Height)
			{
				FreeBitmap();

				if (this.Log().IsEnabled(Microsoft.Extensions.Logging.LogLevel.Trace))
				{
					this.Log().Trace($"Allocated new buffer ({info.Width}x{info.Height}) was {_pixelWidth}x{_pixelHeight}");
				}

				_pixels = new byte[info.BytesSize];
				_pixelsHandle = GCHandle.Alloc(_pixels, GCHandleType.Pinned);
				_pixelWidth = info.Width;
				_pixelHeight = info.Height;
			}
		}

		private void FreeBitmap()
		{
			if (_pixels != null)
			{
				_pixelsHandle.Free();
				_pixels = null;
			}
		}
	}
}
#endif
