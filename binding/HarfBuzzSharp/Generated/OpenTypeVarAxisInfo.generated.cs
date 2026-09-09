using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_ot_var_axis_info_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct OpenTypeVarAxisInfo : IEquatable<OpenTypeVarAxisInfo> {
		// public unsigned int axis_index
		private UInt32 axis_index;
		public UInt32 AxisIndex {
			readonly get => axis_index;
			set => axis_index = value;
		}

		// public hb_tag_t tag
		private UInt32 tag;
		public UInt32 Tag {
			readonly get => tag;
			set => tag = value;
		}

		// public hb_ot_name_id_t name_id
		private OpenTypeNameId name_id;
		public OpenTypeNameId NameId {
			readonly get => name_id;
			set => name_id = value;
		}

		// public hb_ot_var_axis_flags_t flags
		private OpenTypeVarAxisFlags flags;
		public OpenTypeVarAxisFlags Flags {
			readonly get => flags;
			set => flags = value;
		}

		// public float min_value
		private Single min_value;
		public Single MinValue {
			readonly get => min_value;
			set => min_value = value;
		}

		// public float default_value
		private Single default_value;
		public Single DefaultValue {
			readonly get => default_value;
			set => default_value = value;
		}

		// public float max_value
		private Single max_value;
		public Single MaxValue {
			readonly get => max_value;
			set => max_value = value;
		}

		// public unsigned int reserved
		private UInt32 reserved;

		public readonly bool Equals (OpenTypeVarAxisInfo obj) =>
#pragma warning disable CS8909
			axis_index == obj.axis_index && tag == obj.tag && name_id == obj.name_id && flags == obj.flags && min_value == obj.min_value && default_value == obj.default_value && max_value == obj.max_value && reserved == obj.reserved;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is OpenTypeVarAxisInfo f && Equals (f);

		public static bool operator == (OpenTypeVarAxisInfo left, OpenTypeVarAxisInfo right) =>
			left.Equals (right);

		public static bool operator != (OpenTypeVarAxisInfo left, OpenTypeVarAxisInfo right) =>
			!left.Equals (right);

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
