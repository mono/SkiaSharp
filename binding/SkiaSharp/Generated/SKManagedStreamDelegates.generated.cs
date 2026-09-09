using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_managedstream_procs_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct SKManagedStreamDelegates : IEquatable<SKManagedStreamDelegates> {
		// public sk_managedstream_read_proc fRead
#if USE_LIBRARY_IMPORT
		public delegate* unmanaged[Cdecl] <IntPtr, void*, void*, /* size_t */ IntPtr, /* size_t */ IntPtr> fRead;
#else
		public SKManagedStreamReadProxyDelegate fRead;
#endif

		// public sk_managedstream_peek_proc fPeek
#if USE_LIBRARY_IMPORT
		public delegate* unmanaged[Cdecl] <IntPtr, void*, void*, /* size_t */ IntPtr, /* size_t */ IntPtr> fPeek;
#else
		public SKManagedStreamPeekProxyDelegate fPeek;
#endif

		// public sk_managedstream_isAtEnd_proc fIsAtEnd
#if USE_LIBRARY_IMPORT
		public delegate* unmanaged[Cdecl] <IntPtr, void*, bool> fIsAtEnd;
#else
		public SKManagedStreamIsAtEndProxyDelegate fIsAtEnd;
#endif

		// public sk_managedstream_hasPosition_proc fHasPosition
#if USE_LIBRARY_IMPORT
		public delegate* unmanaged[Cdecl] <IntPtr, void*, bool> fHasPosition;
#else
		public SKManagedStreamHasPositionProxyDelegate fHasPosition;
#endif

		// public sk_managedstream_hasLength_proc fHasLength
#if USE_LIBRARY_IMPORT
		public delegate* unmanaged[Cdecl] <IntPtr, void*, bool> fHasLength;
#else
		public SKManagedStreamHasLengthProxyDelegate fHasLength;
#endif

		// public sk_managedstream_rewind_proc fRewind
#if USE_LIBRARY_IMPORT
		public delegate* unmanaged[Cdecl] <IntPtr, void*, bool> fRewind;
#else
		public SKManagedStreamRewindProxyDelegate fRewind;
#endif

		// public sk_managedstream_getPosition_proc fGetPosition
#if USE_LIBRARY_IMPORT
		public delegate* unmanaged[Cdecl] <IntPtr, void*, /* size_t */ IntPtr> fGetPosition;
#else
		public SKManagedStreamGetPositionProxyDelegate fGetPosition;
#endif

		// public sk_managedstream_seek_proc fSeek
#if USE_LIBRARY_IMPORT
		public delegate* unmanaged[Cdecl] <IntPtr, void*, /* size_t */ IntPtr, bool> fSeek;
#else
		public SKManagedStreamSeekProxyDelegate fSeek;
#endif

		// public sk_managedstream_move_proc fMove
#if USE_LIBRARY_IMPORT
		public delegate* unmanaged[Cdecl] <IntPtr, void*, Int32, bool> fMove;
#else
		public SKManagedStreamMoveProxyDelegate fMove;
#endif

		// public sk_managedstream_getLength_proc fGetLength
#if USE_LIBRARY_IMPORT
		public delegate* unmanaged[Cdecl] <IntPtr, void*, /* size_t */ IntPtr> fGetLength;
#else
		public SKManagedStreamGetLengthProxyDelegate fGetLength;
#endif

		// public sk_managedstream_duplicate_proc fDuplicate
#if USE_LIBRARY_IMPORT
		public delegate* unmanaged[Cdecl] <IntPtr, void*, IntPtr> fDuplicate;
#else
		public SKManagedStreamDuplicateProxyDelegate fDuplicate;
#endif

		// public sk_managedstream_fork_proc fFork
#if USE_LIBRARY_IMPORT
		public delegate* unmanaged[Cdecl] <IntPtr, void*, IntPtr> fFork;
#else
		public SKManagedStreamForkProxyDelegate fFork;
#endif

		// public sk_managedstream_destroy_proc fDestroy
#if USE_LIBRARY_IMPORT
		public delegate* unmanaged[Cdecl] <IntPtr, void*, void> fDestroy;
#else
		public SKManagedStreamDestroyProxyDelegate fDestroy;
#endif

		public readonly bool Equals (SKManagedStreamDelegates obj) =>
#pragma warning disable CS8909
			fRead == obj.fRead && fPeek == obj.fPeek && fIsAtEnd == obj.fIsAtEnd && fHasPosition == obj.fHasPosition && fHasLength == obj.fHasLength && fRewind == obj.fRewind && fGetPosition == obj.fGetPosition && fSeek == obj.fSeek && fMove == obj.fMove && fGetLength == obj.fGetLength && fDuplicate == obj.fDuplicate && fFork == obj.fFork && fDestroy == obj.fDestroy;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKManagedStreamDelegates f && Equals (f);

		public static bool operator == (SKManagedStreamDelegates left, SKManagedStreamDelegates right) =>
			left.Equals (right);

		public static bool operator != (SKManagedStreamDelegates left, SKManagedStreamDelegates right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fRead);
			hash.Add (fPeek);
			hash.Add (fIsAtEnd);
			hash.Add (fHasPosition);
			hash.Add (fHasLength);
			hash.Add (fRewind);
			hash.Add (fGetPosition);
			hash.Add (fSeek);
			hash.Add (fMove);
			hash.Add (fGetLength);
			hash.Add (fDuplicate);
			hash.Add (fFork);
			hash.Add (fDestroy);
			return hash.ToHashCode ();
		}

	}
}
