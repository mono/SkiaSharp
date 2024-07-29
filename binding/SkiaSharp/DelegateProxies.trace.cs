using System;
using System.Runtime.InteropServices;
// ReSharper disable InconsistentNaming

namespace SkiaSharp;

internal static unsafe partial class DelegateProxies
{
	private static partial void SKManagedTraceMemoryDumpDumpNumericValueProxyImplementation (IntPtr d,
		void* context, /* char */ void* dumpName, /* char */ void* valueName, /* char */ void* units, ulong value)
	{
		var dump = GetUserData<SKTraceMemoryDump> ((IntPtr)context, out _);
		dump.OnDumpNumericValue (
			Marshal.PtrToStringAnsi ((IntPtr)dumpName),
			Marshal.PtrToStringAnsi ((IntPtr)valueName),
			Marshal.PtrToStringAnsi ((IntPtr)units),
			value);
	}

	private static partial void SKManagedTraceMemoryDumpDumpStringValueProxyImplementation (IntPtr d,
		void* context, /* char */ void* dumpName, /* char */ void* valueName, /* char */ void* value)
	{
		var dump = GetUserData<SKTraceMemoryDump> ((IntPtr)context, out _);
		dump.OnDumpStringValue (
			Marshal.PtrToStringAnsi ((IntPtr)dumpName),
			Marshal.PtrToStringAnsi ((IntPtr)valueName),
			Marshal.PtrToStringAnsi ((IntPtr)value));
	}
}
