#nullable disable

using System;
using System.Collections.Generic;

namespace SkiaSharp
{
	/// <summary>Provides a bounded, least-recently-used cache of Graphite-backed textures keyed by source image, for reuse across recordings.</summary>
	/// <remarks><format type="text/markdown"><![CDATA[
	/// ## Remarks
	///
	/// Use this cache as the callback body for <xref:SkiaSharp.SKGraphiteContext.CreateRecorder(System.Int64,SkiaSharp.SKGraphiteFindOrCreateImageDelegate,System.Action)> so that repeatedly drawn images are uploaded to the GPU only once. The cache holds a reference to each cached texture until the entry is evicted or the cache is disposed.
	///
	/// This type implements `IDisposable`. Dispose it while its owning recorder is still alive so the cached textures can be released safely.
	/// ]]></format></remarks>
	public sealed class SKGraphiteImageCache : IDisposable
	{
		// LRU cap — keeps memory bounded when callers decode fresh SkImages
		// per frame instead of reusing the same SkImage object. Without a cap,
		// scrolling unbounded content would eventually OOM the GPU.
		private const int MaxCacheEntries = 256;

		// Cache entries hold a +1 native ref on the graphite-backed image. On
		// lookup we bump the ref again (Skia consumes one when we return) and
		// hand back a fresh wrapper. Cache refs are released either on eviction
		// or on cache disposal.
		private readonly object cacheLock = new object ();
		private readonly LinkedList<(uint UniqueId, bool Mipmapped)> lruOrder = new ();
		private readonly Dictionary<(uint UniqueId, bool Mipmapped), (LinkedListNode<(uint UniqueId, bool Mipmapped)> node, IntPtr handle)> cache = new ();

		/// <summary>Returns the cached Graphite-backed texture for the specified image, uploading and caching it on the recorder if it is not already present.</summary>
		/// <param name="recorder">The recorder to upload the image to on a cache miss.</param>
		/// <param name="image">The source image to find or upload.</param>
		/// <param name="mipmapped"><see langword="true" /> to create the texture with mipmaps; otherwise, <see langword="false" />.</param>
		/// <returns>A Graphite-backed image, or <see langword="null" /> if the image could not be uploaded.</returns>
		/// <remarks />
		public SKImage FindOrCreate (SKGraphiteRecorder recorder, SKImage image, bool mipmapped)
		{
			if (recorder is null)
				throw new ArgumentNullException (nameof (recorder));
			if (image is null)
				throw new ArgumentNullException (nameof (image));

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

		/// <summary>Releases all cached textures held by the cache.</summary>
		/// <remarks />
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
