using System;
using System.Runtime.InteropServices;

internal static partial class Interop
{
	internal static partial class Elementary
	{
		[DllImport(Libraries.Elementary)]
		internal static extern IntPtr elm_image_object_get(IntPtr obj);
	}
}
