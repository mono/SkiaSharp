using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	public class SKAutoCoInitialize : IDisposable
	{
		private long hResult;

		public SKAutoCoInitialize()
		{
			if (PlatformConfiguration.IsWindows)
				hResult = CoInitializeEx(IntPtr.Zero, COINIT_APARTMENTTHREADED | COINIT_DISABLE_OLE1DDE);
			else
				hResult = S_OK;
		}

		public bool Initialized => hResult >= 0 || hResult == RPC_E_CHANGED_MODE;

		public void Uninitialize()
		{
			if (hResult >= 0)
			{
				if (PlatformConfiguration.IsWindows)
					CoUninitialize();

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

		[DllImport("ole32.dll", CharSet = CharSet.Ansi, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
		private static extern long CoInitializeEx([In, Optional] IntPtr pvReserved, [In] uint dwCoInit);

		[DllImport("ole32.dll", CharSet = CharSet.Ansi, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
		private static extern void CoUninitialize();
	}
}
