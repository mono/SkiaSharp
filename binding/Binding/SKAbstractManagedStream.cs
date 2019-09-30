using System;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using NativePointerDictionary = System.Collections.Concurrent.ConcurrentDictionary<System.IntPtr, SkiaSharp.SKAbstractManagedStream>;

namespace SkiaSharp
{
	public abstract class SKAbstractManagedStream : SKStreamAsset
	{
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		internal delegate IntPtr ReadDelegate (IntPtr s, IntPtr context, IntPtr buffer, IntPtr size);
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		internal delegate IntPtr PeekDelegate (IntPtr s, IntPtr context, IntPtr buffer, IntPtr size);
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal delegate bool IsAtEndDelegate (IntPtr s, IntPtr context);
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal delegate bool HasPositionDelegate (IntPtr s, IntPtr context);
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal delegate bool HasLengthDelegate (IntPtr s, IntPtr context);
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal delegate bool RewindDelegate (IntPtr s, IntPtr context);
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		internal delegate IntPtr GetPositionDelegate (IntPtr s, IntPtr context);
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal delegate bool SeekDelegate (IntPtr s, IntPtr context, IntPtr position);
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal delegate bool MoveDelegate (IntPtr s, IntPtr context, int offset);
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		internal delegate IntPtr GetLengthDelegate (IntPtr s, IntPtr context);
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		internal delegate IntPtr DuplicateDelegate (IntPtr s, IntPtr context);
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		internal delegate IntPtr ForkDelegate (IntPtr s, IntPtr context);
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		internal delegate void DestroyDelegate (IntPtr s, IntPtr context);

		[StructLayout (LayoutKind.Sequential)]
		internal struct Procs
		{
			public ReadDelegate fRead;
			public PeekDelegate fPeek;
			public IsAtEndDelegate fIsAtEnd;
			public HasPositionDelegate fHasPosition;
			public HasLengthDelegate fHasLength;
			public RewindDelegate fRewind;
			public GetPositionDelegate fGetPosition;
			public SeekDelegate fSeek;
			public MoveDelegate fMove;
			public GetLengthDelegate fGetLength;
			public DuplicateDelegate fDuplicate;
			public ForkDelegate fFork;
			public DestroyDelegate fDestroy;
		}

		private static readonly Procs delegates;

		private int fromNative;

		static SKAbstractManagedStream ()
		{
			delegates = new Procs {
				fRead = ReadInternal,
				fPeek = PeekInternal,
				fIsAtEnd = IsAtEndInternal,
				fHasPosition = HasPositionInternal,
				fHasLength = HasLengthInternal,
				fRewind = RewindInternal,
				fGetPosition = GetPositionInternal,
				fSeek = SeekInternal,
				fMove = MoveInternal,
				fGetLength = GetLengthInternal,
				fDuplicate = DuplicateInternal,
				fFork = ForkInternal,
				fDestroy = DestroyInternal,
			};
			SkiaApi.sk_managedstream_set_procs (delegates);
		}

		protected SKAbstractManagedStream ()
			: this (true)
		{
		}

		protected SKAbstractManagedStream (bool owns)
			: base (IntPtr.Zero, owns)
		{
			var ctx = DelegateProxies.CreateUserData (this, true);
			Handle = SkiaApi.sk_managedstream_new (ctx);
		}

		protected override void DisposeNative ()
		{
			if (Interlocked.CompareExchange (ref fromNative, 0, 0) == 0)
				SkiaApi.sk_managedstream_destroy (Handle);
		}

		protected abstract IntPtr OnRead (IntPtr buffer, IntPtr size);

		protected internal abstract IntPtr OnPeek (IntPtr buffer, IntPtr size);

		protected internal abstract bool OnIsAtEnd ();

		protected internal abstract bool OnHasPosition ();

		protected internal abstract bool OnHasLength ();

		protected internal abstract bool OnRewind ();

		protected internal abstract IntPtr OnGetPosition ();

		protected internal abstract IntPtr OnGetLength ();

		protected internal abstract bool OnSeek (IntPtr position);

		protected internal abstract bool OnMove (int offset);

		protected internal abstract IntPtr OnCreateNew ();

		protected virtual IntPtr OnFork ()
		{
			var stream = OnCreateNew ();
			SkiaApi.sk_stream_seek (stream, SkiaApi.sk_stream_get_position (Handle));
			return stream;
		}

		protected virtual IntPtr OnDuplicate () => OnCreateNew ();

		[MonoPInvokeCallback (typeof (ReadDelegate))]
		private static IntPtr ReadInternal (IntPtr s, IntPtr context, IntPtr buffer, IntPtr size)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> (context, out _);
			return stream.OnRead (buffer, size);
		}

		[MonoPInvokeCallback (typeof (PeekDelegate))]
		private static IntPtr PeekInternal (IntPtr s, IntPtr context, IntPtr buffer, IntPtr size)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> (context, out _);
			return stream.OnPeek (buffer, size);
		}

		[MonoPInvokeCallback (typeof (IsAtEndDelegate))]
		private static bool IsAtEndInternal (IntPtr s, IntPtr context)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> (context, out _);
			return stream.OnIsAtEnd ();
		}

		[MonoPInvokeCallback (typeof (HasPositionDelegate))]
		private static bool HasPositionInternal (IntPtr s, IntPtr context)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> (context, out _);
			return stream.OnHasPosition ();
		}

		[MonoPInvokeCallback (typeof (HasLengthDelegate))]
		private static bool HasLengthInternal (IntPtr s, IntPtr context)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> (context, out _);
			return stream.OnHasLength ();
		}

		[MonoPInvokeCallback (typeof (RewindDelegate))]
		private static bool RewindInternal (IntPtr s, IntPtr context)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> (context, out _);
			return stream.OnRewind ();
		}

		[MonoPInvokeCallback (typeof (GetPositionDelegate))]
		private static IntPtr GetPositionInternal (IntPtr s, IntPtr context)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> (context, out _);
			return stream.OnGetPosition ();
		}

		[MonoPInvokeCallback (typeof (SeekDelegate))]
		private static bool SeekInternal (IntPtr s, IntPtr context, IntPtr position)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> (context, out _);
			return stream.OnSeek (position);
		}

		[MonoPInvokeCallback (typeof (MoveDelegate))]
		private static bool MoveInternal (IntPtr s, IntPtr context, int offset)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> (context, out _);
			return stream.OnMove (offset);
		}

		[MonoPInvokeCallback (typeof (GetLengthDelegate))]
		private static IntPtr GetLengthInternal (IntPtr s, IntPtr context)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> (context, out _);
			return stream.OnGetLength ();
		}

		[MonoPInvokeCallback (typeof (DuplicateDelegate))]
		private static IntPtr DuplicateInternal (IntPtr s, IntPtr context)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> (context, out _);
			return stream.OnDuplicate ();
		}

		[MonoPInvokeCallback (typeof (ForkDelegate))]
		private static IntPtr ForkInternal (IntPtr s, IntPtr context)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> (context, out _);
			return stream.OnFork ();
		}

		[MonoPInvokeCallback (typeof (DestroyDelegate))]
		private static void DestroyInternal (IntPtr s, IntPtr context)
		{
			var stream = DelegateProxies.GetUserData<SKAbstractManagedStream> (context, out var gch);
			if (stream != null) {
				Interlocked.Exchange (ref stream.fromNative, 1);
				stream.Dispose ();
			}
			gch.Free ();
		}
	}

	public static class ManagedStreamHelper
	{
		public static int ReadInternal (int managedStreamPtr, int buffer, int size)
		{
			var ret = (int)SKAbstractManagedStream.AsManagedStream ((IntPtr)managedStreamPtr).OnRead ((IntPtr)buffer, (IntPtr)size);
			return ret;
		}

		public static int PeekInternal (int managedStreamPtr, int buffer, int size)
		{
			var ret = (int)SKAbstractManagedStream.AsManagedStream ((IntPtr)managedStreamPtr).OnPeek ((IntPtr)buffer, (IntPtr)size);
			return ret;
		}

		public static bool IsAtEndInternal (int managedStreamPtr)
		{
			var ret = SKAbstractManagedStream.AsManagedStream ((IntPtr)managedStreamPtr).OnIsAtEnd ();
			return ret;
		}

		public static bool HasPositionInternal (int managedStreamPtr)
		{
			var ret = SKAbstractManagedStream.AsManagedStream ((IntPtr)managedStreamPtr).OnHasPosition ();
			return ret;
		}

		public static bool HasLengthInternal (int managedStreamPtr)
		{
			var ret = SKAbstractManagedStream.AsManagedStream ((IntPtr)managedStreamPtr).OnHasLength ();
			return ret;
		}

		public static bool RewindInternal (int managedStreamPtr)
		{
			var ret = SKAbstractManagedStream.AsManagedStream ((IntPtr)managedStreamPtr).OnRewind ();
			return ret;
		}

		public static int GetPositionInternal (int managedStreamPtr)
		{
			var ret = (int)SKAbstractManagedStream.AsManagedStream ((IntPtr)managedStreamPtr).OnGetPosition ();
			return ret;
		}

		public static bool SeekInternal (int managedStreamPtr, int position)
		{
			var ret = SKAbstractManagedStream.AsManagedStream ((IntPtr)managedStreamPtr).OnSeek ((IntPtr)position);
			return ret;
		}

		public static bool MoveInternal (int managedStreamPtr, int offset)
		{
			var ret = SKAbstractManagedStream.AsManagedStream ((IntPtr)managedStreamPtr).OnMove (offset);
			return ret;
		}

		public static int GetLengthInternal (int managedStreamPtr)
		{
			var ret = (int)SKAbstractManagedStream.AsManagedStream ((IntPtr)managedStreamPtr).OnGetLength ();
			return ret;
		}

		public static int CreateNewInternal (int managedStreamPtr)
		{
			var ret = (int)SKAbstractManagedStream.AsManagedStream ((IntPtr)managedStreamPtr).OnCreateNew ();
			return ret;
		}

		public static void DestroyInternal (int managedStreamPtr)
		{
			if (SKAbstractManagedStream.AsManagedStream ((IntPtr)managedStreamPtr, out var managedStream)) {
				managedStream.DisposeFromNative ();
			}
		}
	}
}
