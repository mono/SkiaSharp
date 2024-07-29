using System;
using System.Threading;
// ReSharper disable InconsistentNaming

namespace SkiaSharp;

internal static unsafe partial class DelegateProxies
{
	private static partial /* size_t */ IntPtr SKManagedWStreamBytesWrittenProxyImplementation (IntPtr s, void* context)
	{
		var stream = GetUserData<SKAbstractManagedWStream> ((IntPtr)context, out _);
		return stream.OnBytesWritten ();
	}

	private static partial void SKManagedWStreamDestroyProxyImplementation (IntPtr s, void* context)
	{
		var stream = GetUserData<SKAbstractManagedWStream> ((IntPtr)context, out var gch);
		if (stream != null) {
			Interlocked.Exchange (ref stream.fromNative, 1);
			stream.Dispose ();
		}
		gch.Free ();
	}

	private static partial void SKManagedWStreamFlushProxyImplementation (IntPtr s, void* context)
	{
		var stream = GetUserData<SKAbstractManagedWStream> ((IntPtr)context, out _);
		stream.OnFlush ();
	}

	private static partial bool SKManagedWStreamWriteProxyImplementation (IntPtr s, void* context,
		void* buffer, /* size_t */ IntPtr size)
	{
		var stream = GetUserData<SKAbstractManagedWStream> ((IntPtr)context, out _);
		return stream.OnWrite ((IntPtr)buffer, size);
	}
}
