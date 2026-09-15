using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_ot_var_axis_info_t
	/// <summary>Represents extended information about a variation axis in a variable font.</summary>
	/// <remarks>This structure provides detailed information about a font variation axis, including its range, default value, and flags.</remarks>
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct OpenTypeVarAxisInfo : IEquatable<OpenTypeVarAxisInfo> {
		// public unsigned int axis_index
		private UInt32 axis_index;
		/// <summary>Gets or sets the index of the axis in the font's variation axis array.</summary>
		/// <value>The zero-based axis index.</value>
		/// <remarks />
		public UInt32 AxisIndex {
			readonly get => axis_index;
			set => axis_index = value;
		}

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

		// public hb_ot_var_axis_flags_t flags
		private OpenTypeVarAxisFlags flags;
		/// <summary>Gets or sets the axis flags.</summary>
		/// <value>The axis flags.</value>
		/// <remarks />
		public OpenTypeVarAxisFlags Flags {
			readonly get => flags;
			set => flags = value;
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

		// public unsigned int reserved
		private UInt32 reserved;

		/// <summary>Determines whether the specified <see cref="T:HarfBuzzSharp.OpenTypeVarAxisInfo" /> is equal to the current instance.</summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		/// <returns><see langword="true" /> if the objects are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (OpenTypeVarAxisInfo obj) =>
#pragma warning disable CS8909
			axis_index == obj.axis_index && tag == obj.tag && name_id == obj.name_id && flags == obj.flags && min_value == obj.min_value && default_value == obj.default_value && max_value == obj.max_value && reserved == obj.reserved;
#pragma warning restore CS8909

		/// <summary>Determines whether the specified object is equal to the current instance.</summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		/// <returns><see langword="true" /> if the objects are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is OpenTypeVarAxisInfo f && Equals (f);

		/// <summary>Determines whether two <see cref="T:HarfBuzzSharp.OpenTypeVarAxisInfo" /> structures are equal.</summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns><see langword="true" /> if the values are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (OpenTypeVarAxisInfo left, OpenTypeVarAxisInfo right) =>
			left.Equals (right);

		/// <summary>Determines whether two <see cref="T:HarfBuzzSharp.OpenTypeVarAxisInfo" /> structures are not equal.</summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns><see langword="true" /> if the values are not equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (OpenTypeVarAxisInfo left, OpenTypeVarAxisInfo right) =>
			!left.Equals (right);

		/// <summary>Returns a hash code for this instance.</summary>
		/// <returns>A hash code for the current object.</returns>
		/// <remarks />
		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (axis_index);
			hash.Add (tag);
			hash.Add (name_id);
			hash.Add (flags);
			hash.Add (min_value);
			hash.Add (default_value);
			hash.Add (max_value);
			hash.Add (reserved);
			return hash.ToHashCode ();
		}

	}
}
