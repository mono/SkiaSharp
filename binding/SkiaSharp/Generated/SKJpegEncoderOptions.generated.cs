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
		private readonly sk_data_t xmpMetadata;

		// public int32_t fOrigin
		private readonly Int32 fOrigin;

		// public bool fHasOrigin
		private readonly Byte fHasOrigin;

		/// <summary>Determines whether the specified <see cref="T:SkiaSharp.SKJpegEncoderOptions" /> is equal to the current instance.</summary>
		/// <param name="obj">The <see cref="T:SkiaSharp.SKJpegEncoderOptions" /> to compare with the current instance.</param>
		/// <returns><see langword="true" /> if the specified object is equal to the current instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (SKJpegEncoderOptions obj) =>
#pragma warning disable CS8909
			fQuality == obj.fQuality && fDownsample == obj.fDownsample && fAlphaOption == obj.fAlphaOption && xmpMetadata == obj.xmpMetadata && fOrigin == obj.fOrigin && fHasOrigin == obj.fHasOrigin;
#pragma warning restore CS8909

		/// <summary>Determines whether the specified object is equal to the current instance.</summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		/// <returns><see langword="true" /> if the specified object is equal to the current instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is SKJpegEncoderOptions f && Equals (f);

		/// <summary>Determines whether two specified <see cref="T:SkiaSharp.SKJpegEncoderOptions" /> objects have the same value.</summary>
		/// <param name="left">The first <see cref="T:SkiaSharp.SKJpegEncoderOptions" /> to compare.</param>
		/// <param name="right">The second <see cref="T:SkiaSharp.SKJpegEncoderOptions" /> to compare.</param>
		/// <returns><see langword="true" /> if the value of <paramref name="left" /> is the same as the value of <paramref name="right" />; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (SKJpegEncoderOptions left, SKJpegEncoderOptions right) =>
			left.Equals (right);

		/// <summary>Determines whether two specified <see cref="T:SkiaSharp.SKJpegEncoderOptions" /> objects have different values.</summary>
		/// <param name="left">The first <see cref="T:SkiaSharp.SKJpegEncoderOptions" /> to compare.</param>
		/// <param name="right">The second <see cref="T:SkiaSharp.SKJpegEncoderOptions" /> to compare.</param>
		/// <returns><see langword="true" /> if the value of <paramref name="left" /> is different from the value of <paramref name="right" />; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (SKJpegEncoderOptions left, SKJpegEncoderOptions right) =>
			!left.Equals (right);

		/// <summary>Returns the hash code for this instance.</summary>
		/// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
		/// <remarks />
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
