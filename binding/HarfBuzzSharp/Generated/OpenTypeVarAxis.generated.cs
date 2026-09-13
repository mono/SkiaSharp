using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_ot_var_axis_t
	/// <summary>Represents a variation axis in a variable font.</summary>
	/// <remarks>This structure contains information about a font variation axis, including its valid range and default value.</remarks>
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct OpenTypeVarAxis : IEquatable<OpenTypeVarAxis> {
		// public hb_tag_t tag
		private UInt32 tag;
		/// <summary>Gets or sets the four-character tag identifying the axis (for example, 'wght' for weight).</summary>
		/// <value>The axis tag.</value>
		/// <remarks />
		public UInt32 Tag {
			readonly get => tag;
			set => tag = value;
		}

		// public hb_ot_name_id_t name_id
		private OpenTypeNameId name_id;
		/// <summary>Gets or sets the name identifier for the axis name in the font's name table.</summary>
		/// <value>The name table identifier.</value>
		/// <remarks />
		public OpenTypeNameId NameId {
			readonly get => name_id;
			set => name_id = value;
		}

		// public float min_value
		private Single min_value;
		/// <summary>Gets or sets the minimum value for this axis.</summary>
		/// <value>The minimum axis value.</value>
		/// <remarks />
		public Single MinValue {
			readonly get => min_value;
			set => min_value = value;
		}

		// public float default_value
		private Single default_value;
		/// <summary>Gets or sets the default value for this axis.</summary>
		/// <value>The default axis value.</value>
		/// <remarks />
		public Single DefaultValue {
			readonly get => default_value;
			set => default_value = value;
		}

		// public float max_value
		private Single max_value;
		/// <summary>Gets or sets the maximum value for this axis.</summary>
		/// <value>The maximum axis value.</value>
		/// <remarks />
		public Single MaxValue {
			readonly get => max_value;
			set => max_value = value;
		}

		/// <summary>Determines whether the specified <see cref="T:HarfBuzzSharp.OpenTypeVarAxis" /> is equal to the current instance.</summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		/// <returns><see langword="true" /> if the objects are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (OpenTypeVarAxis obj) =>
#pragma warning disable CS8909
			tag == obj.tag && name_id == obj.name_id && min_value == obj.min_value && default_value == obj.default_value && max_value == obj.max_value;
#pragma warning restore CS8909

		/// <summary>Determines whether the specified object is equal to the current instance.</summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		/// <returns><see langword="true" /> if the objects are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is OpenTypeVarAxis f && Equals (f);

		/// <summary>Determines whether two <see cref="T:HarfBuzzSharp.OpenTypeVarAxis" /> structures are equal.</summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns><see langword="true" /> if the values are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (OpenTypeVarAxis left, OpenTypeVarAxis right) =>
			left.Equals (right);

		/// <summary>Determines whether two <see cref="T:HarfBuzzSharp.OpenTypeVarAxis" /> structures are not equal.</summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns><see langword="true" /> if the values are not equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (OpenTypeVarAxis left, OpenTypeVarAxis right) =>
			!left.Equals (right);

		/// <summary>Returns a hash code for this instance.</summary>
		/// <returns>A hash code for the current object.</returns>
		/// <remarks />
		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (tag);
			hash.Add (name_id);
			hash.Add (min_value);
			hash.Add (default_value);
			hash.Add (max_value);
			return hash.ToHashCode ();
		}

	}
}
