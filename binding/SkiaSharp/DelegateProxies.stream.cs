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
} catch (Exception ex) {
ReportCallbackException (ex, nameof (SKManagedStreamDuplicateProxyImplementation));
return IntPtr.Zero;
}
}

private static partial IntPtr SKManagedStreamForkProxyImplementation (IntPtr s, void* context)
{
try {
var stream = GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
return stream.OnFork ();
} catch (Exception ex) {
ReportCallbackException (ex, nameof (SKManagedStreamForkProxyImplementation));
return IntPtr.Zero;
}
}

private static partial /* size_t */ IntPtr SKManagedStreamGetLengthProxyImplementation (IntPtr s, void* context)
{
try {
var stream = GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
return stream.OnGetLength ();
} catch (Exception ex) {
ReportCallbackException (ex, nameof (SKManagedStreamGetLengthProxyImplementation));
return IntPtr.Zero;
}
}

private static partial /* size_t */ IntPtr SKManagedStreamGetPositionProxyImplementation (IntPtr s, void* context)
{
try {
var stream = GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
return stream.OnGetPosition ();
} catch (Exception ex) {
ReportCallbackException (ex, nameof (SKManagedStreamGetPositionProxyImplementation));
return IntPtr.Zero;
}
}

private static partial bool SKManagedStreamHasLengthProxyImplementation (IntPtr s, void* context)
{
try {
var stream = GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
return stream.OnHasLength ();
} catch (Exception ex) {
ReportCallbackException (ex, nameof (SKManagedStreamHasLengthProxyImplementation));
return false;
}
}

private static partial void SKManagedStreamDestroyProxyImplementation (IntPtr s, void* context)
{
var stream = GetUserData<SKAbstractManagedStream> ((IntPtr)context, out var gch);
try {
if (stream != null) {
Interlocked.Exchange (ref stream.fromNative, 1);
stream.Dispose ();
}
} catch (Exception ex) {
ReportCallbackException (ex, nameof (SKManagedStreamDestroyProxyImplementation));
} finally {
gch.Free ();
}
}

private static partial bool SKManagedStreamHasPositionProxyImplementation (IntPtr s, void* context)
{
try {
var stream = GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
return stream.OnHasPosition ();
} catch (Exception ex) {
ReportCallbackException (ex, nameof (SKManagedStreamHasPositionProxyImplementation));
return false;
}
}

private static partial bool SKManagedStreamIsAtEndProxyImplementation (IntPtr s, void* context)
{
try {
var stream = GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
return stream.OnIsAtEnd ();
} catch (Exception ex) {
ReportCallbackException (ex, nameof (SKManagedStreamIsAtEndProxyImplementation));
// Signal end-of-stream so native stops probing instead of looping.
return true;
}
}

private static partial bool SKManagedStreamMoveProxyImplementation (IntPtr s, void* context, int offset)
{
try {
var stream = GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
return stream.OnMove (offset);
} catch (Exception ex) {
ReportCallbackException (ex, nameof (SKManagedStreamMoveProxyImplementation));
return false;
}
}

private static partial /* size_t */
IntPtr SKManagedStreamPeekProxyImplementation (IntPtr s, void* context, void* buffer, /* size_t */ IntPtr size)
{
try {
var stream = GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
return stream.OnPeek ((IntPtr)buffer, size);
} catch (Exception ex) {
ReportCallbackException (ex, nameof (SKManagedStreamPeekProxyImplementation));
return IntPtr.Zero;
}
}

private static partial /* size_t */
IntPtr SKManagedStreamReadProxyImplementation (IntPtr s, void* context, void* buffer, /* size_t */ IntPtr size)
{
try {
var stream = GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
return stream.OnRead ((IntPtr)buffer, size);
} catch (Exception ex) {
ReportCallbackException (ex, nameof (SKManagedStreamReadProxyImplementation));
return IntPtr.Zero;
}
}

private static partial bool SKManagedStreamRewindProxyImplementation (IntPtr s, void* context)
{
try {
var stream = GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
return stream.OnRewind ();
} catch (Exception ex) {
ReportCallbackException (ex, nameof (SKManagedStreamRewindProxyImplementation));
return false;
}
}

private static partial bool SKManagedStreamSeekProxyImplementation (IntPtr s, void* context, /* size_t */
IntPtr position)
{
try {
var stream = GetUserData<SKAbstractManagedStream> ((IntPtr)context, out _);
return stream.OnSeek (position);
} catch (Exception ex) {
ReportCallbackException (ex, nameof (SKManagedStreamSeekProxyImplementation));
return false;
}
}
}
