using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_fontmetrics_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKFontMetrics : IEquatable<SKFontMetrics> {
		// public uint32_t fFlags
		private UInt32 fFlags;

		// public float fTop
		private Single fTop;

		// public float fAscent
		private Single fAscent;

		// public float fDescent
		private Single fDescent;

		// public float fBottom
		private Single fBottom;

		// public float fLeading
		private Single fLeading;

		// public float fAvgCharWidth
		private Single fAvgCharWidth;

		// public float fMaxCharWidth
		private Single fMaxCharWidth;

		// public float fXMin
		private Single fXMin;

		// public float fXMax
		private Single fXMax;

		// public float fXHeight
		private Single fXHeight;

		// public float fCapHeight
		private Single fCapHeight;

		// public float fUnderlineThickness
		private Single fUnderlineThickness;

		// public float fUnderlinePosition
		private Single fUnderlinePosition;

		// public float fStrikeoutThickness
		private Single fStrikeoutThickness;

		// public float fStrikeoutPosition
		private Single fStrikeoutPosition;

		public readonly bool Equals (SKFontMetrics obj) =>
#pragma warning disable CS8909
			fFlags == obj.fFlags && fTop == obj.fTop && fAscent == obj.fAscent && fDescent == obj.fDescent && fBottom == obj.fBottom && fLeading == obj.fLeading && fAvgCharWidth == obj.fAvgCharWidth && fMaxCharWidth == obj.fMaxCharWidth && fXMin == obj.fXMin && fXMax == obj.fXMax && fXHeight == obj.fXHeight && fCapHeight == obj.fCapHeight && fUnderlineThickness == obj.fUnderlineThickness && fUnderlinePosition == obj.fUnderlinePosition && fStrikeoutThickness == obj.fStrikeoutThickness && fStrikeoutPosition == obj.fStrikeoutPosition;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKFontMetrics f && Equals (f);

		public static bool operator == (SKFontMetrics left, SKFontMetrics right) =>
			left.Equals (right);

		public static bool operator != (SKFontMetrics left, SKFontMetrics right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fFlags);
			hash.Add (fTop);
			hash.Add (fAscent);
			hash.Add (fDescent);
			hash.Add (fBottom);
			hash.Add (fLeading);
			hash.Add (fAvgCharWidth);
			hash.Add (fMaxCharWidth);
			hash.Add (fXMin);
			hash.Add (fXMax);
			hash.Add (fXHeight);
			hash.Add (fCapHeight);
			hash.Add (fUnderlineThickness);
			hash.Add (fUnderlinePosition);
			hash.Add (fStrikeoutThickness);
			hash.Add (fStrikeoutPosition);
			return hash.ToHashCode ();
		}

	}
}
