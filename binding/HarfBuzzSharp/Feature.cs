#nullable disable

using System;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp
{
	/// <summary>Various font features and variations.</summary>
	/// <remarks></remarks>
	public unsafe partial struct Feature
	{
		private const int MaxFeatureStringSize = 128;

		/// <summary>Creates a new <see cref="T:HarfBuzzSharp.Feature" /> instance with the specified tag.</summary>
		/// <param name="tag">The tag to use.</param>
		/// <remarks></remarks>
		public Feature (Tag tag)
			: this (tag, 1u, 0, uint.MaxValue)
		{
		}

		/// <summary>Creates a new <see cref="T:HarfBuzzSharp.Feature" /> instance with the specified tag and value.</summary>
		/// <param name="tag">The tag to use.</param>
		/// <param name="value">The value to use.</param>
		/// <remarks></remarks>
		public Feature (Tag tag, uint value)
			: this (tag, value, 0, uint.MaxValue)
		{
		}

		/// <summary>Creates a new <see cref="T:HarfBuzzSharp.Feature" /> instance with the specified tag, value, start, and end.</summary>
		/// <param name="tag">The tag to use.</param>
		/// <param name="value">The value to use.</param>
		/// <param name="start">The start to use.</param>
		/// <param name="end">The end to use.</param>
		/// <remarks></remarks>
		public Feature (Tag tag, uint value, uint start, uint end)
		{
			this.tag = tag;
			this.value = value;
			this.start = start;
			this.end = end;
		}

		/// <summary>Gets or sets the tag.</summary>
		/// <value>The tag.</value>
		/// <remarks></remarks>
		public Tag Tag {
			readonly get => tag;
			set => tag = value;
		}

		/// <summary>Gets or sets the value.</summary>
		/// <value>The value.</value>
		/// <remarks></remarks>
		public uint Value {
			readonly get => value;
			set => this.value = value;
		}

		/// <summary>Gets or sets the start.</summary>
		/// <value>The start.</value>
		/// <remarks></remarks>
		public uint Start {
			readonly get => start;
			set => start = value;
		}

		/// <summary>Gets or sets the end.</summary>
		/// <value>The end.</value>
		/// <remarks></remarks>
		public uint End {
			readonly get => end;
			set => end = value;
		}

		/// <summary>Converts the feature into a readable string representation.</summary>
		/// <returns>The string representation.</returns>
		/// <remarks></remarks>
		public override string ToString ()
		{
			fixed (Feature* f = &this) {
				var buffer = Marshal.AllocHGlobal (MaxFeatureStringSize);
				HarfBuzzApi.hb_feature_to_string (f, (void*)buffer, MaxFeatureStringSize);
				var str = Marshal.PtrToStringAnsi (buffer);
				Marshal.FreeHGlobal (buffer);
				return str;
			}
		}

		/// <summary>Attempts to parse the feature from the specified string.</summary>
		/// <param name="s">The string to parse.</param>
		/// <param name="feature">The feature.</param>
		/// <returns>Returns true if the parsing was successful, otherwise false.</returns>
		/// <remarks></remarks>
		public static bool TryParse (string s, out Feature feature)
		{
			fixed (Feature* f = &feature) {
				return HarfBuzzApi.hb_feature_from_string (s, -1, f);
			}
		}

		/// <summary>Parses the feature from the specified string.</summary>
		/// <param name="s">The string to parse.</param>
		/// <returns>The feature.</returns>
		/// <remarks></remarks>
		public static Feature Parse (string s) =>
			TryParse (s, out var feature) ? feature : throw new FormatException ("Unrecognized feature string format.");
	}
}
