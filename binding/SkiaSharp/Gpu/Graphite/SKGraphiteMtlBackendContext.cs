#nullable disable

using System;

namespace SkiaSharp
{
	/// <summary>
	/// Caller-supplied Metal handles used to bring up a Graphite Metal context.
	/// Both <see cref="MtlDevice"/> and <see cref="MtlQueue"/> must be valid
	/// CFTypeRef-compatible Objective-C handles (id&lt;MTLDevice&gt; and
	/// id&lt;MTLCommandQueue&gt;); the resulting <see cref="SKGraphiteContext"/>
	/// retains them for its lifetime and the caller may drop their own references
	/// once <see cref="SKGraphiteContext.CreateMetal"/> returns.
	/// </summary>
	public unsafe class SKGraphiteMtlBackendContext
	{
		/// <summary>id&lt;MTLDevice&gt; (CFRetainable Obj-C handle).</summary>
		public IntPtr MtlDevice { get; set; }

		/// <summary>id&lt;MTLCommandQueue&gt; (CFRetainable Obj-C handle).</summary>
		public IntPtr MtlQueue  { get; set; }

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
	}
}
