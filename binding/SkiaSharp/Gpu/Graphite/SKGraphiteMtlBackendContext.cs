#nullable disable

using System;
#if __IOS__ || __MACOS__ || __TVOS__
using Metal;
#endif

namespace SkiaSharp
{
	/// <summary>Supplies the Metal device and command queue used to create a Metal-backed <see cref="T:SkiaSharp.SKGraphiteContext" />.</summary>
	/// <remarks><format type="text/markdown"><![CDATA[
	/// ## Remarks
	///
	/// Populate the <xref:SkiaSharp.SKGraphiteMtlBackendContext.MtlDevice> and <xref:SkiaSharp.SKGraphiteMtlBackendContext.MtlQueue> handles, then pass this object to <xref:SkiaSharp.SKGraphiteContext.CreateMetal(SkiaSharp.SKGraphiteMtlBackendContext)>. You can dispose it as soon as the context has been created.
	///
	/// On Apple platforms you can also assign the strongly typed `Device` and `Queue` properties instead of raw handles.
	/// ]]></format></remarks>
	public unsafe class SKGraphiteMtlBackendContext : IDisposable
	{
		private IntPtr mtlDevice;
		private IntPtr mtlQueue;

		/// <summary>Initializes a new instance of the <see cref="SKGraphiteMtlBackendContext" /> class.</summary>
		/// <remarks />
		public SKGraphiteMtlBackendContext ()
		{
		}

		/// <summary>Gets or sets the handle to the Metal device.</summary>
		/// <value>A handle to the Metal device.</value>
		/// <remarks />
		public IntPtr MtlDevice {
			get => mtlDevice;
			set {
				mtlDevice = value;
#if __IOS__ || __MACOS__ || __TVOS__
				device = null;
#endif
			}
		}

		/// <summary>Gets or sets the handle to the Metal command queue.</summary>
		/// <value>A handle to the Metal command queue.</value>
		/// <remarks />
		public IntPtr MtlQueue {
			get => mtlQueue;
			set {
				mtlQueue = value;
#if __IOS__ || __MACOS__ || __TVOS__
				queue = null;
#endif
			}
		}

#if __IOS__ || __MACOS__ || __TVOS__
		private IMTLDevice device;
		private IMTLCommandQueue queue;

		/// <summary>Gets or sets the Metal device.</summary>
		/// <value>The Metal device, or <see langword="null" />.</value>
		/// <remarks />
		public IMTLDevice Device {
			get => device;
			set {
				device = value;
				mtlDevice = value?.Handle ?? IntPtr.Zero;
			}
		}

		/// <summary>Gets or sets the Metal command queue.</summary>
		/// <value>The Metal command queue, or <see langword="null" />.</value>
		/// <remarks />
		public IMTLCommandQueue Queue {
			get => queue;
			set {
				queue = value;
				mtlQueue = value?.Handle ?? IntPtr.Zero;
			}
		}
#endif

		internal SKGraphiteMtlBackendContextInit ToNative ()
		{
			if (MtlDevice == IntPtr.Zero)
				throw new InvalidOperationException ($"{nameof (MtlDevice)} must be set before materializing the backend context.");
			if (MtlQueue == IntPtr.Zero)
				throw new InvalidOperationException ($"{nameof (MtlQueue)} must be set before materializing the backend context.");
			return new SKGraphiteMtlBackendContextInit {
				Device = (void*)MtlDevice,
				Queue  = (void*)MtlQueue,
			};
		}

		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.SKGraphiteMtlBackendContext" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks />
		protected virtual void Dispose (bool disposing)
		{
		}

		/// <summary>Releases the resources used by the current instance of the <see cref="T:SkiaSharp.SKGraphiteMtlBackendContext" /> class.</summary>
		/// <remarks />
		public void Dispose ()
		{
			Dispose (disposing: true);
			GC.SuppressFinalize (this);
		}
	}
}
