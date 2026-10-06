using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_managedwstream_procs_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct SKManagedWStreamDelegates : IEquatable<SKManagedWStreamDelegates> {
		// public sk_managedwstream_write_proc fWrite
#if USE_LIBRARY_IMPORT
		public delegate* unmanaged[Cdecl] <sk_wstream_managedstream_t, void*, void*, /* size_t */ IntPtr, bool> fWrite;
#else
		public SKManagedWStreamWriteProxyDelegate fWrite;
#endif

		// public sk_managedwstream_flush_proc fFlush
#if USE_LIBRARY_IMPORT
		public delegate* unmanaged[Cdecl] <sk_wstream_managedstream_t, void*, void> fFlush;
#else
		public SKManagedWStreamFlushProxyDelegate fFlush;
#endif

		// public sk_managedwstream_bytesWritten_proc fBytesWritten
#if USE_LIBRARY_IMPORT
		public delegate* unmanaged[Cdecl] <sk_wstream_managedstream_t, void*, /* size_t */ IntPtr> fBytesWritten;
#else
		public SKManagedWStreamBytesWrittenProxyDelegate fBytesWritten;
#endif

		// public sk_managedwstream_destroy_proc fDestroy
#if USE_LIBRARY_IMPORT
		public delegate* unmanaged[Cdecl] <sk_wstream_managedstream_t, void*, void> fDestroy;
#else
		public SKManagedWStreamDestroyProxyDelegate fDestroy;
#endif

		public readonly bool Equals (SKManagedWStreamDelegates obj) =>
#pragma warning disable CS8909
			fWrite == obj.fWrite && fFlush == obj.fFlush && fBytesWritten == obj.fBytesWritten && fDestroy == obj.fDestroy;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKManagedWStreamDelegates f && Equals (f);

		public static bool operator == (SKManagedWStreamDelegates left, SKManagedWStreamDelegates right) =>
			left.Equals (right);

		public static bool operator != (SKManagedWStreamDelegates left, SKManagedWStreamDelegates right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fWrite);
			hash.Add (fFlush);
			hash.Add (fBytesWritten);
			hash.Add (fDestroy);
			return hash.ToHashCode ();
		}

	}
}
