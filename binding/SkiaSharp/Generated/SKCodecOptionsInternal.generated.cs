using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_codec_options_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct SKCodecOptionsInternal : IEquatable<SKCodecOptionsInternal> {
		// public sk_codec_zero_initialized_t fZeroInitialized
		public SKZeroInitialized fZeroInitialized;

		// public sk_irect_t* fSubset
		public SKRectI* fSubset;

		// public int fFrameIndex
		public Int32 fFrameIndex;

		// public int fPriorFrame
		public Int32 fPriorFrame;

		// public size_t fMaxDecodeMemory
		public /* size_t */ IntPtr fMaxDecodeMemory;

		public readonly bool Equals (SKCodecOptionsInternal obj) =>
#pragma warning disable CS8909
			fZeroInitialized == obj.fZeroInitialized && fSubset == obj.fSubset && fFrameIndex == obj.fFrameIndex && fPriorFrame == obj.fPriorFrame && fMaxDecodeMemory == obj.fMaxDecodeMemory;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKCodecOptionsInternal f && Equals (f);

		public static bool operator == (SKCodecOptionsInternal left, SKCodecOptionsInternal right) =>
			left.Equals (right);

		public static bool operator != (SKCodecOptionsInternal left, SKCodecOptionsInternal right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fZeroInitialized);
			hash.Add (fSubset);
			hash.Add (fFrameIndex);
			hash.Add (fPriorFrame);
			hash.Add (fMaxDecodeMemory);
			return hash.ToHashCode ();
		}

	}
}
