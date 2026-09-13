#nullable disable

using System;
using System.Threading;

namespace SkiaSharp
{
	/// <summary>Represents a <see cref="T:SkiaSharp.SKStreamAsset" /> (a seekable, rewindable Skia stream).</summary>
	/// <remarks />
	public unsafe abstract class SKAbstractManagedStream : SKStreamAsset
	{
		private static readonly SKManagedStreamDelegates delegates;

		internal int fromNative;

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKAbstractManagedStream" />.</summary>
		/// <remarks />
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

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKAbstractManagedStream" />.</summary>
		/// <remarks />
		protected SKAbstractManagedStream ()
			: this (true)
		{
		}

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKAbstractManagedStream" />.</summary>
		/// <param name="owns">The value indicating whether this object should destroy the underlying native object.</param>
		/// <remarks />
		protected SKAbstractManagedStream (bool owns)
			: base (IntPtr.Zero, owns)
		{
			var ctx = DelegateProxies.CreateUserData (this, true);
			Handle = SkiaApi.sk_managedstream_new ((void*)ctx);
		}

		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.SKAbstractManagedStream" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.SKAbstractManagedStream" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKObject" /> types to destroy any native objects.</summary>
		/// <remarks />
		protected override void DisposeNative ()
		{
			if (Interlocked.CompareExchange (ref fromNative, 0, 0) == 0)
				SkiaApi.sk_managedstream_destroy (Handle);
		}

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedStream" /> types to copy the specified number of bytes into the specified buffer.</summary>
		/// <param name="buffer">The buffer to read into.</param>
		/// <param name="size">The number of bytes to read.</param>
		/// <returns>Returns the number of bytes actually read.</returns>
		/// <remarks />
		protected internal abstract IntPtr OnRead (IntPtr buffer, IntPtr size);

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedStream" /> types to copy the specified number of bytes into the specified buffer.</summary>
		/// <param name="buffer">The buffer to read into.</param>
		/// <param name="size">The number of bytes to read.</param>
		/// <returns>Returns the number of bytes actually peeked/copied.</returns>
		/// <remarks>The stream's cursor must be returned to the position before this method was called.</remarks>
		protected internal abstract IntPtr OnPeek (IntPtr buffer, IntPtr size);

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedStream" /> types to indicate whether all the bytes in the stream have been read.</summary>
		/// <returns>Returns a value indicating whether all the bytes in the stream have been read.</returns>
		/// <remarks />
		protected internal abstract bool OnIsAtEnd ();

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedStream" /> types to indicate whether this stream can report its current position.</summary>
		/// <returns>Returns a value indicating whether this stream can report its current position.</returns>
		/// <remarks />
		protected internal abstract bool OnHasPosition ();

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedStream" /> types to indicate whether this stream can report its total length.</summary>
		/// <returns>Returns a value indicating whether this stream can report its total length.</returns>
		/// <remarks />
		protected internal abstract bool OnHasLength ();

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedStream" /> types to rewind the current stream.</summary>
		/// <returns><see langword="true" /> if the stream is known to be at the beginning after this call returns.</returns>
		/// <remarks />
		protected internal abstract bool OnRewind ();

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedStream" /> types to get the current position in the stream.</summary>
		/// <returns>Returns the current position in the stream.</returns>
		/// <remarks />
		protected internal abstract IntPtr OnGetPosition ();

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedStream" /> types to return the total length of the stream.</summary>
		/// <returns>Returns the total length of the stream.</returns>
		/// <remarks />
		protected internal abstract IntPtr OnGetLength ();

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedStream" /> types to seek to an absolute position.</summary>
		/// <param name="position">The absolute position.</param>
		/// <returns><see langword="true" /> if seeking is supported and the seek was successful; otherwise, <see langword="false" />.</returns>
		/// <remarks>If an attempt is made to move to a position outside the stream, the position must be set to the closest point within the stream (beginning or end).</remarks>
		protected internal abstract bool OnSeek (IntPtr position);

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedStream" /> types to seek to a relative offset.</summary>
		/// <param name="offset">The relative offset.</param>
		/// <returns><see langword="true" /> if seeking is supported and the seek was successful; otherwise, <see langword="false" />.</returns>
		/// <remarks>If an attempt is made to move to a position outside the stream, the position must be set to the closest point within the stream (beginning or end).</remarks>
		protected internal abstract bool OnMove (int offset);

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedStream" /> types to copy the current stream.</summary>
		/// <returns>Returns a pointer to the new <see cref="T:SkiaSharp.SKStreamAsset" /> instance.</returns>
		/// <remarks />
		protected internal abstract IntPtr OnCreateNew ();

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedStream" /> types to fork the current stream.</summary>
		/// <returns>Returns a pointer to the new <see cref="T:SkiaSharp.SKStreamAsset" /> instance.</returns>
		/// <remarks>After the stream has been duplicated, the new stream must set its position to the same as this stream.</remarks>
		protected internal virtual IntPtr OnFork ()
		{
			var stream = OnCreateNew ();
			SkiaApi.sk_stream_seek (stream, SkiaApi.sk_stream_get_position (Handle));
			return stream;
		}

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKAbstractManagedStream" /> types to duplicate the current stream.</summary>
		/// <returns>Returns a pointer to the new <see cref="T:SkiaSharp.SKStreamAsset" /> instance.</returns>
		/// <remarks>After the stream has been duplicated, the new stream must set its position to the start.</remarks>
		protected internal virtual IntPtr OnDuplicate () => OnCreateNew ();
	}
}
