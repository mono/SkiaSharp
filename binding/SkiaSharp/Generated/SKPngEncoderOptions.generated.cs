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

		/// <summary>Determines whether this instance is equal to another instance.</summary>
		/// <param name="obj">The <see cref="T:SkiaSharp.SKPngEncoderOptions" /> to compare with this instance.</param>
		/// <returns><see langword="true" /> if the instances are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (SKPngEncoderOptions obj) =>
#pragma warning disable CS8909
			fFilterFlags == obj.fFilterFlags && fZLibLevel == obj.fZLibLevel && fComments == obj.fComments && fGainmap == obj.fGainmap && fGainmapInfo == obj.fGainmapInfo;
#pragma warning restore CS8909

		/// <summary>Determines whether this instance is equal to another object.</summary>
		/// <param name="obj">The object to compare with this instance.</param>
		/// <returns><see langword="true" /> if the object is an <see cref="T:SkiaSharp.SKPngEncoderOptions" /> and is equal to this instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is SKPngEncoderOptions f && Equals (f);

		/// <summary>Determines whether two instances are equal.</summary>
		/// <param name="left">The first instance to compare.</param>
		/// <param name="right">The second instance to compare.</param>
		/// <returns><see langword="true" /> if the instances are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (SKPngEncoderOptions left, SKPngEncoderOptions right) =>
			left.Equals (right);

		/// <summary>Determines whether two instances are not equal.</summary>
		/// <param name="left">The first instance to compare.</param>
		/// <param name="right">The second instance to compare.</param>
		/// <returns><see langword="true" /> if the instances are not equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (SKPngEncoderOptions left, SKPngEncoderOptions right) =>
			!left.Equals (right);

		/// <summary>Returns the hash code for this instance.</summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		/// <remarks />
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
