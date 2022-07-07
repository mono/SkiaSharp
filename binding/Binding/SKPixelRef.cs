using System;
using System.Threading;

namespace SkiaSharp
{
	/// <summary>
	/// This class is the smart container for pixel memory, and is used with SkBitmap.
	/// <br></br> This class can be shared/accessed between multiple threads.
	/// </summary>
	public unsafe class SKPixelRef : SKObject, ISKSkipObjectRegistration
	{

		// this is not meant to be anything but a GC reference to keep the actual pixel data alive
		internal SKObject bitmapSource;

		private static readonly SKPixelRefDelegates delegates;
		private readonly IntPtr userData;
		private int fromNative;

		static SKPixelRef()
		{
			delegates = new SKPixelRefDelegates
			{
				fDestroy = DestroyInternal
			};

			SkiaApi.sk_managed_pixel_ref_set_procs(delegates);
		}

		internal unsafe SKPixelRef (void* nativePixelRef)
			: base (IntPtr.Zero, true)
		{
			userData = DelegateProxies.CreateUserData (this, true);
			Handle = SkiaApi.sk_managed_pixel_ref_new_from_existing ((void*)userData, nativePixelRef);

			if (Handle == IntPtr.Zero)
				throw new InvalidOperationException ("Unable to create a new SKPixelRef instance.");
		}

		public SKPixelRef (int width, int height, IntPtr addr, IntPtr rowBytes)
			: base(IntPtr.Zero, true)
		{
			userData = DelegateProxies.CreateUserData(this, true);
			Handle = SkiaApi.sk_managed_pixel_ref_new((void*)userData, width, height, (void*)addr, rowBytes);

			if (Handle == IntPtr.Zero)
				throw new InvalidOperationException("Unable to create a new SKPixelRef instance.");
		}

		protected override void Dispose(bool disposing) =>
			base.Dispose(disposing);

		protected override void DisposeNative()
		{
			if (Interlocked.CompareExchange(ref fromNative, 0, 0) == 0)
			{
				SkiaApi.sk_managed_pixel_ref_delete (Handle);
			}
		}

		[MonoPInvokeCallback(typeof(SKPixelRefDestroyProxyDelegate))]
		private static void DestroyInternal(IntPtr s, void* context)
		{
			var id = DelegateProxies.GetUserData<SKPixelRef>((IntPtr)context, out var gch);
			if (id != null)
			{
				Interlocked.Exchange(ref id.fromNative, 1);
				id.Dispose();
			}
			gch.Free();
		}

		public SKSizeI Dimensions => SkiaApi.sk_managed_pixel_ref_dimensions(Handle);
		public int Width => SkiaApi.sk_managed_pixel_ref_width(Handle);
		public int Height => SkiaApi.sk_managed_pixel_ref_height(Handle);
		public IntPtr Pixels => (IntPtr)SkiaApi.sk_managed_pixel_ref_pixels(Handle);
		public IntPtr SkPixelRefHandle => (IntPtr)SkiaApi.sk_managed_pixel_ref_pixel_ref(Handle);
		public IntPtr RowBytes => SkiaApi.sk_managed_pixel_ref_rowBytes(Handle);

		/// <summary>
		/// Returns a non-zero, unique value corresponding to the pixels in this
		/// <br></br> pixelref. Each time the pixels are changed (and NotifyPixelsChanged is
		/// <br></br> called), a different generation ID will be returned.
		/// </summary>
		public uint GenerationID => SkiaApi.sk_managed_pixel_ref_generation_id(Handle);

		/// <summary>
		/// Call this if you have changed the contents of the pixels. This will in-
		/// <br></br> turn cause a different generation ID value to be returned from
		/// <br></br> GenerationID.
		/// </summary>
		public void NotifyPixelsChanged()
		{
			SkiaApi.sk_managed_pixel_ref_notify_pixels_changed(Handle);
		}

		/// <summary>
		/// Returns true if this pixelref is marked as immutable, meaning that the
		/// <br></br> contents of its pixels will not change for the lifetime of the pixelref.
		/// </summary>
		public bool IsImmutable => SkiaApi.sk_managed_pixel_ref_is_immutable(Handle);

		/// <summary>
		/// Marks this pixelref is immutable, meaning that the contents of its
		/// <br></br> pixels will not change for the lifetime of the pixelref. This state can
		/// <br></br> be set on a pixelref, but it cannot be cleared once it is set.
		/// </summary>
		public void SetImmutable()
		{
			SkiaApi.sk_managed_pixel_ref_set_immutable(Handle);
		}

		/// <summary>
		/// Register a listener that may be called the next time our generation ID changes.
		/// <br></br>
		/// <br></br> We'll only call the listener if we're confident that we are the only SkPixelRef with this
		/// <br></br> generation ID.  If our generation ID changes and we decide not to call the listener, we'll
		/// <br></br> never call it: you must add a new listener for each generation ID change.  We also won't call
		/// <br></br> the listener when we're certain no one knows what our generation ID is.
		/// <br></br>
		/// <br></br> This can be used to invalidate caches keyed by SkPixelRef generation ID.
		/// <br></br> Takes ownership of listener.  Threadsafe.
		/// </summary>
		public void AddGenIDChangeListener(SKIDChangeListener listener)
		{
			SkiaApi.sk_managed_pixel_ref_add_generation_id_listener(Handle, listener.Handle);
		}

		/// <summary>
		/// Call when this pixelref is part of the key to a resourcecache entry. This allows the cache
		/// <br></br> to know automatically those entries can be purged when this pixelref is changed or deleted.
		/// </summary>
		public void NotifyAddedToCache()
		{
			SkiaApi.sk_managed_pixel_ref_notify_added_to_cache(Handle);
		}
	}
}
