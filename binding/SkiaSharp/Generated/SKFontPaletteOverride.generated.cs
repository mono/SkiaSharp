using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_fontarguments_palette_override_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKFontPaletteOverride : IEquatable<SKFontPaletteOverride> {
		// public uint16_t index
		private UInt16 index;
		public UInt16 Index {
			readonly get => index;
			set => index = value;
		}

		// public sk_color_t color
		private UInt32 color;
		public UInt32 Color {
			readonly get => color;
			set => color = value;
		}

		public readonly bool Equals (SKFontPaletteOverride obj) =>
#pragma warning disable CS8909
			index == obj.index && color == obj.color;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKFontPaletteOverride f && Equals (f);

		public static bool operator == (SKFontPaletteOverride left, SKFontPaletteOverride right) =>
			left.Equals (right);

		public static bool operator != (SKFontPaletteOverride left, SKFontPaletteOverride right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (index);
			hash.Add (color);
			return hash.ToHashCode ();
		}

	}
}
