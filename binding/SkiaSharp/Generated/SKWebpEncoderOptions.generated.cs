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

		/// <summary>Determines whether this instance is equal to another instance.</summary>
		/// <param name="obj">The <see cref="T:SkiaSharp.SKWebpEncoderOptions" /> to compare with this instance.</param>
		/// <returns><see langword="true" /> if the instances are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (SKWebpEncoderOptions obj) =>
#pragma warning disable CS8909
			fCompression == obj.fCompression && fQuality == obj.fQuality;
#pragma warning restore CS8909

		/// <summary>Determines whether this instance is equal to another object.</summary>
		/// <param name="obj">The object to compare with this instance.</param>
		/// <returns><see langword="true" /> if the object is an <see cref="T:SkiaSharp.SKWebpEncoderOptions" /> and is equal to this instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is SKWebpEncoderOptions f && Equals (f);

		/// <summary>Determines whether two instances are equal.</summary>
		/// <param name="left">The first instance to compare.</param>
		/// <param name="right">The second instance to compare.</param>
		/// <returns><see langword="true" /> if the instances are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (SKWebpEncoderOptions left, SKWebpEncoderOptions right) =>
			left.Equals (right);

		/// <summary>Determines whether two instances are not equal.</summary>
		/// <param name="left">The first instance to compare.</param>
		/// <param name="right">The second instance to compare.</param>
		/// <returns><see langword="true" /> if the instances are not equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (SKWebpEncoderOptions left, SKWebpEncoderOptions right) =>
			!left.Equals (right);

		/// <summary>Returns the hash code for this instance.</summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		/// <remarks />
		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fCompression);
			hash.Add (fQuality);
			return hash.ToHashCode ();
		}

	}
}
