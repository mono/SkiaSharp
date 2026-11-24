using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_sampling_options_t
	[StructLayout (LayoutKind.Sequential)]
	public readonly unsafe partial struct SKSamplingOptions : IEquatable<SKSamplingOptions> {
		// public int fMaxAniso
		private readonly Int32 fMaxAniso;
		/// <summary>Gets the maximum anisotropic filtering level.</summary>
		/// <value>The maximum anisotropic filtering level, or 0 if anisotropic filtering is disabled.</value>
		/// <remarks />
		public readonly Int32 MaxAniso => fMaxAniso;

		// public bool fUseCubic
		private readonly Byte fUseCubic;
		/// <summary>Gets a value indicating whether cubic resampling is enabled.</summary>
		/// <value><see langword="true" /> if cubic resampling is enabled; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public readonly bool UseCubic => fUseCubic > 0;

		// public sk_cubic_resampler_t fCubic
		private readonly SKCubicResampler fCubic;
		/// <summary>Gets the cubic resampler parameters.</summary>
		/// <value>The cubic resampler used for image sampling.</value>
		/// <remarks />
		public readonly SKCubicResampler Cubic => fCubic;

		// public sk_filter_mode_t fFilter
		private readonly SKFilterMode fFilter;
		/// <summary>Gets the filter mode.</summary>
		/// <value>The filter mode used for image sampling.</value>
		/// <remarks />
		public readonly SKFilterMode Filter => fFilter;

		// public sk_mipmap_mode_t fMipmap
		private readonly SKMipmapMode fMipmap;
		/// <summary>Gets the mipmap mode.</summary>
		/// <value>The mipmap mode used for image sampling.</value>
		/// <remarks />
		public readonly SKMipmapMode Mipmap => fMipmap;

		/// <summary>Determines whether the specified <see cref="T:SkiaSharp.SKSamplingOptions" /> is equal to the current instance.</summary>
		/// <param name="obj">The <see cref="T:SkiaSharp.SKSamplingOptions" /> to compare with this instance.</param>
		/// <returns><see langword="true" /> if the specified object is equal to the current instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (SKSamplingOptions obj) =>
#pragma warning disable CS8909
			fMaxAniso == obj.fMaxAniso && fUseCubic == obj.fUseCubic && fCubic == obj.fCubic && fFilter == obj.fFilter && fMipmap == obj.fMipmap;
#pragma warning restore CS8909

		/// <summary>Determines whether the specified object is equal to the current instance.</summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		/// <returns><see langword="true" /> if the specified object is equal to the current instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is SKSamplingOptions f && Equals (f);

		/// <summary>Determines whether two <see cref="T:SkiaSharp.SKSamplingOptions" /> instances are equal.</summary>
		/// <param name="left">The first <see cref="T:SkiaSharp.SKSamplingOptions" /> to compare.</param>
		/// <param name="right">The second <see cref="T:SkiaSharp.SKSamplingOptions" /> to compare.</param>
		/// <returns><see langword="true" /> if the two instances are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (SKSamplingOptions left, SKSamplingOptions right) =>
			left.Equals (right);

		/// <summary>Determines whether two <see cref="T:SkiaSharp.SKSamplingOptions" /> instances are not equal.</summary>
		/// <param name="left">The first <see cref="T:SkiaSharp.SKSamplingOptions" /> to compare.</param>
		/// <param name="right">The second <see cref="T:SkiaSharp.SKSamplingOptions" /> to compare.</param>
		/// <returns><see langword="true" /> if the two instances are not equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (SKSamplingOptions left, SKSamplingOptions right) =>
			!left.Equals (right);

		/// <summary>Returns the hash code for this instance.</summary>
		/// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
		/// <remarks />
		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fMaxAniso);
			hash.Add (fUseCubic);
			hash.Add (fCubic);
			hash.Add (fFilter);
			hash.Add (fMipmap);
			return hash.ToHashCode ();
		}

	}
}
