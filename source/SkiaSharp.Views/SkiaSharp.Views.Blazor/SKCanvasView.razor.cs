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
	public partial class SKCanvasView : IDisposable
	{
		private ElementReference htmlCanvas;
		private IRenderer? renderer;
		private bool ignorePixelScaling;
		private bool enableRenderLoop;

		[Inject]
		IJSRuntime JS { get; set; } = null!;

#if NET9_0_OR_GREATER
		[Inject]
		IServiceProvider Services { get; set; } = null!;

		/// <summary>
		/// The frame transfer format to use when this view runs in a bridged host (Blazor Server,
		/// Blazor Hybrid or static SSR). When <see langword="null"/> the global option or a host
		/// default is used. Ignored in Blazor WebAssembly.
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

		public double Dpi => renderer?.Dpi ?? 0;

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (!firstRender)
				return;

#if NET9_0_OR_GREATER
			var hostKind = Host.Resolve(RendererInfo.Name);
			if (Host.IsBridged(hostKind))
			{
				var options = (Services?.GetService(typeof(SKBlazorOptions)) as SKBlazorOptions) ?? SKBlazorOptions.Default;
				renderer = new BridgedRenderer(JS, isGL: false, hostKind, options, PaintFrame);
			}
			else if (OperatingSystem.IsBrowser())
			{
				renderer = new CanvasDirectRenderer(JS, PaintFrame);
			}
#else
			renderer = new CanvasDirectRenderer(JS, PaintFrame);
#endif

			if (renderer == null)
				return;

			SyncRenderer();
			await renderer.InitializeAsync(htmlCanvas);
		}

		public void Invalidate()
		{
			if (renderer == null)
				return;

			SyncRenderer();
			renderer.Invalidate();
		}

		private void SyncRenderer()
		{
			if (renderer == null)
				return;

			renderer.EnableRenderLoop = enableRenderLoop;
			renderer.IgnorePixelScaling = ignorePixelScaling;
#if NET9_0_OR_GREATER
			renderer.TransferFormat = TransferFormat;
			renderer.Quality = Quality;
#endif
		}

		private void PaintFrame(SKSurface surface, SKImageInfo info, SKImageInfo rawInfo, GRBackendRenderTarget? glRenderTarget, GRSurfaceOrigin glOrigin) =>
			OnPaintSurface?.Invoke(new SKPaintSurfaceEventArgs(surface, info, rawInfo));

		public void Dispose()
		{
			if (renderer != null)
			{
				_ = renderer.DisposeAsync();
				renderer = null;
			}
		}
	}
}
