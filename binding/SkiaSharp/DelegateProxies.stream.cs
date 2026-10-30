using System;
using System.Threading;
// ReSharper disable InconsistentNaming

namespace SkiaSharp;

internal static unsafe partial class DelegateProxies
{
	private static partial IntPtr SKManagedStreamDuplicateProxyImplementation (IntPtr s, void* context)
	{
		try {
			var stream = GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnDuplicate ();
		} catch {
			return IntPtr.Zero;
		}
	}

	private static partial IntPtr SKManagedStreamForkProxyImplementation (IntPtr s, void* context)
	{
		try {
			var stream = GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnFork ();
		} catch {
			return IntPtr.Zero;
		}
	}

	private static partial /* size_t */ IntPtr SKManagedStreamGetLengthProxyImplementation (IntPtr s, void* context)
	{
		try {
			var stream = GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnGetLength ();
		} catch {
			return IntPtr.Zero;
		}
	}

	private static partial /* size_t */ IntPtr SKManagedStreamGetPositionProxyImplementation (IntPtr s, void* context)
	{
		try {
			var stream = GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnGetPosition ();
		} catch {
			return IntPtr.Zero;
		}
	}

	private static partial byte SKManagedStreamHasLengthProxyImplementation (IntPtr s, void* context)
	{
		try {
			var stream = GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnHasLength () ? (byte)1 : (byte)0;
		} catch {
			return 0;
		}
	}

	private static partial void SKManagedStreamDestroyProxyImplementation (IntPtr s, void* context)
	{
		var stream = GetUserData<SKAbstractManagedStream> ((IntPtr)context, out var gch);
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

	private static partial byte SKManagedStreamHasPositionProxyImplementation (IntPtr s, void* context)
	{
		try {
			var stream = GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnHasPosition () ? (byte)1 : (byte)0;
		} catch {
			return 0;
		}
	}

	private static partial byte SKManagedStreamIsAtEndProxyImplementation (IntPtr s, void* context)
	{
		try {
			var stream = GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			if (stream == null || Interlocked.CompareExchange (ref stream.hasError, 0, 0) != 0)
				return 1;
			return stream.OnIsAtEnd () ? (byte)1 : (byte)0;
		} catch {
			return 1;
		}
	}

	private static partial byte SKManagedStreamMoveProxyImplementation (IntPtr s, void* context, int offset)
	{
		SKAbstractManagedStream? stream = null;
		try {
			stream = GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			if (stream.OnMove (offset)) {
				Interlocked.Exchange (ref stream.hasError, 0);
				return 1;
			}
			return 0;
		} catch {
			if (stream != null)
				Interlocked.Exchange (ref stream.hasError, 1);
			return 0;
		}
	}

	private static partial /* size_t */
		IntPtr SKManagedStreamPeekProxyImplementation (IntPtr s, void* context, void* buffer, /* size_t */ IntPtr size)
	{
		SKAbstractManagedStream? stream = null;
		try {
			stream = GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnPeek ((IntPtr)buffer, size);
		} catch {
			if (stream != null)
				Interlocked.Exchange (ref stream.hasError, 1);
			return IntPtr.Zero;
		}
	}

	private static partial /* size_t */
		IntPtr SKManagedStreamReadProxyImplementation (IntPtr s, void* context, void* buffer, /* size_t */ IntPtr size)
	{
		SKAbstractManagedStream? stream = null;
		try {
			stream = GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			return stream.OnRead ((IntPtr)buffer, size);
		} catch {
			if (stream != null)
				Interlocked.Exchange (ref stream.hasError, 1);
			return IntPtr.Zero;
		}
	}

	private static partial byte SKManagedStreamRewindProxyImplementation (IntPtr s, void* context)
	{
		SKAbstractManagedStream? stream = null;
		try {
			stream = GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			if (stream.OnRewind ()) {
				Interlocked.Exchange (ref stream.hasError, 0);
				return 1;
			}
			return 0;
		} catch {
			if (stream != null)
				Interlocked.Exchange (ref stream.hasError, 1);
			return 0;
		}
	}

	private static partial byte SKManagedStreamSeekProxyImplementation (IntPtr s, void* context, /* size_t */
		IntPtr position)
	{
		SKAbstractManagedStream? stream = null;
		try {
			stream = GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
			if (stream.OnSeek (position)) {
				Interlocked.Exchange (ref stream.hasError, 0);
				return 1;
			}
			return 0;
		} catch {
			if (stream != null)
				Interlocked.Exchange (ref stream.hasError, 1);
			return 0;
		}
	}
}
