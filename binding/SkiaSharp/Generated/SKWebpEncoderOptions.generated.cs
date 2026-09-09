using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_webpencoder_options_t
	[StructLayout (LayoutKind.Sequential)]
	public readonly unsafe partial struct SKWebpEncoderOptions : IEquatable<SKWebpEncoderOptions> {
		// public sk_webpencoder_compression_t fCompression
		private readonly SKWebpEncoderCompression fCompression;

		// public float fQuality
		private readonly Single fQuality;

		public readonly bool Equals (SKWebpEncoderOptions obj) =>
#pragma warning disable CS8909
			fCompression == obj.fCompression && fQuality == obj.fQuality;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKWebpEncoderOptions f && Equals (f);

		public static bool operator == (SKWebpEncoderOptions left, SKWebpEncoderOptions right) =>
			left.Equals (right);

		public static bool operator != (SKWebpEncoderOptions left, SKWebpEncoderOptions right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fCompression);
			hash.Add (fQuality);
			return hash.ToHashCode ();
		}

	}
}
