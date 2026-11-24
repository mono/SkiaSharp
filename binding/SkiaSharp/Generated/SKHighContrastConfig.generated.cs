using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_highcontrastconfig_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKHighContrastConfig : IEquatable<SKHighContrastConfig> {
		// public bool fGrayscale
		private Byte fGrayscale;
		/// <summary>Gets or sets a value indicating whether the color will be converted to grayscale.</summary>
		/// <value><see langword="true" /> if the color will be converted to grayscale; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool Grayscale {
			readonly get => fGrayscale > 0;
			set => fGrayscale = value ? (byte)1 : (byte)0;
		}

		// public sk_highcontrastconfig_invertstyle_t fInvertStyle
		private SKHighContrastConfigInvertStyle fInvertStyle;
		/// <summary>Gets or sets a value indicating whether to invert brightness, lightness, or neither.</summary>
		/// <value>One of the enumeration values that specifies the invert style.</value>
		/// <remarks />
		public SKHighContrastConfigInvertStyle InvertStyle {
			readonly get => fInvertStyle;
			set => fInvertStyle = value;
		}

		// public float fContrast
		private Single fContrast;
		/// <summary>Gets or sets the amount to adjust the contrast by, in the range -1.0 through 1.0.</summary>
		/// <value>The contrast adjustment value.</value>
		/// <remarks />
		public Single Contrast {
			readonly get => fContrast;
			set => fContrast = value;
		}

		/// <summary>Determines whether the specified <see cref="T:SkiaSharp.SKHighContrastConfig" /> is equal to this instance.</summary>
		/// <param name="obj">The <see cref="T:SkiaSharp.SKHighContrastConfig" /> to compare with this instance.</param>
		/// <returns><see langword="true" /> if the specified object is equal to this instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (SKHighContrastConfig obj) =>
#pragma warning disable CS8909
			fGrayscale == obj.fGrayscale && fInvertStyle == obj.fInvertStyle && fContrast == obj.fContrast;
#pragma warning restore CS8909

		/// <summary>Determines whether the specified object is equal to this instance.</summary>
		/// <param name="obj">The object to compare with this instance.</param>
		/// <returns><see langword="true" /> if the specified object is equal to this instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is SKHighContrastConfig f && Equals (f);

		/// <summary>Determines whether two <see cref="T:SkiaSharp.SKHighContrastConfig" /> instances are equal.</summary>
		/// <param name="left">The first <see cref="T:SkiaSharp.SKHighContrastConfig" /> to compare.</param>
		/// <param name="right">The second <see cref="T:SkiaSharp.SKHighContrastConfig" /> to compare.</param>
		/// <returns><see langword="true" /> if the two instances are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (SKHighContrastConfig left, SKHighContrastConfig right) =>
			left.Equals (right);

		/// <summary>Determines whether two <see cref="T:SkiaSharp.SKHighContrastConfig" /> instances are not equal.</summary>
		/// <param name="left">The first <see cref="T:SkiaSharp.SKHighContrastConfig" /> to compare.</param>
		/// <param name="right">The second <see cref="T:SkiaSharp.SKHighContrastConfig" /> to compare.</param>
		/// <returns><see langword="true" /> if the two instances are not equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (SKHighContrastConfig left, SKHighContrastConfig right) =>
			!left.Equals (right);

		/// <summary>Returns a hash code for this instance.</summary>
		/// <returns>A hash code for this instance.</returns>
		/// <remarks />
		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fGrayscale);
			hash.Add (fInvertStyle);
			hash.Add (fContrast);
			return hash.ToHashCode ();
		}

	}
}
