using System;
using System.Runtime.InteropServices;

internal static partial class Interop
{
	internal static partial class Elementary
	{
		[DllImport(Libraries.Elementary)]
		internal static extern IntPtr elm_layout_add(IntPtr obj);

		[DllImport(Libraries.Elementary)]
		internal static extern bool elm_layout_theme_set(IntPtr obj, string klass, string group, string style);

		[DllImport(Libraries.Elementary)]
		internal static extern IntPtr elm_object_part_content_get(IntPtr obj, string part);

		[DllImport(Libraries.Elementary)]
		internal static extern void elm_object_part_content_set(IntPtr obj, string part, IntPtr content);
	}
}
