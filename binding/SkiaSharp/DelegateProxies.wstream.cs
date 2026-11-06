using System;
using System.Threading;
// ReSharper disable InconsistentNaming

namespace SkiaSharp;

internal static unsafe partial class DelegateProxies
{
	private static partial /* size_t */ IntPtr SKManagedWStreamBytesWrittenProxyImplementation (IntPtr s, void* context)
	{
		try {
			var stream = GetUserData<SKAbstractManagedWStream> ((IntPtr)context, out _);
			return stream.OnBytesWritten ();
		} catch {
			return IntPtr.Zero;
		}
	}

	private static partial void SKManagedWStreamDestroyProxyImplementation (IntPtr s, void* context)
	{
		var stream = GetUserData<SKAbstractManagedWStream> ((IntPtr)context, out var gch);
		try {
			if (stream != null) {
				Interlocked.Exchange (ref stream.fromNative, 1);
				try {
					stream.Dispose ();
				} finally {
					stream.CloseFromNative ();
				}
			}
		} catch {
		} finally {
			if (gch.IsAllocated)
				gch.Free ();
		}
	}

	private static partial void SKManagedWStreamFlushProxyImplementation (IntPtr s, void* context)
	{
		try {
			var stream = GetUserData<SKAbstractManagedWStream> ((IntPtr)context, out _);
			stream.OnFlush ();
		} catch {
		}
	}

	private static partial byte SKManagedWStreamWriteProxyImplementation (IntPtr s, void* context,
		void* buffer, /* size_t */ IntPtr size)
	{
		try {
			var stream = GetUserData<SKAbstractManagedWStream> ((IntPtr)context, out _);
			return stream.OnWrite ((IntPtr)buffer, size) ? (byte)1 : (byte)0;
		} catch {
			return 0;
		}
	}
}
