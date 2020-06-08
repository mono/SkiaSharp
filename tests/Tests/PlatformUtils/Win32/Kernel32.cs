using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SkiaSharp.Tests
{
	public class Kernel32
	{
		private const string kernel32 = "kernel32.dll";

		static Kernel32()
		{
			CurrentModuleHandle = Kernel32.GetModuleHandle(null);
			if (CurrentModuleHandle == IntPtr.Zero)
			{
				throw new Exception("Could not get module handle.");
			}
		}

		public static IntPtr CurrentModuleHandle { get; }

		[DllImport(kernel32, CallingConvention = CallingConvention.Winapi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
		public static extern IntPtr GetModuleHandle([MarshalAs(UnmanagedType.LPTStr)] string lpModuleName);
	}
}
