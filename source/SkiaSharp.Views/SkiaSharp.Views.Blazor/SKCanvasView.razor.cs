using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;
using SkiaSharp.Views.Blazor.Internal;

namespace SkiaSharp.Views.Blazor
{
	/// <summary>A Blazor component that provides a SkiaSharp drawing surface using software rendering.</summary>
	/// <remarks>This component renders to an HTML canvas element using WebAssembly. The drawing surface is CPU-based and renders each frame by transferring pixel data to the browser canvas. For GPU-accelerated rendering, use <see cref="T:SkiaSharp.Views.Blazor.SKGLView" /> instead.</remarks>
	[SupportedOSPlatform("browser")]
	public partial class SKCanvasView : ComponentBase, IDisposable
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

		/// <summary>Initializes a new instance of the <see cref="SKCanvasView" /> class.</summary>
		/// <remarks />
		public SKCanvasView()
		{
		}

		[Inject]
		IJSRuntime JS { get; set; } = null!;

		/// <summary>Gets or sets the callback invoked when the canvas needs to be painted.</summary>
		/// <value>An action that receives <see cref="T:SkiaSharp.Views.Blazor.SKPaintSurfaceEventArgs" /> containing the surface and canvas information.</value>
		/// <remarks>Use this callback to perform your drawing operations. The callback is invoked when <see cref="M:SkiaSharp.Views.Blazor.SKCanvasView.Invalidate" /> is called or continuously when <see cref="P:SkiaSharp.Views.Blazor.SKCanvasView.EnableRenderLoop" /> is <see langword="true" />.</remarks>
		[Parameter]
		public Action<SKPaintSurfaceEventArgs>? OnPaintSurface { get; set; }

		/// <summary>Gets or sets a value indicating whether continuous rendering is enabled.</summary>
		/// <value><see langword="true" /> to render continuously using requestAnimationFrame; <see langword="false" /> to render only when <see cref="M:SkiaSharp.Views.Blazor.SKCanvasView.Invalidate" /> is called.</value>
		/// <remarks>Enable this for animations that need to update every frame. Disable for static content to conserve resources.</remarks>
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

		/// <summary>Gets or sets a value indicating whether the canvas should ignore the device pixel ratio.</summary>
		/// <value><see langword="true" /> to use logical pixels matching the CSS size; <see langword="false" /> to use physical pixels scaled by the DPI.</value>
		/// <remarks>When <see langword="false" /> (the default), the canvas is scaled to match the physical pixel density, resulting in sharper rendering on high-DPI displays. When <see langword="true" />, drawing coordinates match the CSS pixel size.</remarks>
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

		/// <summary>Gets or sets additional HTML attributes to apply to the canvas element.</summary>
		/// <value>A dictionary of attribute names and values, or <see langword="null" />.</value>
		/// <remarks>Use this property to add CSS classes, styles, or other HTML attributes to the underlying canvas element.</remarks>
		[Parameter(CaptureUnmatchedValues = true)]
		public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

		/// <summary>Gets the current device pixel ratio (DPI scaling factor).</summary>
		/// <value>The device pixel ratio, typically 1.0 for standard displays and 2.0 or higher for high-DPI displays.</value>
		/// <remarks>This value is automatically updated when the browser's DPI changes, such as when moving windows between monitors with different scaling.</remarks>
		public double Dpi => dpi;

		/// <summary>Called after the component has rendered.</summary>
		/// <param name="firstRender"><see langword="true" /> if this is the first time the component has been rendered; otherwise, <see langword="false" />.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		/// <remarks>On first render, this method initializes the JavaScript interop for the HTML canvas and sets up DPI and size change watchers.</remarks>
		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (firstRender)
			{
				interop = await SKHtmlCanvasInterop.ImportAsync(JS, htmlCanvas, OnRenderFrame);
				interop.InitRaster();

				sizeWatcher = await SizeWatcherInterop.ImportAsync(JS, htmlCanvas, OnSizeChanged);
				dpiWatcher = await DpiWatcherInterop.ImportAsync(JS, OnDpiChanged);
			}
		}

		/// <summary>Requests a redraw of the canvas.</summary>
		/// <remarks>This method schedules a repaint using the browser's requestAnimationFrame API. The <see cref="P:SkiaSharp.Views.Blazor.SKCanvasView.OnPaintSurface" /> callback will be invoked on the next animation frame.</remarks>
		public void Invalidate()
		{
			if (canvasSize.Width <= 0 || canvasSize.Height <= 0 || dpi <= 0)
				return;

			interop.RequestAnimationFrame(EnableRenderLoop, (int)(canvasSize.Width * dpi), (int)(canvasSize.Height * dpi));
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

			interop.PutImageData(pixelsHandle.AddrOfPinnedObject(), info.Size);
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
			if ((int)(canvasSize.Width * dpi) == newSize.Width && (int)(canvasSize.Height * dpi) == newSize.Height)
				return;
			canvasSize = newSize;

			Invalidate();
		}

		/// <summary>Releases all resources used by this component.</summary>
		/// <remarks>Call this method when the component is no longer needed to free the underlying pixel buffer and JavaScript interop resources.</remarks>
		public void Dispose()
		{
			dpiWatcher?.Unsubscribe(OnDpiChanged);
			sizeWatcher?.Dispose();
			interop?.Dispose();

			FreeBitmap();
		}

		/// <summary>Builds the render tree for the canvas element.</summary>
		/// <param name="__builder">The builder used to construct the render tree.</param>
		/// <remarks>Renders a canvas element, applies <see cref="AdditionalAttributes" />, and captures its element reference.</remarks>
		protected override void BuildRenderTree(RenderTreeBuilder __builder)
		{
			__builder.OpenElement(0, "canvas");
			__builder.AddMultipleAttributes(1, RuntimeHelpers.TypeCheck((IEnumerable<KeyValuePair<string, object>>)AdditionalAttributes!));
			__builder.AddElementReferenceCapture(2, delegate(ElementReference __value)
			{
				htmlCanvas = __value;
			});
			__builder.CloseElement();
		}
	}
}
