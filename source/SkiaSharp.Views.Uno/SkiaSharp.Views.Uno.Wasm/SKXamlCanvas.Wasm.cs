using System;
using System.Runtime.InteropServices;
using Uno.Foundation;
using Uno.UI.Runtime.WebAssembly;
#if WINUI
using Microsoft.UI.Xaml;
#else
using Windows.UI.Xaml;
#endif

#if WINDOWS || WINUI
namespace SkiaSharp.Views.Windows
#else
namespace SkiaSharp.Views.UWP
#endif
{
	[HtmlElement("canvas")]
	public partial class SKXamlCanvas
	{
#if HAS_UNO_WINUI
		const string SKXamlCanvasFullTypeName = "SkiaSharp.Views.Windows." + nameof(SKXamlCanvas);
#else
		const string SKXamlCanvasFullTypeName = "SkiaSharp.Views.UWP." + nameof(SKXamlCanvas);
#endif

		private byte[] pixels;
		private GCHandle pixelsHandle;
		private int pixelWidth;
		private int pixelHeight;

		public SKXamlCanvas()
		{
			Initialize();
		}

		partial void DoUnloaded() =>
			FreeBitmap();

		private void DoInvalidate()
		{
			if (designMode)
				return;

			if (!isVisible)
				return;

			if (ActualWidth <= 0 || ActualHeight <= 0)
			{
				CanvasSize = SKSize.Empty;
				return;
			}

			var info = CreateBitmap(out var unscaledSize, out var dpi);

			using (var surface = SKSurface.Create(info, pixelsHandle.AddrOfPinnedObject(), info.RowBytes))
			{
				var userVisibleSize = IgnorePixelScaling ? unscaledSize : info.Size;
				CanvasSize = userVisibleSize;

				if (IgnorePixelScaling)
				{
					var canvas = surface.Canvas;
					canvas.Scale(dpi);
					canvas.Save();
				}

				OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info.WithSize(userVisibleSize), info));
			}

			NativeMethods.InvalidateCanvas(pixelsHandle.AddrOfPinnedObject(), this.GetHtmlId(), info.Width, pixelHeight);
		}

		private SKImageInfo CreateBitmap(out SKSizeI unscaledSize, out float dpi)
		{
			var size = CreateSize(out unscaledSize, out dpi);
			var info = new SKImageInfo(size.Width, size.Height, SKImageInfo.PlatformColorType, SKAlphaType.Unpremul);

			if (pixels == null || pixelWidth != info.Width || pixelHeight != info.Height)
			{
				FreeBitmap();

				pixels = new byte[info.BytesSize];
				pixelsHandle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
				pixelWidth = info.Width;
				pixelHeight = info.Height;
			}

			return info;
		}

		private void FreeBitmap()
		{
			NativeMethods.ClearCanvas(this.GetHtmlId());

			if (pixels != null)
			{
				pixelsHandle.Free();
				pixels = null;
			}
		}

		private static partial class NativeMethods
		{
#if NET7_0_OR_GREATER
			[System.Runtime.InteropServices.JavaScript.JSImport("globalThis." + SKXamlCanvasFullTypeName + ".invalidateCanvas")]
			public static partial void InvalidateCanvas(IntPtr intPtr, string htmlId, int width, int height);
#else
			public static void InvalidateCanvas(IntPtr intPtr, string htmlId, int width, int height)
			{
				WebAssemblyRuntime.InvokeJS(SKXamlCanvasFullTypeName + $".invalidateCanvas({intPtr}, \"{htmlId}\", {width}, {height});");
			}
#endif

#if NET7_0_OR_GREATER
			[System.Runtime.InteropServices.JavaScript.JSImport("globalThis." + SKXamlCanvasFullTypeName + ".clearCanvas")]
			public static partial void ClearCanvas(string htmlId);
#else
			public static void ClearCanvas(string htmlId)
			{
				WebAssemblyRuntime.InvokeJS(SKXamlCanvasFullTypeName + $".clearCanvas(\"{htmlId}\");");
			}
#endif
		}
	}
}
