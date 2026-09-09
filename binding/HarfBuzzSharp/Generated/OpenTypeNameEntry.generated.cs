using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_ot_name_entry_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct OpenTypeNameEntry : IEquatable<OpenTypeNameEntry> {
		// public hb_ot_name_id_t name_id
		private OpenTypeNameId name_id;
		public OpenTypeNameId NameId {
			readonly get => name_id;
			set => name_id = value;
		}

		// public hb_var_int_t var
		private Int32 var;
		public Int32 Var {
			readonly get => var;
			set => var = value;
		}

		// public hb_language_t language
		private IntPtr language;
		public IntPtr Language {
			readonly get => language;
			set => language = value;
		}

		public readonly bool Equals (OpenTypeNameEntry obj) =>
#pragma warning disable CS8909
			name_id == obj.name_id && var == obj.var && language == obj.language;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is OpenTypeNameEntry f && Equals (f);

		public static bool operator == (OpenTypeNameEntry left, OpenTypeNameEntry right) =>
			left.Equals (right);

		public static bool operator != (OpenTypeNameEntry left, OpenTypeNameEntry right) =>
			!left.Equals (right);

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
