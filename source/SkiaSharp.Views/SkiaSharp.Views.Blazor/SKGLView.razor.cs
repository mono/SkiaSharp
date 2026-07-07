using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SkiaSharp.Views.Blazor.Internal;

namespace SkiaSharp.Views.Blazor
{
#if !NET9_0_OR_GREATER
	[SupportedOSPlatform("browser")]
#endif
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

#if NET9_0_OR_GREATER
		[Inject]
		IServiceProvider Services { get; set; } = null!;

		private SKBlazorHostKind hostKind;
		private bool isBridged;
		private SKBlazorBridgedRenderer? renderer;

		/// <summary>
		/// The frame transfer format to use when this view runs in a bridged host (Blazor Server,
		/// Blazor Hybrid or static SSR). When <see langword="null"/> the global option or a host
		/// default is used. Ignored in Blazor WebAssembly. In a bridged host drawing is CPU raster
		/// and the frame is presented into a WebGL-backed canvas.
		/// </summary>
		[Parameter]
		public SKBlazorTransferFormat? TransferFormat { get; set; }

		/// <summary>
		/// The JPEG quality (0-100) used when the resolved <see cref="TransferFormat"/> is
		/// <see cref="SKBlazorTransferFormat.Jpeg"/>. When <see langword="null"/> the global option
		/// value is used. Ignored in Blazor WebAssembly.
		/// </summary>
		[Parameter]
		public int? Quality { get; set; }
#endif

		[Parameter]
		public Action<SKPaintGLSurfaceEventArgs>? OnPaintSurface { get; set; }

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

		public double Dpi
		{
			get
			{
#if NET9_0_OR_GREATER
				if (isBridged && renderer != null)
					return renderer.Dpi;
#endif
				return dpi;
			}
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (!firstRender)
				return;

#if NET9_0_OR_GREATER
			hostKind = SKBlazorHost.Resolve(RendererInfo.Name);
			isBridged = SKBlazorHost.IsBridged(hostKind);
			if (isBridged)
			{
				var options = (Services?.GetService(typeof(SKBlazorOptions)) as SKBlazorOptions) ?? SKBlazorOptions.Default;
				renderer = new SKBlazorBridgedRenderer(JS, isGL: true, hostKind, options, PaintBridgedFrame)
				{
					EnableRenderLoop = enableRenderLoop,
					IgnorePixelScaling = ignorePixelScaling,
					TransferFormat = TransferFormat,
					Quality = Quality,
				};
				await renderer.InitializeAsync(htmlCanvas);
				return;
			}
#endif

			if (OperatingSystem.IsBrowser())
			{
				interop = await SKHtmlCanvasInterop.ImportAsync(JS, htmlCanvas, OnRenderFrame);
				jsGLInfo = interop.InitGL();

				sizeWatcher = await SizeWatcherInterop.ImportAsync(JS, htmlCanvas, OnSizeChanged);
				dpiWatcher = await DpiWatcherInterop.ImportAsync(JS, OnDpiChanged);
			}
		}

		public void Invalidate()
		{
#if NET9_0_OR_GREATER
			if (isBridged)
			{
				if (renderer != null)
				{
					renderer.EnableRenderLoop = enableRenderLoop;
					renderer.IgnorePixelScaling = ignorePixelScaling;
					renderer.TransferFormat = TransferFormat;
					renderer.Quality = Quality;
					renderer.Invalidate();
				}
				return;
			}
#endif

			if (!OperatingSystem.IsBrowser())
				return;

			if (canvasSize.Width <= 0 || canvasSize.Height <= 0 || dpi <= 0 || jsGLInfo == null)
				return;

			interop.RequestAnimationFrame(EnableRenderLoop, (int)(canvasSize.Width * dpi), (int)(canvasSize.Height * dpi));
		}

#if NET9_0_OR_GREATER
		private void PaintBridgedFrame(SKSurface surface, SKImageInfo userInfo, SKImageInfo rawInfo)
		{
			// Server-side drawing is CPU raster; present into the WebGL-backed canvas. The render
			// target is a lightweight placeholder so the GL event args shape is preserved.
			using var placeholder = new GRBackendRenderTarget(
				rawInfo.Width,
				rawInfo.Height,
				0,
				0,
				new GRGlFramebufferInfo(0, rawInfo.ColorType.ToGlSizedFormat()));

			OnPaintSurface?.Invoke(new SKPaintGLSurfaceEventArgs(surface, placeholder, GRSurfaceOrigin.TopLeft, userInfo, rawInfo));
		}
#endif

		[SupportedOSPlatform("browser")]
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

		public void Dispose()
		{
#if NET9_0_OR_GREATER
			if (renderer != null)
			{
				_ = renderer.DisposeAsync();
				renderer = null;
			}
#endif

			if (OperatingSystem.IsBrowser())
			{
				dpiWatcher?.Unsubscribe(OnDpiChanged);
				sizeWatcher?.Dispose();
				interop?.Dispose();
			}
		}
	}
}
