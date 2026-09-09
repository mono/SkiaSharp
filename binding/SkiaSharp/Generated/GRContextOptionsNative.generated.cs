using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// gr_context_options_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct GRContextOptionsNative : IEquatable<GRContextOptionsNative> {
		// public bool fAvoidStencilBuffers
		public Byte fAvoidStencilBuffers;

		// public int fRuntimeProgramCacheSize
		public Int32 fRuntimeProgramCacheSize;

		// public size_t fGlyphCacheTextureMaximumBytes
		public /* size_t */ IntPtr fGlyphCacheTextureMaximumBytes;

		// public bool fAllowPathMaskCaching
		public Byte fAllowPathMaskCaching;

		// public bool fDoManualMipmapping
		public Byte fDoManualMipmapping;

		// public int fBufferMapThreshold
		public Int32 fBufferMapThreshold;

		public readonly bool Equals (GRContextOptionsNative obj) =>
#pragma warning disable CS8909
			fAvoidStencilBuffers == obj.fAvoidStencilBuffers && fRuntimeProgramCacheSize == obj.fRuntimeProgramCacheSize && fGlyphCacheTextureMaximumBytes == obj.fGlyphCacheTextureMaximumBytes && fAllowPathMaskCaching == obj.fAllowPathMaskCaching && fDoManualMipmapping == obj.fDoManualMipmapping && fBufferMapThreshold == obj.fBufferMapThreshold;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is GRContextOptionsNative f && Equals (f);

		public static bool operator == (GRContextOptionsNative left, GRContextOptionsNative right) =>
			left.Equals (right);

		public static bool operator != (GRContextOptionsNative left, GRContextOptionsNative right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fAvoidStencilBuffers);
			hash.Add (fRuntimeProgramCacheSize);
			hash.Add (fGlyphCacheTextureMaximumBytes);
			hash.Add (fAllowPathMaskCaching);
			hash.Add (fDoManualMipmapping);
			hash.Add (fBufferMapThreshold);
			return hash.ToHashCode ();
		}

	}
}
