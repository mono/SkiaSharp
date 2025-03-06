#nullable disable

using System;
using System.Runtime.InteropServices;
using SkiaSharp.Internals;

namespace SkiaSharp
{
	public partial class SKAutoCoInitialize : IDisposable
	{
		private long hResult;

		public SKAutoCoInitialize()
		{
#if !(__IOS__ || __TVOS__ || __MACOS__ || __MACCATALYST__ || __ANDROID__)
			if (PlatformConfiguration.IsWindows)
				hResult = CoInitializeEx(IntPtr.Zero, COINIT_APARTMENTTHREADED | COINIT_DISABLE_OLE1DDE);
			else
#endif
				hResult = S_OK;
		}

		public bool Initialized => hResult >= 0 || hResult == RPC_E_CHANGED_MODE;

		public void Uninitialize()
		{
			if (hResult >= 0)
			{
#if !(__IOS__ || __TVOS__ || __MACOS__ || __MACCATALYST__ || __ANDROID__)
				if (PlatformConfiguration.IsWindows)
					CoUninitialize();
#endif

				hResult = -1;
			}
		}

		public void Dispose() => Uninitialize();

		private const long S_OK = 0x00000000L;
		private const long RPC_E_CHANGED_MODE = 0x80010106L;

		private const uint COINIT_MULTITHREADED = 0x0;
		private const uint COINIT_APARTMENTTHREADED = 0x2;
		private const uint COINIT_DISABLE_OLE1DDE = 0x4;
		private const uint COINIT_SPEED_OVER_MEMORY = 0x8;

#if !(__IOS__ || __TVOS__ || __MACOS__ || __MACCATALYST__ || __ANDROID__)
#if USE_LIBRARY_IMPORT
		[LibraryImport("ole32.dll", SetLastError = true)]
		private static partial long CoInitializeEx(IntPtr pvReserved, uint dwCoInit);
		[LibraryImport("ole32.dll", SetLastError = true)]
		private static partial void CoUninitialize();
#else
		[DllImport("ole32.dll", CharSet = CharSet.Ansi, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
		private static extern long CoInitializeEx([In, Optional] IntPtr pvReserved, [In] uint dwCoInit);
		[DllImport("ole32.dll", CharSet = CharSet.Ansi, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
		private static extern void CoUninitialize();
#endif
#endif
	}
}
