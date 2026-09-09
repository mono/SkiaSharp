using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_jpegencoder_options_t
	[StructLayout (LayoutKind.Sequential)]
	public readonly unsafe partial struct SKJpegEncoderOptions : IEquatable<SKJpegEncoderOptions> {
		// public int fQuality
		private readonly Int32 fQuality;

		// public sk_jpegencoder_downsample_t fDownsample
		private readonly SKJpegEncoderDownsample fDownsample;

		// public sk_jpegencoder_alphaoption_t fAlphaOption
		private readonly SKJpegEncoderAlphaOption fAlphaOption;

		// public const sk_data_t* xmpMetadata
		private readonly IntPtr xmpMetadata;

		// public int32_t fOrigin
		private readonly Int32 fOrigin;

		// public bool fHasOrigin
		private readonly Byte fHasOrigin;

		public readonly bool Equals (SKJpegEncoderOptions obj) =>
#pragma warning disable CS8909
			fQuality == obj.fQuality && fDownsample == obj.fDownsample && fAlphaOption == obj.fAlphaOption && xmpMetadata == obj.xmpMetadata && fOrigin == obj.fOrigin && fHasOrigin == obj.fHasOrigin;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKJpegEncoderOptions f && Equals (f);

		public static bool operator == (SKJpegEncoderOptions left, SKJpegEncoderOptions right) =>
			left.Equals (right);

		public static bool operator != (SKJpegEncoderOptions left, SKJpegEncoderOptions right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fQuality);
			hash.Add (fDownsample);
			hash.Add (fAlphaOption);
			hash.Add (xmpMetadata);
			hash.Add (fOrigin);
			hash.Add (fHasOrigin);
			return hash.ToHashCode ();
		}

	}
}
