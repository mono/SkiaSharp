using System;
using System.Runtime.InteropServices;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace SkiaSharp.Views.UWP
{
	public class SKSwapChainPanel : AngleSwapChainPanel
	{
		private static bool designMode = DesignMode.DesignModeEnabled;

		private GRContext context;
		private GRBackendRenderTargetDesc renderTarget;
		private bool isVisible;

		public SKSwapChainPanel()
		{
			Initialize();

			CompositionScaleChanged += OnCompositionScaleChanged;
		}

		public SKSize CanvasSize => new SKSize(renderTarget.Width, renderTarget.Height);

		protected override Size ArrangeOverride(Size finalSize)
		{
			var arrange = base.ArrangeOverride(finalSize);

			isVisible = Visibility == Visibility.Visible;
			Invalidate();

			return arrange;
		}

		private void OnCompositionScaleChanged(SwapChainPanel sender, object args)
		{
			var info = Windows.Graphics.Display.DisplayInformation.GetForCurrentView();
			var dpi = info.LogicalDpi / 96.0f;

			if (ContentsScale != dpi)
			{
				ContentsScale = dpi;
				RenderTransform = new ScaleTransform
				{
					ScaleX = 1 / ContentsScale,
					ScaleY = 1 / ContentsScale
				};
			}
		}

		private void Initialize()
		{
			if (designMode)
				return;

			//Multisampling = GlesMultisampling.FourTimes; not yet supported
			DepthFormat = GlesDepthFormat.Format24;
			StencilFormat = GlesStencilFormat.Format8;

			Context = new GlesContext();
		}

		public event EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;

		protected virtual void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
		{
			// invoke the event
			PaintSurface?.Invoke(this, e);
		}

		protected override void OnRenderFrame(Rect rect)
		{
			base.OnRenderFrame(rect);

			if (designMode)
				return;

			if (!isVisible)
				return;

			// create the SkiaSharp context
			if (context == null)
			{
				var glInterface = GRGlInterface.CreateNativeAngleInterface();
				context = GRContext.Create(GRBackend.OpenGL, glInterface);

				renderTarget = SKGLDrawable.CreateRenderTarget();
			}

			// set the size
			renderTarget.Width = (int)rect.Width;
			renderTarget.Height = (int)rect.Height;

			// create the surface
			using (var surface = SKSurface.Create(context, renderTarget))
			{
				// draw to the SkiaSharp surface
				OnPaintSurface(new SKPaintGLSurfaceEventArgs(surface, renderTarget));

				// flush the canvas
				surface.Canvas.Flush();
			}

			// flush the SkiaSharp context to the GL context
			context.Flush();
		}
	}
}
