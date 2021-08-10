using System;
using System.Windows.Forms;
using OpenTK.Graphics;
using OpenTK.Platform;
using OpenTK.Platform.Egl;

namespace SkiaSharp.Views.WPF.Angle
{
	/// <summary>
	/// GLES 2.0 and ANGLE context initialization over DirectX 9
	/// </summary>
    internal class D3DAngleInterop : IDisposable
    {
        private readonly Control control;
        private readonly GraphicsContext context;
        private readonly IAngleWindowInfo windowInfo;
        private readonly D3D9Interop d3d9Interop;
        private bool disposed;

		public D3DAngleInterop()
        {
            d3d9Interop = new D3D9Interop();
            control = new Control();

			var basicWindowInfo = Utilities.CreateWindowsWindowInfo(control.Handle);
            windowInfo = Utilities.CreateAngleWindowInfo(basicWindowInfo);

            context = new GraphicsContext(
                new GraphicsMode(32, 24), windowInfo, 2, 0,
                GraphicsContextFlags.Embedded | GraphicsContextFlags.Offscreen | GraphicsContextFlags.AngleD3D9);
            context.MakeCurrent(windowInfo);
            context.LoadAll();
        }

        public IntPtr GetD3DSharedHandleForSurface(IntPtr eglSurface, int width, int height)
        {
            var ptr = windowInfo.QuerySurfacePointer(eglSurface);
            var texture = d3d9Interop.CreateNewSharedTexture(ptr, width, height);
            return texture.GetSurfaceLevel(0).NativePointer;
        }

        public void EnsureContext() => context.MakeCurrent(windowInfo);

        public void MakeCurrent(IntPtr surface) => windowInfo.MakeCurrent(surface);

        public IntPtr CreateOffscreenSurface(int width, int height) => windowInfo.CreateSurface(width, height);

        public void DestroyOffscreenSurface(ref IntPtr surface) => windowInfo.DestroySurface(ref surface);

        ~D3DAngleInterop()
        {
	        Dispose(false);
        }

        public void Dispose()
        {
	        Dispose(true);
	        GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposeManaged)
        {
	        if (disposed)
		        return;

	        if (disposeManaged)
			{
				context.Dispose();
				d3d9Interop.Dispose();
			}
	        disposed = true;
        }
	}
}
