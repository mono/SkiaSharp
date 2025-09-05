#nullable disable

using System;

namespace HarfBuzzSharp
{
	/// <summary>
	/// Represents a particular Unicode script.
	/// </summary>
	/// <remarks></remarks>
	public partial struct Script : IEquatable<Script>
	{
		private readonly Tag tag;

		private Script (Tag tag)
		{
			this.tag = tag;
		}

		/// <summary>
		/// Gets the horizontal direction of this script.
		/// </summary>
		/// <value></value>
		/// <remarks></remarks>
		public Direction HorizontalDirection =>
			HarfBuzzApi.hb_script_get_horizontal_direction (tag);

		/// <summary>
		/// Parses the ISO 15924 script tag into the corresponding <see cref="T:HarfBuzzSharp.Script" />.
		/// </summary>
		/// <param name="str">The ISO 15924 script tag to parse.</param>
		/// <returns>Returns the <see cref="T:HarfBuzzSharp.Script" /> that corresponds the script tag that was parsed.</returns>
		/// <remarks></remarks>
		public static Script Parse (string str) =>
			HarfBuzzApi.hb_script_from_string (str, -1);

		/// <summary>
		/// To be added.
		/// </summary>
		/// <param name="str">To be added.</param>
		/// <param name="script">To be added.</param>
		/// <returns>To be added.</returns>
		/// <remarks>To be added.</remarks>
		public static bool TryParse (string str, out Script script)
		{
			script = Parse (str);

			return script != Unknown;
		}

		/// <summary>
		/// Returns a string representation of the value of this instance of the <see cref="T:HarfBuzzSharp.Script" />.
		/// </summary>
		/// <returns>Returns a string representation.</returns>
		/// <remarks></remarks>
		public override string ToString () => tag.ToString ();

		/// <summary>
		/// Defines an implicit conversion of a <see cref="T:HarfBuzzSharp.Script" /> to a <see cref="T:System.UInt32" /> tag.
		/// </summary>
		/// <param name="script">The script to be converted into a tag.</param>
		/// <returns>Returns the tag that corresponds to the script.</returns>
		/// <remarks></remarks>
		public static implicit operator uint (Script script) => script.tag;

		/// <summary>
		/// Defines an implicit conversion of a <see cref="T:System.UInt32" /> tag to a <see cref="T:HarfBuzzSharp.Script" />.
		/// </summary>
		/// <param name="tag">The tag to be converted into a script.</param>
		/// <returns>Returns the script that corresponds to the tag.</returns>
		/// <remarks></remarks>
		public static implicit operator Script (uint tag) => new Script (tag);

		/// <summary>
		/// Returns a value indicating whether this instance and a specified <see cref="T:HarfBuzzSharp.Script" /> object represent the same value.
		/// </summary>
		/// <param name="obj">An object to compare to this instance.</param>
		/// <returns>Returns true if the other value is equal to this instance, otherwise false.</returns>
		/// <remarks></remarks>
		public override bool Equals (object obj) =>
			obj is Script script && tag.Equals (script.tag);

		/// <summary>
		/// Returns a value indicating whether this instance and a specified <see cref="T:HarfBuzzSharp.Script" /> object represent the same value.
		/// </summary>
		/// <param name="other">An object to compare to this instance.</param>
		/// <returns>Returns true if the other value is equal to this instance, otherwise false.</returns>
		/// <remarks></remarks>
		public bool Equals (Script other) => tag.Equals (other.tag);

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>Returns the hash code for this instance.</returns>
		/// <remarks></remarks>
		public override int GetHashCode () => tag.GetHashCode ();
	}
}
