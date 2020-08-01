﻿using Android.Opengl;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SkiaSharp.Views.UWP
{
	public partial class SKSwapChainPanel : FrameworkElement
	{
		private SKGLTextureView glTextureView;

		public SKSwapChainPanel()
		{
			Initialize();
		}

		private SKSize GetCanvasSize() =>
			glTextureView?.CanvasSize ?? SKSize.Empty;

		private GRContext GetGRContext() =>
			glTextureView?.GRContext;

		partial void DoLoaded()
		{
			glTextureView = new SKGLTextureView(Context);
			DoEnableRenderLoop(EnableRenderLoop);
			glTextureView.PaintSurface += OnPaintSurface;
			AddView(glTextureView);
		}

		partial void DoUnloaded()
		{
			if (glTextureView == null)
				return;

			RemoveView(glTextureView);
			glTextureView.PaintSurface -= OnPaintSurface;
			glTextureView.Dispose();
			glTextureView = null;
		}

		partial void DoEnableRenderLoop(bool enable)
		{
			if (glTextureView == null)
				return;

			glTextureView.RenderMode = enable
				? Rendermode.Continuously
				: Rendermode.WhenDirty;
		}

		private void DoInvalidate() =>
			glTextureView?.RequestRender();

		private void OnPaintSurface(object sender, SKPaintGLSurfaceEventArgs e) =>
			OnPaintSurface(e);
	}
}
