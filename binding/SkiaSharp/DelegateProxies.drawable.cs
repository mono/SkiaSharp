using System;
using System.Threading;
// ReSharper disable InconsistentNaming

namespace SkiaSharp;

internal static unsafe partial class DelegateProxies
{
	private static partial /* size_t */ IntPtr SKManagedDrawableApproximateBytesUsedProxyImplementation (IntPtr d, void* context)
	{
		var drawable = GetUserData<SKDrawable> ((IntPtr)context, out _);
		return (IntPtr)drawable.OnGetApproximateBytesUsed ();
	}

	private static partial void SKManagedDrawableDestroyProxyImplementation (IntPtr d, void* context)
	{
		var drawable = GetUserData<SKDrawable> ((IntPtr)context, out var gch);
		if (drawable != null) {
			Interlocked.Exchange (ref drawable.fromNative, 1);
			drawable.Dispose ();
		}
		gch.Free ();
	}

	private static partial void SKManagedDrawableDrawProxyImplementation (IntPtr d, void* context, IntPtr ccanvas)
	{
		var drawable = GetUserData<SKDrawable> ((IntPtr)context, out _);
		drawable.OnDraw (SKCanvas.GetObject (ccanvas, false));
	}

	private static partial void SKManagedDrawableGetBoundsProxyImplementation (IntPtr d, void* context, SKRect* rect)
	{
		var drawable = GetUserData<SKDrawable> ((IntPtr)context, out _);
		var bounds = drawable.OnGetBounds ();
		*rect = bounds;
	}

	private static partial IntPtr SKManagedDrawableMakePictureSnapshotProxyImplementation (IntPtr d, void* context)
	{
		var drawable = GetUserData<SKDrawable> ((IntPtr)context, out _);
		return drawable.OnSnapshot ()?.Handle ?? IntPtr.Zero;
	}
}
