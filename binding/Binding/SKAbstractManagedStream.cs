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

#if __WASM__
		[StructLayout (LayoutKind.Sequential)]
		internal struct Procs
		{
			public IntPtr fRead;
			public IntPtr fPeek;
			public IntPtr fIsAtEnd;
			public IntPtr fHasPosition;
			public IntPtr fHasLength;
			public IntPtr fRewind;
			public IntPtr fGetPosition;
			public IntPtr fSeek;
			public IntPtr fMove;
			public IntPtr fGetLength;
			public IntPtr fDuplicate;
			public IntPtr fFork;
			public IntPtr fDestroy;
		}
#else
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
#endif

		private static readonly Procs delegates;

		private int fromNative;

		static SKAbstractManagedStream ()
		{
#if __WASM__
			//
			// Javascript returns the list of registered methods, then the
			// sk_managedstream_set_delegates gets called through P/Invoke
			// because it is not exported when building for AOT.
			//

			var ret = WebAssembly.Runtime.InvokeJS ($"SkiaSharp.SurfaceManager.registerManagedStream();");
			var funcs = ret.Split (new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
				.Select (f => (IntPtr)int.Parse (f, CultureInfo.InvariantCulture))
				.ToArray ();

			if (funcs.Length == 13) {
				delegates = new Procs {
					fRead = funcs[0],
					fPeek = funcs[1],
					fIsAtEnd = funcs[2],
					fHasPosition = funcs[3],
					fHasLength = funcs[4],
					fRewind = funcs[5],
					fGetPosition = funcs[6],
					fSeek = funcs[7],
					fMove = funcs[8],
					fGetLength = funcs[9],
					fDuplicate = funcs[10],
					fFork = funcs[11],
					fDestroy = funcs[12],
				};
			} else {
				throw new InvalidOperationException ($"Mismatch for registerManagedStream returned values (got {funcs.Length}, expected 12)");
			}
#else
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
#endif

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
}
