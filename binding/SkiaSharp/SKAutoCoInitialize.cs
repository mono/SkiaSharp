#nullable disable

using System;
using System.Runtime.InteropServices;
using SkiaSharp.Internals;

namespace SkiaSharp
{
	/// <summary>
	/// Convenience class used to automatically initialize and uninitialize COM on supported platforms.
	/// </summary>
	/// <remarks><para>This is only supported on Windows, and is usually not needed. However, when creating a .NET Core app, COM may not be initialized.</para><para>Currently, only <see cref="SKDocument" /> and more specifically, XPS documents require COM.</para></remarks>
	public partial class SKAutoCoInitialize : IDisposable
	{
		private long hResult;

		/// <summary>
		/// Initializes COM.
		/// </summary>
		public SKAutoCoInitialize()
		{
#if !(__IOS__ || __TVOS__ || __MACOS__ || __MACCATALYST__ || __ANDROID__)
			if (PlatformConfiguration.IsWindows)
				hResult = CoInitializeEx(IntPtr.Zero, COINIT_APARTMENTTHREADED | COINIT_DISABLE_OLE1DDE);
			else
#endif
				hResult = S_OK;
		}

		/// <summary>
		/// Gets a value indicating whether COM is initialized or not.
		/// </summary>
		public bool Initialized => hResult >= 0 || hResult == RPC_E_CHANGED_MODE;

		/// <summary>
		/// Uninitializes COM.
		/// </summary>
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

		/// <summary>
		/// Uninitializes COM.
		/// </summary>
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
