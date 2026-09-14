using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_ot_color_layer_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct OpenTypeColorLayer : IEquatable<OpenTypeColorLayer> {
		// public hb_codepoint_t glyph
		private UInt32 glyph;
		public UInt32 Glyph {
			readonly get => glyph;
			set => glyph = value;
		}

		// public unsigned int color_index
		private UInt32 color_index;
		public UInt32 ColorIndex {
			readonly get => color_index;
			set => color_index = value;
		}

		public readonly bool Equals (OpenTypeColorLayer obj) =>
#pragma warning disable CS8909
			glyph == obj.glyph && color_index == obj.color_index;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is OpenTypeColorLayer f && Equals (f);

		public static bool operator == (OpenTypeColorLayer left, OpenTypeColorLayer right) =>
			left.Equals (right);

		public static bool operator != (OpenTypeColorLayer left, OpenTypeColorLayer right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (glyph);
			hash.Add (color_index);
			return hash.ToHashCode ();
		}

	}
}
