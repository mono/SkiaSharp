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
		public bool Grayscale {
			readonly get => fGrayscale > 0;
			set => fGrayscale = value ? (byte)1 : (byte)0;
		}

		// public sk_highcontrastconfig_invertstyle_t fInvertStyle
		private SKHighContrastConfigInvertStyle fInvertStyle;
		public SKHighContrastConfigInvertStyle InvertStyle {
			readonly get => fInvertStyle;
			set => fInvertStyle = value;
		}

		// public float fContrast
		private Single fContrast;
		public Single Contrast {
			readonly get => fContrast;
			set => fContrast = value;
		}

		public readonly bool Equals (SKHighContrastConfig obj) =>
#pragma warning disable CS8909
			fGrayscale == obj.fGrayscale && fInvertStyle == obj.fInvertStyle && fContrast == obj.fContrast;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKHighContrastConfig f && Equals (f);

		public static bool operator == (SKHighContrastConfig left, SKHighContrastConfig right) =>
			left.Equals (right);

		public static bool operator != (SKHighContrastConfig left, SKHighContrastConfig right) =>
			!left.Equals (right);

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
