using System;
using System.Runtime.InteropServices;
// ReSharper disable InconsistentNaming

namespace SkiaSharp;

internal static unsafe partial class DelegateProxies
{
	private static partial void SKManagedTraceMemoryDumpDumpNumericValueProxyImplementation (IntPtr d,
		void* context, /* char */ void* dumpName, /* char */ void* valueName, /* char */ void* units, ulong value)
	{
		try {
			var dump = GetUserData<SKTraceMemoryDump> ((IntPtr)context, out _);
			dump.OnDumpNumericValue (
				Marshal.PtrToStringAnsi ((IntPtr)dumpName),
				Marshal.PtrToStringAnsi ((IntPtr)valueName),
				Marshal.PtrToStringAnsi ((IntPtr)units),
				value);
		} catch (Exception ex) {
			ReportCallbackException (ex, nameof (SKManagedTraceMemoryDumpDumpNumericValueProxyImplementation));
		}
	}

	private static partial void SKManagedTraceMemoryDumpDumpStringValueProxyImplementation (IntPtr d,
		void* context, /* char */ void* dumpName, /* char */ void* valueName, /* char */ void* value)
	{
		try {
			var dump = GetUserData<SKTraceMemoryDump> ((IntPtr)context, out _);
			dump.OnDumpStringValue (
				Marshal.PtrToStringAnsi ((IntPtr)dumpName),
				Marshal.PtrToStringAnsi ((IntPtr)valueName),
				Marshal.PtrToStringAnsi ((IntPtr)value));
		} catch (Exception ex) {
			ReportCallbackException (ex, nameof (SKManagedTraceMemoryDumpDumpStringValueProxyImplementation));
		}
	}
}
