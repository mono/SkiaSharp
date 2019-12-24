using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SkiaSharp.Tests
{
	public class User32
	{
		private const string user32 = "user32.dll";

		public const uint IDC_ARROW = 32512;

		public const uint IDI_APPLICATION = 32512;
		public const uint IDI_WINLOGO = 32517;

		public const int SW_HIDE = 0;

		public const uint CS_VREDRAW = 0x1;
		public const uint CS_HREDRAW = 0x2;
		public const uint CS_OWNDC = 0x20;

		public const uint WS_EX_CLIENTEDGE = 0x00000200;

		[DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
		public static extern ushort RegisterClass(ref WNDCLASS lpWndClass);

		[DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
		public static extern ushort UnregisterClass([MarshalAs(UnmanagedType.LPTStr)] string lpClassName, IntPtr hInstance);

		[DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
		public static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

		[DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
		public static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

		[DllImport(user32, CallingConvention = CallingConvention.Winapi)]
		public static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

		[DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
		public static extern IntPtr CreateWindowEx(uint dwExStyle, [MarshalAs(UnmanagedType.LPTStr)] string lpClassName, [MarshalAs(UnmanagedType.LPTStr)] string lpWindowName, WindowStyles dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

		public static IntPtr CreateWindow(string lpClassName, string lpWindowName, WindowStyles dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam)
		{
			return CreateWindowEx(0, lpClassName, lpWindowName, dwStyle, x, y, nWidth, nHeight, hWndParent, hMenu, hInstance, lpParam);
		}

		[DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
		public static extern IntPtr GetDC(IntPtr hWnd);

		[DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);

		[DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool DestroyWindow(IntPtr hWnd);

		[DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool IsWindow(IntPtr hWnd);

		[DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
		public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

		[DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool AdjustWindowRectEx(ref RECT lpRect, WindowStyles dwStyle, bool bMenu, uint dwExStyle);

		[DllImport(user32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);
	}
}
