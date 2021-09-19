using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SkiaSharp.Views.Blazor.Internal;
using System;
using System.Threading.Tasks;

namespace SkiaSharp.Views.Blazor
{
	public partial class SKGLView : IAsyncDisposable
	{
		private SKGLViewInterop interop = null!;
		private SKGLViewInterop.Info? jsInfo;

		private ElementReference htmlCanvas;

		private const int ResourceCacheBytes = 256 * 1024 * 1024; // 256 MB
		private const SKColorType colorType = SKColorType.Rgba8888;
		private const GRSurfaceOrigin surfaceOrigin = GRSurfaceOrigin.BottomLeft;

		private GRContext? context;
		private GRGlInterface? glInterface;
		private GRBackendRenderTarget? renderTarget;
		private SKSurface? surface;
		private SKCanvas? canvas;
		private double dpi;
		private bool enableRenderLoop;

		[Inject]
		IJSRuntime JS { get; set; } = null!;

		[Parameter]
		public double Width { get; set; }

		[Parameter]
		public double Height { get; set; }

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

		public GRContext? GRContext => context;

		public SKSize CanvasSize { get; private set; }

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (firstRender)
			{
				interop = new SKGLViewInterop(JS, RenderFrame);
				jsInfo = await interop.InitAsync(htmlCanvas);

				DpiWatcherInterop.Init(JS);
				DpiWatcherInterop.DpiChanged += OnDpiChanged;

				OnDpiChanged(await DpiWatcherInterop.GetDpiAsync());
			}
		}

		protected virtual Task InvokeOnPaintSurfaceAsync(SKPaintGLSurfaceEventArgs e)
		{
			return OnPaintSurface.InvokeAsync(e);
		}

		public async void Invalidate()
		{
			await InvalidateAsync();
		}

		public async Task InvalidateAsync()
		{
			if (Width <= 0 || Height <= 0 || dpi <= 0 || jsInfo == null)
			{
				CanvasSize = SKSize.Empty;
				return;
			}

			await interop.RequestAnimationFrameAsync(htmlCanvas, EnableRenderLoop, (int)(Width * dpi), (int)(Height * dpi));
		}

		private void RenderFrame()
		{
			if (Width <= 0 || Height <= 0 || dpi <= 0 || jsInfo == null)
			{
				CanvasSize = SKSize.Empty;
				return;
			}

			// create the SkiaSharp context
			if (context == null)
			{
				glInterface = GRGlInterface.Create();
				context = GRContext.CreateGl(glInterface);

				// bump the default resource cache limit
				context.SetResourceCacheLimit(ResourceCacheBytes);
			}

			// get the new surface size
			var newSize = CreateSize(out _);

			// manage the drawing surface
			if (renderTarget == null || CanvasSize != newSize || !renderTarget.IsValid)
			{
				// create or update the dimensions
				CanvasSize = newSize;

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
				InvokeOnPaintSurfaceAsync(new SKPaintGLSurfaceEventArgs(surface, renderTarget, surfaceOrigin, colorType));
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

		private SKSizeI CreateSize(out SKSizeI unscaledSize)
		{
			unscaledSize = SKSizeI.Empty;

			var w = Width;
			var h = Height;

			if (!IsPositive(w) || !IsPositive(h))
				return SKSizeI.Empty;

			unscaledSize = new SKSizeI((int)w, (int)h);
			return new SKSizeI((int)(w * dpi), (int)(h * dpi));

			static bool IsPositive(double value)
			{
				return !double.IsNaN(value) && !double.IsInfinity(value) && value > 0;
			}
		}

		public async ValueTask DisposeAsync()
		{
			DpiWatcherInterop.DpiChanged -= OnDpiChanged;

			await interop.DisposeAsync();
		}
	}
}