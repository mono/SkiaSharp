using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_ot_name_entry_t
	/// <summary>Represents an entry in the OpenType name table.</summary>
	/// <remarks />
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct OpenTypeNameEntry : IEquatable<OpenTypeNameEntry> {
		// public hb_ot_name_id_t name_id
		private OpenTypeNameId name_id;
		/// <summary>Gets or sets the name identifier.</summary>
		/// <value>The name identifier.</value>
		/// <remarks />
		public OpenTypeNameId NameId {
			readonly get => name_id;
			set => name_id = value;
		}

		// public hb_var_int_t var
		private Int32 var;
		/// <summary>Gets or sets the variation index.</summary>
		/// <value>The variation index.</value>
		/// <remarks />
		public Int32 Var {
			readonly get => var;
			set => var = value;
		}

		// public hb_language_t language
		private IntPtr language;
		/// <summary>Gets or sets the language identifier.</summary>
		/// <value>The language identifier.</value>
		/// <remarks />
		public IntPtr Language {
			readonly get => language;
			set => language = value;
		}

		/// <summary>Determines whether the specified OpenTypeNameEntry is equal to this instance.</summary>
		/// <param name="obj">The OpenTypeNameEntry to compare with.</param>
		/// <returns><see langword="true" /> if the specified OpenTypeNameEntry is equal to this instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (OpenTypeNameEntry obj) =>
#pragma warning disable CS8909
			name_id == obj.name_id && var == obj.var && language == obj.language;
#pragma warning restore CS8909

		/// <summary>Determines whether the specified object is equal to this instance.</summary>
		/// <param name="obj">The object to compare with.</param>
		/// <returns><see langword="true" /> if the specified object is equal to this instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is OpenTypeNameEntry f && Equals (f);

		/// <summary>Determines whether two specified OpenTypeNameEntry objects are equal.</summary>
		/// <param name="left">The first OpenTypeNameEntry to compare.</param>
		/// <param name="right">The second OpenTypeNameEntry to compare.</param>
		/// <returns><see langword="true" /> if the two OpenTypeNameEntry objects are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (OpenTypeNameEntry left, OpenTypeNameEntry right) =>
			left.Equals (right);

		/// <summary>Determines whether two specified OpenTypeNameEntry objects are not equal.</summary>
		/// <param name="left">The first OpenTypeNameEntry to compare.</param>
		/// <param name="right">The second OpenTypeNameEntry to compare.</param>
		/// <returns><see langword="true" /> if the two OpenTypeNameEntry objects are not equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (OpenTypeNameEntry left, OpenTypeNameEntry right) =>
			!left.Equals (right);

		/// <summary>Returns the hash code for this instance.</summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		/// <remarks />
		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (name_id);
			hash.Add (var);
			hash.Add (language);
			return hash.ToHashCode ();
		}

	}
}
