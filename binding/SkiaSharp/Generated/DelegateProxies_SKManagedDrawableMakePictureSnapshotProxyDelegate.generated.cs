using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for sk_manageddrawable_makePictureSnapshot_proc native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <sk_manageddrawable_t, void*, sk_picture_t> SKManagedDrawableMakePictureSnapshotProxy = &SKManagedDrawableMakePictureSnapshotProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly SKManagedDrawableMakePictureSnapshotProxyDelegate SKManagedDrawableMakePictureSnapshotProxy = SKManagedDrawableMakePictureSnapshotProxyImplementation;
	[MonoPInvokeCallback (typeof (SKManagedDrawableMakePictureSnapshotProxyDelegate))]
#endif
	private static partial sk_picture_t SKManagedDrawableMakePictureSnapshotProxyImplementation(sk_manageddrawable_t d,void* context);

	}
}
