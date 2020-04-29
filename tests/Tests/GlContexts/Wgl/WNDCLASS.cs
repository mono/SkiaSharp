using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SkiaSharp.Tests
{
	public delegate IntPtr WNDPROC(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

	[StructLayout(LayoutKind.Sequential)]
	public struct WNDCLASS
	{
		public uint style;
		[MarshalAs(UnmanagedType.FunctionPtr)]
		public WNDPROC lpfnWndProc;
		public int cbClsExtra;
		public int cbWndExtra;
		public IntPtr hInstance;
		public IntPtr hIcon;
		public IntPtr hCursor;
		public IntPtr hbrBackground;
		[MarshalAs(UnmanagedType.LPTStr)]
		public string lpszMenuName;
		[MarshalAs(UnmanagedType.LPTStr)]
		public string lpszClassName;
	}
}
