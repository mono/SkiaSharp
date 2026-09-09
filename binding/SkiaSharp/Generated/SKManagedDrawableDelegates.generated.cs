using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_manageddrawable_procs_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct SKManagedDrawableDelegates : IEquatable<SKManagedDrawableDelegates> {
		// public sk_manageddrawable_draw_proc fDraw
#if USE_LIBRARY_IMPORT
		public delegate* unmanaged[Cdecl] <IntPtr, void*, IntPtr, void> fDraw;
#else
		public SKManagedDrawableDrawProxyDelegate fDraw;
#endif

		// public sk_manageddrawable_getBounds_proc fGetBounds
#if USE_LIBRARY_IMPORT
		public delegate* unmanaged[Cdecl] <IntPtr, void*, SKRect*, void> fGetBounds;
#else
		public SKManagedDrawableGetBoundsProxyDelegate fGetBounds;
#endif

		// public sk_manageddrawable_approximateBytesUsed_proc fApproximateBytesUsed
#if USE_LIBRARY_IMPORT
		public delegate* unmanaged[Cdecl] <IntPtr, void*, /* size_t */ IntPtr> fApproximateBytesUsed;
#else
		public SKManagedDrawableApproximateBytesUsedProxyDelegate fApproximateBytesUsed;
#endif

		// public sk_manageddrawable_makePictureSnapshot_proc fMakePictureSnapshot
#if USE_LIBRARY_IMPORT
		public delegate* unmanaged[Cdecl] <IntPtr, void*, IntPtr> fMakePictureSnapshot;
#else
		public SKManagedDrawableMakePictureSnapshotProxyDelegate fMakePictureSnapshot;
#endif

		// public sk_manageddrawable_destroy_proc fDestroy
#if USE_LIBRARY_IMPORT
		public delegate* unmanaged[Cdecl] <IntPtr, void*, void> fDestroy;
#else
		public SKManagedDrawableDestroyProxyDelegate fDestroy;
#endif

		public readonly bool Equals (SKManagedDrawableDelegates obj) =>
#pragma warning disable CS8909
			fDraw == obj.fDraw && fGetBounds == obj.fGetBounds && fApproximateBytesUsed == obj.fApproximateBytesUsed && fMakePictureSnapshot == obj.fMakePictureSnapshot && fDestroy == obj.fDestroy;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKManagedDrawableDelegates f && Equals (f);

		public static bool operator == (SKManagedDrawableDelegates left, SKManagedDrawableDelegates right) =>
			left.Equals (right);

		public static bool operator != (SKManagedDrawableDelegates left, SKManagedDrawableDelegates right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fDraw);
			hash.Add (fGetBounds);
			hash.Add (fApproximateBytesUsed);
			hash.Add (fMakePictureSnapshot);
			hash.Add (fDestroy);
			return hash.ToHashCode ();
		}

	}
}
