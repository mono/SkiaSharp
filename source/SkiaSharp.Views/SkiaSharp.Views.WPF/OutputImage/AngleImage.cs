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
		private readonly D3DAngleInterop _angleInterop;
		private IntPtr _d3dSurface;
		private IntPtr _eglSurface;
		private GRBackendRenderTarget _renderTarget;
		private WaterfallContext _context;
		private D3DImage _image;
		private Int32Rect _rect;
		private bool _isBackBufferSet;

		public SizeWithDpi Size { get; private set; }

		public AngleImage(SizeWithDpi size, D3DAngleInterop angleInterop, WaterfallContext context)
		{
			_angleInterop = angleInterop;
			_context = context;
			TryResize(size);
		}

		public ImageSource Source => _image;

		public bool TryLock()
		{
			if (!_isBackBufferSet ||
			    _image == null ||
			    !_image.IsFrontBufferAvailable)
			{
				return false;
			}

			return _image.TryLock(LockTimeout);
		}

		public void Unlock()
		{
			_image.AddDirtyRect(_rect);
			_image.Unlock();
		}

		public SKSurface CreateSurface(WaterfallContext context)
		{
			return SKSurface.Create(context.GrContext, _renderTarget, GRSurfaceOrigin.TopLeft, context.ColorType);
		}

		public void TryResize(SizeWithDpi size)
		{
			if (Size.Equals(size))
			{
				return;
			}

			if (Size.DpiX != size.DpiX ||
			    Size.DpiY != size.DpiY)
			{
				_image = new D3DImage(size.DpiX, size.DpiX);
				_isBackBufferSet = false;
			}

			Size = size;

			_angleInterop.EnsureContext();

			if (_eglSurface != IntPtr.Zero)
			{
				_angleInterop.DestroyOffscreenSurface(ref _eglSurface);
			}

			_eglSurface = _angleInterop.CreateOffscreenSurface(size.Width, size.Height);
			_angleInterop.MakeCurrent(_eglSurface);

			_d3dSurface = _angleInterop.GetD3DSharedHandleForSurface(_eglSurface, size.Width, size.Height);
			if (_d3dSurface != IntPtr.Zero)
			{
				SetSharedSurfaceToD3DImage();
			}


			var glInfo = new GRGlFramebufferInfo(
				fboId: 0,
				format: _context.ColorType.ToGlSizedFormat());
			_renderTarget = new GRBackendRenderTarget(
				width: Size.Width,
				height: Size.Height,
				sampleCount: _context.SampleCount,
				stencilBits: _context.StencilBits,
				glInfo: glInfo);
		}

		private void SetSharedSurfaceToD3DImage()
		{
			if (_image == null)
			{
				return;
			}
			if (_image.TryLock(LockTimeout))
			{
				_image.SetBackBuffer(D3DResourceType.IDirect3DSurface9, _d3dSurface, true);
				_isBackBufferSet = true;
				_rect = new Int32Rect(0, 0, _image.PixelWidth, _image.PixelHeight);
				_image.AddDirtyRect(_rect);
			}
			_image.Unlock();
		}

		public void MakeCurrent() => _angleInterop.MakeCurrent(_eglSurface);
	}
}
