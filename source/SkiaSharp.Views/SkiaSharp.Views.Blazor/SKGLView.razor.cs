using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SkiaSharp.Views.Blazor.Internal;

namespace SkiaSharp.Views.Blazor
{
	/// <summary>A Blazor component that provides a GPU-accelerated SkiaSharp drawing surface using WebGL.</summary>
	/// <remarks>This component renders to an HTML canvas element using WebGL for hardware-accelerated graphics. It provides better performance than <see cref="T:SkiaSharp.Views.Blazor.SKCanvasView" /> for complex scenes and animations, but requires WebGL support in the browser.</remarks>
	[SupportedOSPlatform("browser")]
	public partial class SKGLView : IDisposable
	{
		private SKHtmlCanvasInterop interop = null!;
		private SizeWatcherInterop sizeWatcher = null!;
		private DpiWatcherInterop dpiWatcher = null!;
		private SKHtmlCanvasInterop.GLInfo jsGLInfo = null!;
		private ElementReference htmlCanvas;

		private const int ResourceCacheBytes = 256 * 1024 * 1024; // 256 MB
		private const SKColorType colorType = SKColorType.Rgba8888;
		private const GRSurfaceOrigin surfaceOrigin = GRSurfaceOrigin.BottomLeft;

		private GRContext? context;
		private GRGlInterface? glInterface;
		private GRBackendRenderTarget? renderTarget;
		private SKSize renderTargetSize;
		private SKSurface? surface;
		private SKCanvas? canvas;
		private bool enableRenderLoop;
		private bool ignorePixelScaling;
		private double dpi;
		private SKSize canvasSize;

		[Inject]
		IJSRuntime JS { get; set; } = null!;

		/// <summary>Gets or sets the callback invoked when the canvas needs to be painted.</summary>
		/// <value>An action that receives <see cref="T:SkiaSharp.Views.Blazor.SKPaintGLSurfaceEventArgs" /> containing the surface, canvas, and GPU context information.</value>
		/// <remarks>Use this callback to perform your drawing operations. The callback is invoked when <see cref="M:SkiaSharp.Views.Blazor.SKGLView.Invalidate" /> is called or continuously when <see cref="P:SkiaSharp.Views.Blazor.SKGLView.EnableRenderLoop" /> is <see langword="true" />.</remarks>
		[Parameter]
		public Action<SKPaintGLSurfaceEventArgs>? OnPaintSurface { get; set; }

		/// <summary>Gets or sets a value indicating whether continuous rendering is enabled.</summary>
		/// <value>
		///           <see langword="true" /> to render continuously using requestAnimationFrame; <see langword="false" /> to render only when <see cref="M:SkiaSharp.Views.Blazor.SKGLView.Invalidate" /> is called.</value>
		/// <remarks>Enable this for animations that need to update every frame. Disable for static content to conserve GPU resources and battery.</remarks>
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
		/// <value>
		///           <see langword="true" /> to use logical pixels matching the CSS size; <see langword="false" /> to use physical pixels scaled by the DPI.</value>
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

		/// <param name="firstRender">
		///           <see langword="true" /> if this is the first time the component has been rendered; otherwise, <see langword="false" />.</param>
		/// <summary>Called after the component has rendered.</summary>
		/// <returns>A task representing the asynchronous operation.</returns>
		/// <remarks>On first render, this method initializes the WebGL context, creates the GPU context and surface, and sets up DPI and size change watchers.</remarks>
		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (firstRender)
			{
				interop = await SKHtmlCanvasInterop.ImportAsync(JS, htmlCanvas, OnRenderFrame);
				jsGLInfo = interop.InitGL();

				sizeWatcher = await SizeWatcherInterop.ImportAsync(JS, htmlCanvas, OnSizeChanged);
				dpiWatcher = await DpiWatcherInterop.ImportAsync(JS, OnDpiChanged);
			}
		}

		/// <summary>Requests a redraw of the canvas.</summary>
		/// <remarks>This method schedules a repaint using the browser's requestAnimationFrame API. The <see cref="P:SkiaSharp.Views.Blazor.SKGLView.OnPaintSurface" /> callback will be invoked on the next animation frame.</remarks>
		public void Invalidate()
		{
			if (canvasSize.Width <= 0 || canvasSize.Height <= 0 || dpi <= 0 || jsGLInfo == null)
				return;

			interop.RequestAnimationFrame(EnableRenderLoop, (int)(canvasSize.Width * dpi), (int)(canvasSize.Height * dpi));
		}

		private void OnRenderFrame()
		{
			if (canvasSize.Width <= 0 || canvasSize.Height <= 0 || dpi <= 0 || jsGLInfo == null)
				return;

			// create the SkiaSharp context
			if (context == null)
			{
				glInterface = GRGlInterface.Create();
				context = GRContext.CreateGl(glInterface);

				// bump the default resource cache limit
				context.SetResourceCacheLimit(ResourceCacheBytes);
			}

			// get the new surface size
			var newSize = CreateSize(out var unscaledSize);
			var info = new SKImageInfo(newSize.Width, newSize.Height, colorType);
			var userVisibleSize = IgnorePixelScaling ? unscaledSize : info.Size;

			// manage the drawing surface
			if (renderTarget == null || renderTargetSize != newSize || !renderTarget.IsValid)
			{
				// create or update the dimensions
				renderTargetSize = newSize;

				var glInfo = new GRGlFramebufferInfo(jsGLInfo.FboId, colorType.ToGlSizedFormat());

				// destroy the old surface
				surface?.Dispose();
				surface = null;
				canvas = null;

				// re-create the render target
				renderTarget?.Dispose();
				renderTarget = new GRBackendRenderTarget(newSize.Width, newSize.Height, jsGLInfo.Samples, jsGLInfo.Stencils, glInfo);
			}

			// create the surface
			if (surface == null)
			{
				surface = SKSurface.Create(context, renderTarget, surfaceOrigin, colorType);
				canvas = surface.Canvas;
			}

			using (new SKAutoCanvasRestore(canvas, true))
			{
				if (IgnorePixelScaling)
				{
					var canvas = surface.Canvas;
					canvas.Scale((float)dpi);
					canvas.Save();
				}

				// start drawing
				OnPaintSurface?.Invoke(new SKPaintGLSurfaceEventArgs(surface, renderTarget, surfaceOrigin, info.WithSize(userVisibleSize), info));
			}

			// update the control
			canvas?.Flush();
			context.Flush();
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

		/// <summary>Releases all resources used by this component.</summary>
		/// <remarks>Call this method when the component is no longer needed to free the GPU context, surface, and JavaScript interop resources.</remarks>
		public void Dispose()
		{
			dpiWatcher.Unsubscribe(OnDpiChanged);
			sizeWatcher.Dispose();
			interop.Dispose();
		}
	}
}
