using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_ot_color_layer_t
	/// <summary>Represents a color layer in an OpenType color font.</summary>
	/// <remarks />
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct OpenTypeColorLayer : IEquatable<OpenTypeColorLayer> {
		// public hb_codepoint_t glyph
		private UInt32 glyph;
		/// <summary>Gets or sets the glyph index for this layer.</summary>
		/// <value>The glyph index.</value>
		/// <remarks />
		public UInt32 Glyph {
			readonly get => glyph;
			set => glyph = value;
		}

		// public unsigned int color_index
		private UInt32 color_index;
		/// <summary>Gets or sets the color index in the palette.</summary>
		/// <value>The zero-based index into the color palette.</value>
		/// <remarks />
		public UInt32 ColorIndex {
			readonly get => color_index;
			set => color_index = value;
		}

		/// <summary>Determines whether the specified OpenTypeColorLayer is equal to this instance.</summary>
		/// <param name="obj">The OpenTypeColorLayer to compare with.</param>
		/// <returns><see langword="true" /> if the specified OpenTypeColorLayer is equal to this instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (OpenTypeColorLayer obj) =>
#pragma warning disable CS8909
			glyph == obj.glyph && color_index == obj.color_index;
#pragma warning restore CS8909

		/// <summary>Determines whether the specified object is equal to this instance.</summary>
		/// <param name="obj">The object to compare with.</param>
		/// <returns><see langword="true" /> if the specified object is equal to this instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is OpenTypeColorLayer f && Equals (f);

		/// <summary>Determines whether two specified OpenTypeColorLayer objects are equal.</summary>
		/// <param name="left">The first OpenTypeColorLayer to compare.</param>
		/// <param name="right">The second OpenTypeColorLayer to compare.</param>
		/// <returns><see langword="true" /> if the two OpenTypeColorLayer objects are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (OpenTypeColorLayer left, OpenTypeColorLayer right) =>
			left.Equals (right);

		/// <summary>Determines whether two specified OpenTypeColorLayer objects are not equal.</summary>
		/// <param name="left">The first OpenTypeColorLayer to compare.</param>
		/// <param name="right">The second OpenTypeColorLayer to compare.</param>
		/// <returns><see langword="true" /> if the two OpenTypeColorLayer objects are not equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (OpenTypeColorLayer left, OpenTypeColorLayer right) =>
			!left.Equals (right);

		/// <summary>Returns the hash code for this instance.</summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		/// <remarks />
		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (glyph);
			hash.Add (color_index);
			return hash.ToHashCode ();
		}

	}
}
