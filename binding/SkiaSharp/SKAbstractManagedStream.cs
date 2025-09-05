#nullable disable

using System;
using System.Threading;

namespace SkiaSharp
{
	/// <summary>
	/// Represents a <see cref="T:SkiaSharp.SKStreamAsset" /> (a seekable, rewindable Skia stream).
	/// </summary>
	public unsafe abstract class SKAbstractManagedStream : SKStreamAsset
	{
		private static readonly SKManagedStreamDelegates delegates;

		internal int fromNative;

		static SKAbstractManagedStream ()
		{
			delegates = new SKManagedStreamDelegates {
				fRead = DelegateProxies.SKManagedStreamReadProxy,
				fPeek = DelegateProxies.SKManagedStreamPeekProxy,
				fIsAtEnd = DelegateProxies.SKManagedStreamIsAtEndProxy,
				fHasPosition = DelegateProxies.SKManagedStreamHasPositionProxy,
				fHasLength = DelegateProxies.SKManagedStreamHasLengthProxy,
				fRewind = DelegateProxies.SKManagedStreamRewindProxy,
				fGetPosition = DelegateProxies.SKManagedStreamGetPositionProxy,
				fSeek = DelegateProxies.SKManagedStreamSeekProxy,
				fMove = DelegateProxies.SKManagedStreamMoveProxy,
				fGetLength = DelegateProxies.SKManagedStreamGetLengthProxy,
				fDuplicate = DelegateProxies.SKManagedStreamDuplicateProxy,
				fFork = DelegateProxies.SKManagedStreamForkProxy,
				fDestroy = DelegateProxies.SKManagedStreamDestroyProxy,
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
			Handle = SkiaApi.sk_managedstream_new ((void*)ctx);
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative ()
		{
			if (Interlocked.CompareExchange (ref fromNative, 0, 0) == 0)
				SkiaApi.sk_managedstream_destroy (Handle);
		}

		protected internal abstract IntPtr OnRead (IntPtr buffer, IntPtr size);

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

		protected internal virtual IntPtr OnFork ()
		{
			var stream = OnCreateNew ();
			SkiaApi.sk_stream_seek (stream, SkiaApi.sk_stream_get_position (Handle));
			return stream;
		}

		protected internal virtual IntPtr OnDuplicate () => OnCreateNew ();
	}
}
