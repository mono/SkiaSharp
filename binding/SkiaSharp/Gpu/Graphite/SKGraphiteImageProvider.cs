#nullable disable

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
#if THROW_OBJECT_EXCEPTIONS
	using GCHandle = SkiaSharp.GCHandleProxy;
#endif

	/// <summary>
	/// Convert <paramref name="image"/> (raster or lazy) to a Graphite-backed image suitable for
	/// drawing on <paramref name="recorder"/>, or return null to drop the draw. The returned
	/// SKImage's reference is consumed by Skia.
	/// </summary>
	public delegate SKImage SKGraphiteFindOrCreateImageDelegate (SKGraphiteRecorder recorder, SKImage image, bool mipmapped);

	/// <summary>
	/// Bridges Graphite's non-Graphite-SkImage → Graphite-backed-SkImage hook to a managed
	/// callback. Pass an instance to
	/// <see cref="SKGraphiteContext.CreateRecorder(long, SKGraphiteImageProvider)"/>.
	///
	/// LIFETIME: one provider per recorder. Ownership transfers to the recorder on success.
	/// Do NOT share between recorders — construct a fresh one each time (see <see cref="CreateDefault"/>).
	///
	/// THREADING: the callback runs on the recorder's owning thread, not necessarily the
	/// thread that constructed the provider — implementations must be thread-safe around
	/// any caches they maintain.
	///
	/// EXCEPTIONS: never throw out of the callback; the FFI boundary catches and converts
	/// to a null return (draw dropped).
	/// </summary>
	public sealed unsafe class SKGraphiteImageProvider : IDisposable
	{
		// Internal: signature the proxy expects. Returning IntPtr.Zero drops the draw.
		internal delegate IntPtr FindOrCreateProxy (IntPtr recorderHandle, IntPtr imageHandle, bool mipmapped);

		private readonly SKGraphiteFindOrCreateImageDelegate findOrCreate;
		private readonly Action onDispose;
		private FindOrCreateProxy proxyDelegate;
		private GCHandle delegateHandle;
		private IntPtr nativeProvider;
		private bool ownershipTransferred;

		/// <summary>
		/// Build a provider from a callback. <paramref name="onDispose"/>, if supplied, runs
		/// once when the provider is disposed (typically together with the owning recorder)
		/// — use it to release any cached resources the callback closure built up.
		/// </summary>
		public SKGraphiteImageProvider (SKGraphiteFindOrCreateImageDelegate findOrCreate, Action onDispose = null)
		{
			this.findOrCreate = findOrCreate ?? throw new ArgumentNullException (nameof (findOrCreate));
			this.onDispose = onDispose;

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
			var result   = findOrCreate (recorder, image, mipmapped);
			if (result == null) return IntPtr.Zero;
			// Skia consumes the +1 reference on `result`. Detach the managed wrapper
			// from the underlying native handle so Dispose/finalize does NOT call
			// sk_image_unref (which would decrement the reference Skia just took).
			var raw = result.Handle;
			result.RevokeOwnership (null);
			return raw;
		}

		internal IntPtr Handle => nativeProvider;

		// Called by SKGraphiteContext after a successful CreateXxx — the recorder took its
		// own ref on the underlying ImageProvider. Our wrapper (and the GCHandle pinning
		// the user's delegate) must stay alive for as long as the recorder lives, so we
		// DON'T free the GCHandle here.
		internal void TransferOwnership () => ownershipTransferred = true;

		// Called by SKGraphiteRecorder.DisposeNative BEFORE the native recorder is
		// destroyed. Cached graphite-backed SkImages reference recorder-allocated
		// GPU resources; releasing them while the recorder is alive is safe,
		// releasing them after destruction is undefined behavior.
		internal void DrainCacheBeforeRecorderDispose ()
		{
			onDispose?.Invoke ();
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

		public void Dispose ()
		{
			if (ownershipTransferred) {
				// Recorder owns the native side now; freeing it here would double-free.
				// The GCHandle stays pinned until the recorder is disposed, which
				// will call onDispose + FreeAfterContextDispose for us.
				GC.SuppressFinalize (this);
				return;
			}

			onDispose?.Invoke ();
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
		/// A fresh default provider: uploads each unique source SkImage to a Graphite-backed
		/// texture exactly once, LRU-cached at 256 entries. Construct a custom provider via
		/// <see cref="SKGraphiteImageProvider(SKGraphiteFindOrCreateImageDelegate, Action)"/>
		/// for a different eviction policy.
		/// </summary>
		public static SKGraphiteImageProvider CreateDefault ()
		{
			var cache = new DefaultCache ();
			return new SKGraphiteImageProvider (cache.FindOrCreate, cache.Dispose);
		}

		private sealed class DefaultCache
		{
			// LRU cap — keeps memory bounded when callers (like Uno) decode fresh
			// SkImages per frame instead of reusing the same SkImage object. Without
			// a cap, scrolling unbounded content would eventually OOM the GPU.
			private const int MaxCacheEntries = 256;

			// Cache entries hold a +1 native ref on the graphite-backed image. On
			// lookup we bump the ref again (Skia consumes one when we return) and
			// hand back a fresh wrapper. Cache refs are released either on eviction
			// or on provider disposal.
			private readonly object _cacheLock = new object ();
			private readonly LinkedList<(uint UniqueId, bool Mipmapped)> _lruOrder = new ();
			private readonly Dictionary<(uint UniqueId, bool Mipmapped), (LinkedListNode<(uint UniqueId, bool Mipmapped)> node, IntPtr handle)> _cache = new ();

			public SKImage FindOrCreate (SKGraphiteRecorder recorder, SKImage image, bool mipmapped)
			{
				var key = (image.UniqueId, mipmapped);

				// Hold the lock across the upload so concurrent misses for the same key
				// don't each pay a redundant ToTextureImage. FindOrCreate is expected to
				// run on the recorder's owning thread, so contention with unrelated keys
				// should be rare in practice.
				lock (_cacheLock) {
					if (_cache.TryGetValue (key, out var entry)) {
						// Hit — promote to MRU, bump native ref, return fresh wrapper.
						_lruOrder.Remove (entry.node);
						_lruOrder.AddFirst (entry.node);
						SkiaApi.sk_refcnt_safe_ref (entry.handle);
						return SKImage.GetObject (entry.handle);
					}

					var uploaded = SKImage.ToTextureImage (recorder, image, mipmapped);
					if (uploaded == null) return null;

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

			public void Dispose ()
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
