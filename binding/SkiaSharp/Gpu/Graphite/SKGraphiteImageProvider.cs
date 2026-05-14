#nullable disable

using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
#if THROW_OBJECT_EXCEPTIONS
	using GCHandle = SkiaSharp.GCHandleProxy;
#endif

	/// <summary>
	/// Bridges Graphite's "non-Graphite SkImage → Graphite-backed SkImage" hook to a
	/// managed override. Construct an instance and pass it to
	/// <see cref="SKGraphiteContext.CreateRecorder(long, SKGraphiteImageProvider)"/>.
	/// The recorder will call <see cref="FindOrCreate"/> for every raster/lazy
	/// SkImage it encounters in a draw.
	///
	/// LIFETIME: one provider per recorder. On a successful CreateRecorder call,
	/// ownership transfers to the recorder; <see cref="IDisposable.Dispose"/> on the
	/// original wrapper becomes a no-op afterwards. Do NOT share a single
	/// SKGraphiteImageProvider between multiple recorders — construct a fresh
	/// provider per recorder (see <see cref="CreateDefault"/>).
	///
	/// THREADING: <see cref="FindOrCreate"/> runs on the recorder's owning thread,
	/// not necessarily the thread that constructed the provider. Implementations
	/// must be thread-safe with respect to any caches they maintain.
	///
	/// EXCEPTIONS: never throw out of <see cref="FindOrCreate"/>. Exceptions are
	/// caught at the FFI boundary and converted to a null return (draw dropped).
	/// </summary>
	public unsafe abstract class SKGraphiteImageProvider : IDisposable
	{
		// Internal: signature the proxy expects. Returning IntPtr.Zero drops the draw.
		internal delegate IntPtr FindOrCreateDelegate (IntPtr recorderHandle, IntPtr imageHandle, bool mipmapped);

		private FindOrCreateDelegate proxyDelegate;
		private GCHandle delegateHandle;
		private IntPtr nativeProvider;
		private bool ownershipTransferred;

		protected SKGraphiteImageProvider ()
		{
			// Capture a closure over `this` so the unmanaged callback ends up at the
			// virtual override. We allocate the GCHandle eagerly in the constructor
			// rather than lazily so SKGraphiteContextOptions can read Handle without
			// side effects.
			proxyDelegate = (recorderHandle, imageHandle, mipmapped) => InvokeFindOrCreate (recorderHandle, imageHandle, mipmapped);
			DelegateProxies.Create (proxyDelegate, out delegateHandle, out var ctx);

#if USE_LIBRARY_IMPORT
			nativeProvider = SkiaApi.sk_graphite_image_provider_new (
				(delegate* unmanaged[Cdecl] <void*, IntPtr, IntPtr, Int32, IntPtr>)
					DelegateProxies.SKGraphiteImageProviderProxy,
				(void*)ctx);
#else
			nativeProvider = SkiaApi.sk_graphite_image_provider_new (
				DelegateProxies.SKGraphiteImageProviderProxy,
				(void*)ctx);
#endif
			if (nativeProvider == IntPtr.Zero) {
				if (delegateHandle.IsAllocated)
					delegateHandle.Free ();
				throw new InvalidOperationException ("sk_graphite_image_provider_new failed (Graphite not built into libSkiaSharp?)");
			}
		}

		/// <summary>
		/// Convert <paramref name="image"/> to a Graphite-backed image suitable for
		/// drawing on <paramref name="recorder"/>, or return null to drop the draw.
		/// The returned SKImage's reference is consumed by Skia.
		///
		/// The provider returned by <see cref="CreateDefault"/> uploads each
		/// unique image once and caches the result keyed on
		/// <c>image.UniqueId</c>. Subclass this base when you need a different
		/// caching policy.
		/// </summary>
		public abstract SKImage FindOrCreate (SKGraphiteRecorder recorder, SKImage image, bool mipmapped);

		private IntPtr InvokeFindOrCreate (IntPtr recorderHandle, IntPtr imageHandle, bool mipmapped)
		{
			if (recorderHandle == IntPtr.Zero || imageHandle == IntPtr.Zero)
				return IntPtr.Zero;
			// The recorder/image handles belong to Skia for the duration of this call.
			// Wrap them in non-owning managed views — pass unrefExisting:false so we
			// don't accidentally decrement the live ref on a wrapper that's already
			// in HandleDictionary. (The 2-arg / 3-arg GetOrAddObject overloads pass
			// unrefExisting:true, which is correct for fresh +1-ref handles coming
			// out of a Skia factory but wrong for borrowed handles like these.)
			var recorder = SKObject.GetOrAddObject<SKGraphiteRecorder> (recorderHandle, false, false, (h, o) => new SKGraphiteRecorder (h, o));
			var image    = SKObject.GetOrAddObject<SKImage>            (imageHandle,    false, false, (h, o) => new SKImage (h, o));
			var result   = FindOrCreate (recorder, image, mipmapped);
			if (result == null) return IntPtr.Zero;
			// Skia consumes the +1 reference on `result`. Detach the managed wrapper
			// from the underlying native handle so Dispose/finalize does NOT call
			// sk_image_unref (which would decrement the reference Skia just took).
			var raw = result.Handle;
			result.RevokeOwnership (null);
			return raw;
		}

		internal IntPtr Handle => nativeProvider;

		// Called by SKGraphiteContext after a successful CreateXxx — at that point
		// the native context wrapper consumed the sk_graphite_image_provider_t* and
		// took its own ref on the underlying ImageProvider. Our wrapper (and the
		// GCHandle pinning the user's delegate) must stay alive for as long as the
		// context lives, so we DON'T free the GCHandle here.
		internal void TransferOwnership () => ownershipTransferred = true;

		// Called by SKGraphiteRecorder.DisposeNative BEFORE the native recorder is
		// destroyed. Cached graphite-backed SkImages reference recorder-allocated
		// GPU resources; releasing them while the recorder is alive is safe,
		// releasing them after destruction is undefined behavior.
		internal void DrainCacheBeforeRecorderDispose ()
		{
			DisposeCachedResources ();
		}

		// Called by SKGraphiteRecorder.DisposeNative AFTER the native recorder has
		// been destroyed (so the FfiImageProvider's destructor has run and won't
		// dispatch into our delegate again). Frees the GCHandle pinning the user's
		// FindOrCreate delegate.
		internal void FreeAfterContextDispose ()
		{
			if (delegateHandle.IsAllocated) {
				delegateHandle.Free ();
				delegateHandle = default;
			}
		}

		/// <summary>
		/// Hook for subclasses to release any cached graphite-backed images, GPU
		/// resources, or other state when the provider is disposed (typically when
		/// the owning recorder is disposed).
		/// </summary>
		protected virtual void DisposeCachedResources ()
		{
		}

		public void Dispose ()
		{
			DisposeCachedResources ();

			if (ownershipTransferred) {
				// Context owns the native side now; freeing it here would double-free.
				// The GCHandle stays pinned until the context is disposed, which
				// will call FreeAfterContextDispose for us.
				GC.SuppressFinalize (this);
				return;
			}

			if (nativeProvider != IntPtr.Zero) {
				SkiaApi.sk_graphite_image_provider_delete (nativeProvider);
				nativeProvider = IntPtr.Zero;
			}
			if (delegateHandle.IsAllocated) {
				delegateHandle.Free ();
				delegateHandle = default;
			}
			GC.SuppressFinalize (this);
		}

		/// <summary>
		/// Construct a fresh default provider: uploads each unique source SkImage
		/// to a Graphite-backed texture exactly once, LRU-cached at 256 entries.
		/// Cache lifetime is the provider's lifetime — typically the recorder's
		/// lifetime via <see cref="SKGraphiteContext.CreateRecorder(long, SKGraphiteImageProvider)"/>.
		///
		/// Sufficient for most apps. Subclass <see cref="SKGraphiteImageProvider"/>
		/// if you need a different eviction policy or want to coordinate with your
		/// own decode pipeline.
		/// </summary>
		public static SKGraphiteImageProvider CreateDefault () => new DefaultProvider ();

		private sealed class DefaultProvider : SKGraphiteImageProvider
		{
			// LRU cap — keeps memory bounded when callers (like Uno) decode fresh
			// SkImages per frame instead of reusing the same SkImage object. Without
			// a cap, scrolling unbounded content would eventually OOM the GPU.
			private const int MaxCacheEntries = 256;

			// Cache entries hold a +1 native ref on the graphite-backed image. On
			// lookup we bump the ref again (Skia consumes one when we return) and
			// hand back a fresh wrapper. Cache refs are released either on eviction
			// or on provider disposal.
			//
			// Keys pack (uniqueId : 32, mipmapped : 1) into a long.
			private readonly object _cacheLock = new object ();
			private readonly System.Collections.Generic.LinkedList<long> _lruOrder = new ();
			private readonly System.Collections.Generic.Dictionary<long, (System.Collections.Generic.LinkedListNode<long> node, IntPtr handle)> _cache = new ();

			public override SKImage FindOrCreate (SKGraphiteRecorder recorder, SKImage image, bool mipmapped)
			{
				long key = ((long)image.UniqueId << 1) | (mipmapped ? 1L : 0L);

				lock (_cacheLock) {
					if (_cache.TryGetValue (key, out var entry)) {
						// Hit — promote to MRU, bump native ref, return fresh wrapper.
						_lruOrder.Remove (entry.node);
						_lruOrder.AddFirst (entry.node);
						SkiaApi.sk_refcnt_safe_ref (entry.handle);
						return SKImage.GetObject (entry.handle);
					}
				}

				// Miss — upload outside the lock so concurrent draws don't serialize.
				var uploaded = SKImage.ToTextureImage (recorder, image, mipmapped);
				if (uploaded == null) return null;

				lock (_cacheLock) {
					if (_cache.TryGetValue (key, out var winner)) {
						// Lost the upload race. Drop ours, return a wrapper to the winner.
						uploaded.Dispose ();
						_lruOrder.Remove (winner.node);
						_lruOrder.AddFirst (winner.node);
						SkiaApi.sk_refcnt_safe_ref (winner.handle);
						return SKImage.GetObject (winner.handle);
					}

					// Evict LRU entries until we're under cap. Each eviction drops one
					// ref; if Skia is still drawing with the evicted image it has its
					// own ref so the underlying resource stays alive.
					while (_cache.Count >= MaxCacheEntries && _lruOrder.Last is { } oldestNode) {
						_lruOrder.RemoveLast ();
						if (_cache.TryGetValue (oldestNode.Value, out var oldEntry)) {
							_cache.Remove (oldestNode.Value);
							SkiaApi.sk_refcnt_safe_unref (oldEntry.handle);
						}
					}

					// Take a +1 to keep the entry alive in the cache. Return the local
					// wrapper, which owns its own +1 from ToTextureImage — Skia consumes
					// that one after the draw. Cache holds the second.
					var rawHandle = uploaded.Handle;
					SkiaApi.sk_refcnt_safe_ref (rawHandle);
					var node = _lruOrder.AddFirst (key);
					_cache[key] = (node, rawHandle);
					return uploaded;
				}
			}

			protected override void DisposeCachedResources ()
			{
				lock (_cacheLock) {
					foreach (var entry in _cache.Values) {
						SkiaApi.sk_refcnt_safe_unref (entry.handle);
					}
					_cache.Clear ();
					_lruOrder.Clear ();
				}
			}
		}
	}
}
