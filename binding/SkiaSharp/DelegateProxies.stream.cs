using System;
using System.Threading;
// ReSharper disable InconsistentNaming

namespace SkiaSharp;

internal static unsafe partial class DelegateProxies
{
	private static partial IntPtr SKManagedStreamDuplicateProxyImplementation (IntPtr s, void* context)
	{
		var stream = GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
		return stream.OnDuplicate ();
	}

	private static partial IntPtr SKManagedStreamForkProxyImplementation (IntPtr s, void* context)
	{
		var stream = GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
		return stream.OnFork ();
	}

	private static partial /* size_t */ IntPtr SKManagedStreamGetLengthProxyImplementation (IntPtr s, void* context)
	{
		var stream = GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
		return stream.OnGetLength ();
	}

	private static partial /* size_t */ IntPtr SKManagedStreamGetPositionProxyImplementation (IntPtr s, void* context)
	{
		var stream = GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
		return stream.OnGetPosition ();
	}

	private static partial bool SKManagedStreamHasLengthProxyImplementation (IntPtr s, void* context)
	{
		var stream = GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
		return stream.OnHasLength ();
	}

	private static partial void SKManagedStreamDestroyProxyImplementation (IntPtr s, void* context)
	{
		var stream = GetUserData<SKAbstractManagedStream> ((IntPtr)context, out var gch);
		if (stream != null) {
			Interlocked.Exchange (ref stream.fromNative, 1);
			stream.Dispose ();
		}
		gch.Free ();
	}

	private static partial bool SKManagedStreamHasPositionProxyImplementation (IntPtr s, void* context)
	{
		var stream = GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
		return stream.OnHasPosition ();
	}

	private static partial bool SKManagedStreamIsAtEndProxyImplementation (IntPtr s, void* context)
	{
		var stream = GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
		return stream.OnIsAtEnd ();
	}

	private static partial bool SKManagedStreamMoveProxyImplementation (IntPtr s, void* context, int offset)
	{
		var stream = GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
		return stream.OnMove (offset);
	}

	private static partial /* size_t */
		IntPtr SKManagedStreamPeekProxyImplementation (IntPtr s, void* context, void* buffer, /* size_t */ IntPtr size)
	{
		var stream = GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
		return stream.OnPeek ((IntPtr)buffer, size);
	}

	private static partial /* size_t */
		IntPtr SKManagedStreamReadProxyImplementation (IntPtr s, void* context, void* buffer, /* size_t */ IntPtr size)
	{

		var stream = GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
		return stream.OnRead ((IntPtr)buffer, size);
	}

	private static partial bool SKManagedStreamRewindProxyImplementation (IntPtr s, void* context)
	{
		var stream = GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
		return stream.OnRewind ();
	}

	private static partial bool SKManagedStreamSeekProxyImplementation (IntPtr s, void* context, /* size_t */
		IntPtr position)
	{
		var stream = GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
		return stream.OnSeek (position);
	}
}
