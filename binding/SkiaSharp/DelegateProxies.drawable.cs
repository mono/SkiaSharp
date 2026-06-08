using System;
using System.Threading;
// ReSharper disable InconsistentNaming

namespace SkiaSharp;

internal static unsafe partial class DelegateProxies
{
private static partial /* size_t */ IntPtr SKManagedDrawableApproximateBytesUsedProxyImplementation (IntPtr d, void* context)
{
try {
var drawable = GetUserData<SKDrawable> ((IntPtr)context, out _);
return (IntPtr)drawable.OnGetApproximateBytesUsed ();
} catch (Exception ex) {
ReportCallbackException (ex, nameof (SKManagedDrawableApproximateBytesUsedProxyImplementation));
return IntPtr.Zero;
}
}

private static partial void SKManagedDrawableDestroyProxyImplementation (IntPtr d, void* context)
{
var drawable = GetUserData<SKDrawable> ((IntPtr)context, out var gch);
try {
if (drawable != null) {
Interlocked.Exchange (ref drawable.fromNative, 1);
drawable.Dispose ();
}
} catch (Exception ex) {
ReportCallbackException (ex, nameof (SKManagedDrawableDestroyProxyImplementation));
} finally {
gch.Free ();
}
}

private static partial void SKManagedDrawableDrawProxyImplementation (IntPtr d, void* context, IntPtr ccanvas)
{
try {
var drawable = GetUserData<SKDrawable> ((IntPtr)context, out _);
drawable.OnDraw (SKCanvas.GetObject (ccanvas, false));
} catch (Exception ex) {
ReportCallbackException (ex, nameof (SKManagedDrawableDrawProxyImplementation));
}
}

private static partial void SKManagedDrawableGetBoundsProxyImplementation (IntPtr d, void* context, SKRect* rect)
{
try {
var drawable = GetUserData<SKDrawable> ((IntPtr)context, out _);
var bounds = drawable.OnGetBounds ();
*rect = bounds;
} catch (Exception ex) {
ReportCallbackException (ex, nameof (SKManagedDrawableGetBoundsProxyImplementation));
// Ensure native never reads an uninitialized rect on the failure path.
*rect = SKRect.Empty;
}
}

private static partial IntPtr SKManagedDrawableMakePictureSnapshotProxyImplementation (IntPtr d, void* context)
{
try {
var drawable = GetUserData<SKDrawable> ((IntPtr)context, out _);
return drawable.OnSnapshot ()?.Handle ?? IntPtr.Zero;
} catch (Exception ex) {
ReportCallbackException (ex, nameof (SKManagedDrawableMakePictureSnapshotProxyImplementation));
return IntPtr.Zero;
}
}
}
