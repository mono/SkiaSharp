using System;
using System.Runtime.InteropServices;
using System.Threading;

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
		internal delegate IntPtr CreateNewDelegate (IntPtr s, IntPtr context);
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
			public CreateNewDelegate fCreateNew;
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
				fCreateNew = CreateNewInternal,
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
			DelegateProxies.Create (this, out _, out var ctx);
			Handle = SkiaApi.sk_managedstream_new (ctx);
		}

		protected override void Dispose (bool disposing)
		{
			if (Interlocked.CompareExchange (ref fromNative, 0, 0) == 0 && Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_managedstream_destroy (Handle);
			}

			base.Dispose (disposing);
		}

		protected abstract IntPtr OnRead (IntPtr buffer, IntPtr size);

		protected abstract IntPtr OnPeek (IntPtr buffer, IntPtr size);

		protected abstract bool OnIsAtEnd ();

		protected abstract bool OnHasPosition ();

		protected abstract bool OnHasLength ();

		protected abstract bool OnRewind ();

		protected abstract IntPtr OnGetPosition ();

		protected abstract IntPtr OnGetLength ();

		protected abstract bool OnSeek (IntPtr position);

		protected abstract bool OnMove (int offset);

		protected abstract IntPtr OnCreateNew ();

		[MonoPInvokeCallback (typeof (ReadDelegate))]
		private static IntPtr ReadInternal (IntPtr s, IntPtr context, IntPtr buffer, IntPtr size)
		{
			var stream = DelegateProxies.Get<SKAbstractManagedStream> (context, out _);
			return stream.OnRead (buffer, size);
		}

		[MonoPInvokeCallback (typeof (PeekDelegate))]
		private static IntPtr PeekInternal (IntPtr s, IntPtr context, IntPtr buffer, IntPtr size)
		{
			var stream = DelegateProxies.Get<SKAbstractManagedStream> (context, out _);
			return stream.OnPeek (buffer, size);
		}

		[MonoPInvokeCallback (typeof (IsAtEndDelegate))]
		private static bool IsAtEndInternal (IntPtr s, IntPtr context)
		{
			var stream = DelegateProxies.Get<SKAbstractManagedStream> (context, out _);
			return stream.OnIsAtEnd ();
		}

		[MonoPInvokeCallback (typeof (HasPositionDelegate))]
		private static bool HasPositionInternal (IntPtr s, IntPtr context)
		{
			var stream = DelegateProxies.Get<SKAbstractManagedStream> (context, out _);
			return stream.OnHasPosition ();
		}

		[MonoPInvokeCallback (typeof (HasLengthDelegate))]
		private static bool HasLengthInternal (IntPtr s, IntPtr context)
		{
			var stream = DelegateProxies.Get<SKAbstractManagedStream> (context, out _);
			return stream.OnHasLength ();
		}

		[MonoPInvokeCallback (typeof (RewindDelegate))]
		private static bool RewindInternal (IntPtr s, IntPtr context)
		{
			var stream = DelegateProxies.Get<SKAbstractManagedStream> (context, out _);
			return stream.OnRewind ();
		}

		[MonoPInvokeCallback (typeof (GetPositionDelegate))]
		private static IntPtr GetPositionInternal (IntPtr s, IntPtr context)
		{
			var stream = DelegateProxies.Get<SKAbstractManagedStream> (context, out _);
			return stream.OnGetPosition ();
		}

		[MonoPInvokeCallback (typeof (SeekDelegate))]
		private static bool SeekInternal (IntPtr s, IntPtr context, IntPtr position)
		{
			var stream = DelegateProxies.Get<SKAbstractManagedStream> (context, out _);
			return stream.OnSeek (position);
		}

		[MonoPInvokeCallback (typeof (MoveDelegate))]
		private static bool MoveInternal (IntPtr s, IntPtr context, int offset)
		{
			var stream = DelegateProxies.Get<SKAbstractManagedStream> (context, out _);
			return stream.OnMove (offset);
		}

		[MonoPInvokeCallback (typeof (GetLengthDelegate))]
		private static IntPtr GetLengthInternal (IntPtr s, IntPtr context)
		{
			var stream = DelegateProxies.Get<SKAbstractManagedStream> (context, out _);
			return stream.OnGetLength ();
		}

		[MonoPInvokeCallback (typeof (CreateNewDelegate))]
		private static IntPtr CreateNewInternal (IntPtr s, IntPtr context)
		{
			var stream = DelegateProxies.Get<SKAbstractManagedStream> (context, out _);
			return stream.OnCreateNew ();
		}

		[MonoPInvokeCallback (typeof (DestroyDelegate))]
		private static void DestroyInternal (IntPtr s, IntPtr context)
		{
			var stream = DelegateProxies.Get<SKAbstractManagedStream> (context, out var gch);
			Interlocked.Exchange (ref stream.fromNative, 1);
			gch.Free ();
			stream.Dispose ();
		}
	}
}
