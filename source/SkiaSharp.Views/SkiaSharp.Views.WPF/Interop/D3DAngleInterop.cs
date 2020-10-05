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
        private readonly Control _control;
        private readonly GraphicsContext _context;
        private readonly IAngleWindowInfo _windowInfo;
        private readonly D3D9Interop _d3d9Interop;

        public D3DAngleInterop()
        {
            _d3d9Interop = new D3D9Interop();
            _control = new Control();

			var basicWindowInfo = Utilities.CreateWindowsWindowInfo(_control.Handle);
            _windowInfo = Utilities.CreateAngleWindowInfo(basicWindowInfo);

            _context = new GraphicsContext(
                new GraphicsMode(32, 24), _windowInfo, 2, 0,
                GraphicsContextFlags.Embedded | GraphicsContextFlags.Offscreen | GraphicsContextFlags.AngleD3D9);
            _context.MakeCurrent(_windowInfo);
            _context.LoadAll();
        }

        public void Dispose()
        {
            _context.Dispose();
            _d3d9Interop.Dispose();
        }

        public IntPtr GetD3DSharedHandleForSurface(IntPtr eglSurface, int width, int height)
        {
            var ptr = _windowInfo.QuerySurfacePointer(eglSurface);
            var texture = _d3d9Interop.CreateNewSharedTexture(ptr, width, height);
            return texture.GetSurfaceLevel(0).NativePointer;
        }

        public void EnsureContext() => _context.MakeCurrent(_windowInfo);

        public void MakeCurrent(IntPtr surface) => _windowInfo.MakeCurrent(surface);

        public IntPtr CreateOffscreenSurface(int width, int height) => _windowInfo.CreateSurface(width, height);

        public void DestroyOffscreenSurface(ref IntPtr surface) => _windowInfo.DestroySurface(ref surface);
    }
}
