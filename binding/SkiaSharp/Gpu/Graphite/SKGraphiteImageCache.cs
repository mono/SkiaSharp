#nullable disable

using System;
using System.Collections.Generic;

namespace SkiaSharp
{
	/// <summary>
	/// Upload-once + LRU cache to back
	/// <see cref="SKGraphiteContext.CreateRecorder(long, SKGraphiteFindOrCreateImageDelegate, Action)"/>.
	/// Construct one, pass <see cref="FindOrCreate"/> and <see cref="Dispose"/> to the recorder.
	/// One cache per recorder — cached graphite-backed images reference resources owned by
	/// that recorder and become invalid when the recorder is destroyed.
	/// </summary>
	public sealed class SKGraphiteImageCache : IDisposable
	{
		// LRU cap — keeps memory bounded when callers (like Uno) decode fresh
		// SkImages per frame instead of reusing the same SkImage object. Without
		// a cap, scrolling unbounded content would eventually OOM the GPU.
		private const int MaxCacheEntries = 256;

		// Cache entries hold a +1 native ref on the graphite-backed image. On
		// lookup we bump the ref again (Skia consumes one when we return) and
		// hand back a fresh wrapper. Cache refs are released either on eviction
		// or on cache disposal.
		private readonly object cacheLock = new object ();
		private readonly LinkedList<(uint UniqueId, bool Mipmapped)> lruOrder = new ();
		private readonly Dictionary<(uint UniqueId, bool Mipmapped), (LinkedListNode<(uint UniqueId, bool Mipmapped)> node, IntPtr handle)> cache = new ();

		/// <summary>
		/// Matches the <see cref="SKGraphiteFindOrCreateImageDelegate"/> signature; pass directly
		/// to <see cref="SKGraphiteContext.CreateRecorder(long, SKGraphiteFindOrCreateImageDelegate, Action)"/>.
		/// </summary>
		public SKImage FindOrCreate (SKGraphiteRecorder recorder, SKImage image, bool mipmapped)
		{
			var key = (image.UniqueId, mipmapped);

			// Hold the lock across the upload so concurrent misses for the same key
			// don't each pay a redundant ToTextureImage. FindOrCreate is expected to
			// run on the recorder's owning thread, so contention with unrelated keys
			// should be rare in practice.
			lock (cacheLock) {
				if (cache.TryGetValue (key, out var entry)) {
					// Hit — promote to MRU, bump native ref, return fresh wrapper.
					lruOrder.Remove (entry.node);
					lruOrder.AddFirst (entry.node);
					SkiaApi.sk_refcnt_safe_ref (entry.handle);
					return SKImage.GetObject (entry.handle);
				}

				var uploaded = SKImage.ToTextureImage (recorder, image, mipmapped);
				if (uploaded == null) return null;

				// Evict LRU entries until we're under cap. Each eviction drops one
				// ref; if Skia is still drawing with the evicted image it has its
				// own ref so the underlying resource stays alive.
				while (cache.Count >= MaxCacheEntries && lruOrder.Last is { } oldestNode) {
					lruOrder.RemoveLast ();
					if (cache.TryGetValue (oldestNode.Value, out var oldEntry)) {
						cache.Remove (oldestNode.Value);
						SkiaApi.sk_refcnt_safe_unref (oldEntry.handle);
					}
				}

				// Take a +1 to keep the entry alive in the cache. Return the local
				// wrapper, which owns its own +1 from ToTextureImage — Skia consumes
				// that one after the draw. Cache holds the second.
				var rawHandle = uploaded.Handle;
				SkiaApi.sk_refcnt_safe_ref (rawHandle);
				var node = lruOrder.AddFirst (key);
				cache[key] = (node, rawHandle);
				return uploaded;
			}
		}

		/// <summary>
		/// Release every cached graphite-backed image. Must run before the owning recorder
		/// is destroyed — typically wired via the <c>onDispose</c> argument to
		/// <see cref="SKGraphiteContext.CreateRecorder(long, SKGraphiteFindOrCreateImageDelegate, Action)"/>.
		/// Safe to call multiple times.
		/// </summary>
		public void Dispose ()
		{
			lock (cacheLock) {
				foreach (var entry in cache.Values) {
					SkiaApi.sk_refcnt_safe_unref (entry.handle);
				}
				cache.Clear ();
				lruOrder.Clear ();
			}
		}
	}
}
