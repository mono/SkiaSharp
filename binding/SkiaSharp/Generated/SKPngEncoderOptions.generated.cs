using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_pngencoder_options_t
	[StructLayout (LayoutKind.Sequential)]
	public readonly unsafe partial struct SKPngEncoderOptions : IEquatable<SKPngEncoderOptions> {
		// public sk_pngencoder_filterflags_t fFilterFlags
		private readonly SKPngEncoderFilterFlags fFilterFlags;

		// public int fZLibLevel
		private readonly Int32 fZLibLevel;

		// public void* fComments
		private readonly void* fComments;

		// public const void* fGainmap
		private readonly void* fGainmap;

		// public const void* fGainmapInfo
		private readonly void* fGainmapInfo;

		public readonly bool Equals (SKPngEncoderOptions obj) =>
#pragma warning disable CS8909
			fFilterFlags == obj.fFilterFlags && fZLibLevel == obj.fZLibLevel && fComments == obj.fComments && fGainmap == obj.fGainmap && fGainmapInfo == obj.fGainmapInfo;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKPngEncoderOptions f && Equals (f);

		public static bool operator == (SKPngEncoderOptions left, SKPngEncoderOptions right) =>
			left.Equals (right);

		public static bool operator != (SKPngEncoderOptions left, SKPngEncoderOptions right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fFilterFlags);
			hash.Add (fZLibLevel);
			hash.Add (fComments);
			hash.Add (fGainmap);
			hash.Add (fGainmapInfo);
			return hash.ToHashCode ();
		}

	}
}
