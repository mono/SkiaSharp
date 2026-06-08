#nullable disable
// ReSharper disable InconsistentNaming

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
// public delegates

public delegate void SKBitmapReleaseDelegate (IntPtr address, object context);

public delegate void SKDataReleaseDelegate (IntPtr address, object context);

public delegate void SKImageRasterReleaseDelegate (IntPtr pixels, object context);

public delegate void SKImageTextureReleaseDelegate (object context);

public delegate void SKSurfaceReleaseDelegate (IntPtr address, object context);

public delegate IntPtr GRGlGetProcedureAddressDelegate (string name);

public delegate IntPtr GRVkGetProcedureAddressDelegate (string name, IntPtr instance, IntPtr device);

public delegate void SKGlyphPathDelegate (SKPath path, SKMatrix matrix);

internal static unsafe partial class DelegateProxies
{
// internal proxy implementations

private static partial void SKBitmapReleaseProxyImplementation (void* addr, void* context)
{
var del = Get<SKBitmapReleaseDelegate> ((IntPtr)context, out var gch);
try {
del.Invoke ((IntPtr)addr, null);
} catch (Exception ex) {
ReportCallbackException (ex, nameof (SKBitmapReleaseProxyImplementation));
} finally {
gch.Free ();
}
}

private static partial void SKDataReleaseProxyImplementation (void* ptr, void* context)
{
var del = Get<SKDataReleaseDelegate> ((IntPtr)context, out var gch);
try {
del.Invoke ((IntPtr)ptr, null);
} catch (Exception ex) {
ReportCallbackException (ex, nameof (SKDataReleaseProxyImplementation));
} finally {
gch.Free ();
}
}

private static partial void SKImageRasterReleaseProxyImplementation (void* addr, void* context)
{
var del = Get<SKImageRasterReleaseDelegate> ((IntPtr)context, out var gch);
try {
del.Invoke ((IntPtr)addr, null);
} catch (Exception ex) {
ReportCallbackException (ex, nameof (SKImageRasterReleaseProxyImplementation));
} finally {
gch.Free ();
}
}

private static partial void SKImageTextureReleaseProxyImplementation (void* context)
{
var del = Get<SKImageTextureReleaseDelegate> ((IntPtr)context, out var gch);
try {
del.Invoke (null);
} catch (Exception ex) {
ReportCallbackException (ex, nameof (SKImageTextureReleaseProxyImplementation));
} finally {
gch.Free ();
}
}

private static partial void SKSurfaceRasterReleaseProxyImplementation (void* addr, void* context)
{
var del = Get<SKSurfaceReleaseDelegate> ((IntPtr)context, out var gch);
try {
del.Invoke ((IntPtr)addr, null);
} catch (Exception ex) {
ReportCallbackException (ex, nameof (SKSurfaceRasterReleaseProxyImplementation));
} finally {
gch.Free ();
}
}

private static partial void SKImageRasterReleaseProxyImplementationForCoTaskMem (void* addr, void* context)
{
try {
Marshal.FreeCoTaskMem ((IntPtr)addr);
} catch (Exception ex) {
ReportCallbackException (ex, nameof (SKImageRasterReleaseProxyImplementationForCoTaskMem));
}
}

private static partial IntPtr GRGlGetProcProxyImplementation (void* ctx, void* name)
{
try {
var del = Get<GRGlGetProcedureAddressDelegate> ((IntPtr)ctx, out _);
return del.Invoke (Marshal.PtrToStringAnsi ((IntPtr)name));
} catch (Exception ex) {
ReportCallbackException (ex, nameof (GRGlGetProcProxyImplementation));
return IntPtr.Zero;
}
}

private static partial IntPtr GRVkGetProcProxyImplementation (void* ctx, void* name, IntPtr instance, IntPtr device)
{
try {
var del = Get<GRVkGetProcedureAddressDelegate> ((IntPtr)ctx, out _);
return del.Invoke (Marshal.PtrToStringAnsi ((IntPtr)name), instance, device);
} catch (Exception ex) {
ReportCallbackException (ex, nameof (GRVkGetProcProxyImplementation));
return IntPtr.Zero;
}
}

private static partial void SKGlyphPathProxyImplementation (IntPtr pathOrNull, SKMatrix* matrix, void* context)
{
try {
var del = Get<SKGlyphPathDelegate> ((IntPtr)context, out _);
var path = SKPath.GetObject (pathOrNull, false);
del.Invoke (path, *matrix);
} catch (Exception ex) {
ReportCallbackException (ex, nameof (SKGlyphPathProxyImplementation));
}
}
}
}
