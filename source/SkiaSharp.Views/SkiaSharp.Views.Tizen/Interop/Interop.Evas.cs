using System;
using System.Runtime.InteropServices;

internal static partial class Interop
{
	internal static partial class Evas
	{
		[DllImport(Libraries.Evas)]
		internal static extern IntPtr evas_object_evas_get(IntPtr obj);
	}
}
