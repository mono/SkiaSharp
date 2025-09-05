#nullable disable

using System;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp
{
	/// <summary>
	/// Various font features and variations.
	/// </summary>
	public unsafe partial struct Feature
	{
		private const int MaxFeatureStringSize = 128;

		/// <summary>
		/// Creates a new <see cref="Feature" /> instance with the specified tag.
		/// </summary>
		/// <param name="tag">The tag to use.</param>
		public Feature (Tag tag)
			: this (tag, 1u, 0, uint.MaxValue)
		{
		}

		/// <summary>
		/// Creates a new <see cref="Feature" /> instance with the specified tag.
		/// </summary>
		/// <param name="tag">The tag to use.</param>
		/// <param name="value">The value to use.</param>
		public Feature (Tag tag, uint value)
			: this (tag, value, 0, uint.MaxValue)
		{
		}

		/// <summary>
		/// Creates a new <see cref="Feature" /> instance with the specified tag.
		/// </summary>
		/// <param name="tag">The tag to use.</param>
		/// <param name="value">The value to use.</param>
		/// <param name="start">The start value.</param>
		/// <param name="end">The end value.</param>
		public Feature (Tag tag, uint value, uint start, uint end)
		{
			this.tag = tag;
			this.value = value;
			this.start = start;
			this.end = end;
		}

		/// <summary>
		/// Gets or sets the tag.
		/// </summary>
		public Tag Tag {
			readonly get => tag;
			set => tag = value;
		}

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		public uint Value {
			readonly get => value;
			set => this.value = value;
		}

		/// <summary>
		/// Gets or sets the start.
		/// </summary>
		public uint Start {
			readonly get => start;
			set => start = value;
		}

		/// <summary>
		/// Gets or sets the end.
		/// </summary>
		public uint End {
			readonly get => end;
			set => end = value;
		}

		/// <summary>
		/// Returns the string representation of the feature.
		/// </summary>
		/// <returns>Returns the string representation of the feature.</returns>
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

		/// <summary>
		/// Tries to parse the feature string.
		/// </summary>
		/// <param name="s">The feature string to parse.</param>
		/// <param name="feature">The feature.</param>
		/// <returns>Returns true on success, otherwise false.</returns>
		public static bool TryParse (string s, out Feature feature)
		{
			fixed (Feature* f = &feature) {
				return HarfBuzzApi.hb_feature_from_string (s, -1, f);
			}
		}

		/// <summary>
		/// Parses a feature string.
		/// </summary>
		/// <param name="s">The feature string to parse.</param>
		/// <returns>Returns the new feature.</returns>
		public static Feature Parse (string s) =>
			TryParse (s, out var feature) ? feature : throw new FormatException ("Unrecognized feature string format.");
	}
}
