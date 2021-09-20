using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SkiaSharp.Views.Blazor.Internal;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SkiaSharp.Views.Blazor
{
	public partial class SKGLView : IAsyncDisposable
	{
		private SKGLViewInterop interop = null!;
		private SizeWatcherInterop sizeWatcher = null!;
		private DpiWatcherInterop dpiWatcher = null!;
		private SKGLViewInterop.Info jsInfo = null!;
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
		private double dpi;
		private SKSize canvasSize;

		[Inject]
		IJSRuntime JS { get; set; } = null!;

		[Parameter]
		public EventCallback<SKPaintGLSurfaceEventArgs> OnPaintSurface { get; set; }

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

		[Parameter(CaptureUnmatchedValues = true)]
		public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (firstRender)
			{
				interop = new SKGLViewInterop(JS, htmlCanvas, OnRenderFrame);
				jsInfo = await interop.InitAsync();

				sizeWatcher = new SizeWatcherInterop(JS, htmlCanvas, OnSizeChanged);
				await sizeWatcher.StartAsync();

				dpiWatcher = DpiWatcherInterop.Get(JS);
				await dpiWatcher.SubscribeAsync(OnDpiChanged);
			}
		}

		public async void Invalidate()
		{
			await InvalidateAsync();
		}

		public async Task InvalidateAsync()
		{
			if (canvasSize.Width <= 0 || canvasSize.Height <= 0 || dpi <= 0 || jsInfo == null)
				return;

			await interop.RequestAnimationFrameAsync(EnableRenderLoop, (int)(canvasSize.Width * dpi), (int)(canvasSize.Height * dpi));
		}

		private void OnRenderFrame()
		{
			if (canvasSize.Width <= 0 || canvasSize.Height <= 0 || dpi <= 0 || jsInfo == null)
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
			var newSize = CreateSize();

			// manage the drawing surface
			if (renderTarget == null || renderTargetSize != newSize || !renderTarget.IsValid)
			{
				// create or update the dimensions
				renderTargetSize = newSize;

				var glInfo = new GRGlFramebufferInfo(jsInfo.FboId, colorType.ToGlSizedFormat());

				// destroy the old surface
				surface?.Dispose();
				surface = null;
				canvas = null;

				// re-create the render target
				renderTarget?.Dispose();
				renderTarget = new GRBackendRenderTarget(newSize.Width, newSize.Height, jsInfo.Samples, jsInfo.Stencils, glInfo);
			}

			// create the surface
			if (surface == null)
			{
				surface = SKSurface.Create(context, renderTarget, surfaceOrigin, colorType);
				canvas = surface.Canvas;
			}

			using (new SKAutoCanvasRestore(canvas, true))
			{
				// start drawing
				OnPaintSurface.InvokeAsync(new SKPaintGLSurfaceEventArgs(surface, renderTarget, surfaceOrigin, colorType));
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

		private SKSizeI CreateSize()
		{
			var w = canvasSize.Width;
			var h = canvasSize.Height;

			if (!IsPositive(w) || !IsPositive(h))
				return SKSizeI.Empty;

			return new SKSizeI((int)(w * dpi), (int)(h * dpi));

			static bool IsPositive(double value)
			{
				return !double.IsNaN(value) && !double.IsInfinity(value) && value > 0;
			}
		}

		public async ValueTask DisposeAsync()
		{
			await dpiWatcher.UnsubscribeAsync(OnDpiChanged);
			await sizeWatcher.DisposeAsync();
			await interop.DisposeAsync();
		}
	}
}
