using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SkiaSharp.Views.Blazor.Internal;

namespace SkiaSharp.Views.Blazor
{
	public partial class SKCanvasView : IAsyncDisposable
	{
		private SKHtmlCanvasInterop interop = null!;
		private SizeWatcherInterop sizeWatcher = null!;
		private DpiWatcherInterop dpiWatcher = null!;
		private ElementReference htmlCanvas;

		private SKSizeI pixelSize;
		private byte[]? pixels;
		private GCHandle pixelsHandle;
		private bool ignorePixelScaling;
		private double dpi;
		private SKSize canvasSize;
		private bool enableRenderLoop;

		[Inject]
		IJSRuntime JS { get; set; } = null!;

		[Parameter]
		public Action<SKPaintSurfaceEventArgs>? OnPaintSurface { get; set; }

		[Parameter]
		public bool EnableRenderLoop
		{
			get => enableRenderLoop;
			set
			{
				if (enableRenderLoop != value)
				{
					enableRenderLoop = value;
					Invalidate();
				}
			}
		}

		[Parameter]
		public bool IgnorePixelScaling
		{
			get => ignorePixelScaling;
			set
			{
				if (ignorePixelScaling != value)
				{
					ignorePixelScaling = value;
					Invalidate();
				}
			}
		}

		[Parameter(CaptureUnmatchedValues = true)]
		public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (firstRender)
			{
				interop = await SKHtmlCanvasInterop.ImportAsync(JS, htmlCanvas, OnRenderFrame);
				await interop.InitRasterAsync();

				sizeWatcher = await SizeWatcherInterop.ImportAsync(JS, htmlCanvas, OnSizeChanged);
				dpiWatcher = await DpiWatcherInterop.ImportAsync(JS, OnDpiChanged);
			}
		}

		public void Invalidate()
		{
			if (canvasSize.Width <= 0 || canvasSize.Height <= 0 || dpi <= 0)
				return;

			// TODO: fire and forget for now
			_ = interop.RequestAnimationFrame(EnableRenderLoop, (int)(canvasSize.Width * dpi), (int)(canvasSize.Height * dpi));
		}

		private void OnRenderFrame()
		{
			if (canvasSize.Width <= 0 || canvasSize.Height <= 0 || dpi <= 0)
				return;

			var info = CreateBitmap(out var unscaledSize);
			var userVisibleSize = IgnorePixelScaling ? unscaledSize : info.Size;

			using (var surface = SKSurface.Create(info, pixelsHandle.AddrOfPinnedObject(), info.RowBytes))
			{
				if (IgnorePixelScaling)
				{
					var canvas = surface.Canvas;
					canvas.Scale((float)dpi);
					canvas.Save();
				}

				OnPaintSurface?.Invoke(new SKPaintSurfaceEventArgs(surface, info.WithSize(userVisibleSize), info));
			}

			// TODO: fire and forget for now
			_ = interop.PutImageData(pixelsHandle.AddrOfPinnedObject(), info.Size);
		}

		private SKImageInfo CreateBitmap(out SKSizeI unscaledSize)
		{
			var size = CreateSize(out unscaledSize);
			var info = new SKImageInfo(size.Width, size.Height, SKImageInfo.PlatformColorType, SKAlphaType.Opaque);

			if (pixels == null || pixelSize.Width != info.Width || pixelSize.Height != info.Height)
			{
				FreeBitmap();

				pixels = new byte[info.BytesSize];
				pixelsHandle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
				pixelSize = info.Size;
			}

			return info;
		}

		private SKSizeI CreateSize(out SKSizeI unscaledSize)
		{
			unscaledSize = SKSizeI.Empty;

			var w = canvasSize.Width;
			var h = canvasSize.Height;

			if (!IsPositive(w) || !IsPositive(h))
				return SKSizeI.Empty;

			unscaledSize = new SKSizeI((int)w, (int)h);
			return new SKSizeI((int)(w * dpi), (int)(h * dpi));

			static bool IsPositive(double value)
			{
				return !double.IsNaN(value) && !double.IsInfinity(value) && value > 0;
			}
		}

		private void FreeBitmap()
		{
			if (pixels != null)
			{
				pixelsHandle.Free();
				pixels = null;
			}
		}

		private void OnDpiChanged(double newDpi)
		{
			dpi = newDpi;

			Invalidate();
		}

		private void OnSizeChanged(SKSize newSize)
		{
			canvasSize = newSize;

			Invalidate();
		}

		public async ValueTask DisposeAsync()
		{
			await dpiWatcher.UnsubscribeAsync(OnDpiChanged);
			await sizeWatcher.DisposeAsync();
			await interop.DisposeAsync();

			FreeBitmap();
		}
	}
}
