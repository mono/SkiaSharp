using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using SkiaSharp.Views.WPF.Angle;

namespace SkiaSharp.Views.WPF.OutputImage
{
	internal class AngleImage : IOutputImage
	{
		private static readonly Duration LockTimeout = new Duration(TimeSpan.FromMilliseconds(2000));
		private readonly D3DAngleInterop angleInterop;
		private IntPtr d3dSurface;
		private IntPtr eglSurface;
		private GRBackendRenderTarget renderTarget;
		private readonly WaterfallContext context;
		private D3DImage image;
		private Int32Rect rect;
		private bool isBackBufferSet;

		public SizeWithDpi Size { get; private set; }

		public AngleImage(SizeWithDpi size, D3DAngleInterop angleInterop, WaterfallContext context)
		{
			this.angleInterop = angleInterop;
			this.context = context;
			TryResize(size);
		}

		public ImageSource Source => image;

		public bool TryLock()
		{
			if (!isBackBufferSet ||
			    image == null ||
			    !image.IsFrontBufferAvailable)
			{
				return false;
			}

			return image.TryLock(LockTimeout);
		}

		public void Unlock()
		{
			image.AddDirtyRect(rect);
			image.Unlock();
		}

		public SKSurface CreateSurface() => SKSurface.Create(context.GrContext, renderTarget, GRSurfaceOrigin.TopLeft, context.ColorType);

		public void TryResize(SizeWithDpi size)
		{
			if (Size.Equals(size))
			{
				return;
			}

			if (Size.DpiX != size.DpiX ||
			    Size.DpiY != size.DpiY)
			{
				image = new D3DImage(size.DpiX, size.DpiX);
				isBackBufferSet = false;
			}

			Size = size;

			angleInterop.EnsureContext();

			if (eglSurface != IntPtr.Zero)
			{
				angleInterop.DestroyOffscreenSurface(ref eglSurface);
			}

			eglSurface = angleInterop.CreateOffscreenSurface(size.Width, size.Height);
			angleInterop.MakeCurrent(eglSurface);

			d3dSurface = angleInterop.GetD3DSharedHandleForSurface(eglSurface, size.Width, size.Height);
			if (d3dSurface != IntPtr.Zero)
			{
				SetSharedSurfaceToD3DImage();
			}

			UpdateBackendRenderTarget();
		}

		private void UpdateBackendRenderTarget()
		{
			renderTarget?.Dispose();
			var glInfo = new GRGlFramebufferInfo(
				fboId: 0,
				format: context.ColorType.ToGlSizedFormat());
			renderTarget = new GRBackendRenderTarget(
				width: Size.Width,
				height: Size.Height,
				sampleCount: context.SampleCount,
				stencilBits: context.StencilBits,
				glInfo: glInfo);
		}

		private void SetSharedSurfaceToD3DImage()
		{
			if (image == null)
			{
				return;
			}
			if (image.TryLock(LockTimeout))
			{
				image.SetBackBuffer(D3DResourceType.IDirect3DSurface9, d3dSurface, true);
				isBackBufferSet = true;
				rect = new Int32Rect(0, 0, image.PixelWidth, image.PixelHeight);
				image.AddDirtyRect(rect);
			}
			image.Unlock();
		}

		public void MakeCurrent() => angleInterop.MakeCurrent(eglSurface);
	}
}
