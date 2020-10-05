using System;
using SharpDX.Direct3D9;

namespace SkiaSharp.Views.WPF.Angle
{
	internal class D3D9Interop
	{
		private bool _disposed;
		private Texture? _texture;
		private readonly Direct3DEx _d3dEx;
		private readonly DeviceEx _deviceEx;

		public D3D9Interop()
		{
			_d3dEx = new Direct3DEx();
			_deviceEx = MakeDevice();
		}

		/// <summary>
		///     Creates a new Direct3D9 Ex device, required for efficient
		///     hardware-accelerated in Windows Vista and later.
		/// </summary>
		/// <returns></returns>
		private DeviceEx MakeDevice() =>
			new DeviceEx(_d3dEx, 0,
				DeviceType.Hardware,
				IntPtr.Zero,
				CreateFlags.HardwareVertexProcessing
				| CreateFlags.Multithreaded
				| CreateFlags.FpuPreserve,
				new PresentParameters
				{
					Windowed = true,
					SwapEffect = SwapEffect.Discard,
					PresentationInterval = PresentInterval.Immediate,

					// The device back buffer is not used.
					BackBufferFormat = Format.Unknown,
					BackBufferWidth = 1,
					BackBufferHeight = 1,

					// Use dummy window handle.
					DeviceWindowHandle = IntPtr.Zero
				});

		/// <summary>
		///     Creates a new Direct3D9 texture that uses the same memory as the
		///     passed in DirectX11-texture.
		/// </summary>
		public Texture CreateNewSharedTexture(IntPtr sharedHandle, int width, int height)
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(GetType().FullName);
			}
			if (sharedHandle == IntPtr.Zero)
			{
				throw new ArgumentException(
					"Unable to access resource. The texture needs to be created as a shared resource.", "render_target");
			}

			_texture?.Dispose();
			_texture = new Texture(_deviceEx,
				width,
				height,
				1, Usage.RenderTarget, Format.A8R8G8B8,
				Pool.Default, ref sharedHandle);
			return _texture;
		}

		~D3D9Interop()
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
			if (_disposed)
				return;

			if (disposeManaged)
			{
				_texture?.Dispose();
				_deviceEx?.Dispose();
				_d3dEx?.Dispose();
			}
			_disposed = true;
		}
	}
}
