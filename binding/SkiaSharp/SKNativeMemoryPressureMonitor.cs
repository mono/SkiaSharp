#nullable disable

using System;
using System.Threading;

namespace SkiaSharp
{
	/// <summary>
	/// Reports SkiaSharp's outstanding native memory usage to the managed
	/// garbage collector via
	/// <see cref="GC.AddMemoryPressure(long)"/> and
	/// <see cref="GC.RemoveMemoryPressure(long)"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The GC has no native visibility into Skia's pixel buffers, paths,
	/// glyph caches, etc. Without this monitor, a process can hold gigabytes
	/// of native memory through SkiaSharp wrappers without the GC ever
	/// being prompted to reclaim eligible managed wrappers.
	/// </para>
	/// <para>
	/// Implementation: SkiaSharp's native allocator tracks outstanding
	/// bytes in an atomic counter. When the counter drifts past +/- a
	/// configurable threshold since the last notification, the allocator
	/// fires a callback (on the thread that performed the allocation) which
	/// queues a single <see cref="ThreadPool"/> work item that reconciles
	/// the delta with the GC. No background timer, no managed polling.
	/// </para>
	/// <para>
	/// The monitor is opt-in. Enable it by calling <see cref="Start()"/>
	/// (typically at application startup), and disable it with
	/// <see cref="Stop"/>.
	/// </para>
	/// </remarks>
	public static unsafe class SKNativeMemoryPressureMonitor
	{
		/// <summary>
		/// The default threshold (1 MB) used by <see cref="Start()"/>.
		/// </summary>
		public const long DefaultThresholdBytes = 1024 * 1024;

		private static readonly object Sync = new object ();

		private static bool s_running;
		private static long s_reportedPressure;
		private static int s_pendingReconcile;

		/// <summary>
		/// Gets a value indicating whether the monitor is currently
		/// installed and reporting pressure to the GC.
		/// </summary>
		public static bool IsRunning {
			get {
				lock (Sync) {
					return s_running;
				}
			}
		}

		/// <summary>
		/// The number of bytes currently reported to the GC via
		/// <see cref="GC.AddMemoryPressure(long)"/>. Returns to zero after
		/// <see cref="Stop"/>. Provided for diagnostics.
		/// </summary>
		public static long ReportedPressure {
			get {
				lock (Sync) {
					return s_reportedPressure;
				}
			}
		}

		/// <summary>
		/// Installs the monitor with the default 1 MB threshold.
		/// </summary>
		/// <remarks>
		/// Idempotent: calling <see cref="Start()"/> while already running
		/// is a no-op.
		/// </remarks>
		public static void Start () =>
			Start (DefaultThresholdBytes);

		/// <summary>
		/// Installs the monitor with a custom delta threshold. The native
		/// allocator notifies the managed layer each time the outstanding
		/// allocation has changed by at least <paramref name="thresholdBytes"/>
		/// since the last notification.
		/// </summary>
		/// <param name="thresholdBytes">
		/// Minimum signed delta (in bytes) that triggers a notification.
		/// Smaller values report pressure more accurately but fire the
		/// callback more often. Must be positive.
		/// </param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="thresholdBytes"/> is zero or negative.
		/// </exception>
		public static void Start (long thresholdBytes)
		{
			if (thresholdBytes <= 0)
				throw new ArgumentOutOfRangeException (nameof (thresholdBytes), "Threshold must be positive.");

			lock (Sync) {
				if (s_running)
					return;
				SkiaApi.sk_memory_set_threshold_callback (
					DelegateProxies.SKMemoryThresholdProxy, (ulong)thresholdBytes);
				s_running = true;
				// Anchor s_reportedPressure to the current counter so the
				// first callback's delta isn't inflated by allocations that
				// pre-date Start().
				Reconcile ();
			}
		}

		/// <summary>
		/// Detaches the monitor and releases any pressure previously
		/// reported to the GC. Safe to call when the monitor is not running.
		/// </summary>
		public static void Stop ()
		{
			lock (Sync) {
				if (!s_running)
					return;
				// Detach the native callback first so no new fires arrive.
				SkiaApi.sk_memory_set_threshold_callback (null, 0);
				s_running = false;

				// Release any pressure we previously reported. In-flight
				// ThreadPool reconciles that run after Stop() will see
				// s_running == false and refuse to add new pressure.
				if (s_reportedPressure > 0) {
					GC.RemoveMemoryPressure (s_reportedPressure);
					s_reportedPressure = 0;
				}
			}
		}

		// Invoked (indirectly, via DelegateProxies) from the native
		// allocator when the threshold is crossed. Runs on whatever thread
		// did the allocation -- must not take any lock that could be held
		// by the caller, must not call back into Skia, must return
		// immediately.
		internal static void OnNativeThresholdCrossed ()
		{
			// Coalesce: if a reconcile is already pending, drop this fire.
			if (Interlocked.Exchange (ref s_pendingReconcile, 1) == 1)
				return;

			ThreadPool.UnsafeQueueUserWorkItem (static _ => {
				// Clear the pending flag BEFORE reconciling so a fire that
				// arrives mid-reconcile schedules a follow-up tick rather
				// than getting dropped.
				Interlocked.Exchange (ref s_pendingReconcile, 0);
				try {
					lock (Sync) {
						Reconcile ();
					}
				} catch {
					// Never propagate from a ThreadPool work item.
				}
			}, null);
		}

		// Must be called under Sync.
		private static void Reconcile ()
		{
			long current = (long)SkiaApi.sk_memory_get_native_allocated ();
			if (current < 0)
				current = 0;

			long delta = current - s_reportedPressure;
			if (delta == 0)
				return;

			// After Stop() has run, suppress new additions to avoid leaking
			// pressure that nobody will ever release. Removals stay allowed
			// (harmless).
			if (delta > 0 && !s_running)
				return;

			s_reportedPressure = current;

			if (delta > 0)
				GC.AddMemoryPressure (delta);
			else
				GC.RemoveMemoryPressure (-delta);
		}
	}
}
