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
		public readonly Int32 MaxAniso => fMaxAniso;

		// public bool fUseCubic
		private readonly Byte fUseCubic;
		public readonly bool UseCubic => fUseCubic > 0;

		// public sk_cubic_resampler_t fCubic
		private readonly SKCubicResampler fCubic;
		public readonly SKCubicResampler Cubic => fCubic;

		// public sk_filter_mode_t fFilter
		private readonly SKFilterMode fFilter;
		public readonly SKFilterMode Filter => fFilter;

		// public sk_mipmap_mode_t fMipmap
		private readonly SKMipmapMode fMipmap;
		public readonly SKMipmapMode Mipmap => fMipmap;

		public readonly bool Equals (SKSamplingOptions obj) =>
#pragma warning disable CS8909
			fMaxAniso == obj.fMaxAniso && fUseCubic == obj.fUseCubic && fCubic == obj.fCubic && fFilter == obj.fFilter && fMipmap == obj.fMipmap;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKSamplingOptions f && Equals (f);

		public static bool operator == (SKSamplingOptions left, SKSamplingOptions right) =>
			left.Equals (right);

		public static bool operator != (SKSamplingOptions left, SKSamplingOptions right) =>
			!left.Equals (right);

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
