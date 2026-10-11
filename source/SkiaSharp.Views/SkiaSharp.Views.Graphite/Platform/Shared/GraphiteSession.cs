using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SkiaSharp.Views
{
	// Retired sessions are held only by their host and registered recorder callbacks,
	// never by a process-lifetime pool. The host cannot recreate until retirement finishes.
	internal sealed class GraphiteSession
	{
		private readonly int ownerThread = Environment.CurrentManagedThreadId;
		private readonly Action releaseBackend;
		private int externalRecorders;
		private readonly object recordingsLock = new ();
		private readonly ConditionalWeakTable<SKGraphiteRecorder, object> registeredRecorders = new ();
		private readonly List<WeakReference<SKGraphiteRecorder>> recorderLifetimes = new ();
		private readonly HashSet<SKGraphiteRecording> recordings = new ();
		private bool closing;
		private Exception? insertionFailure;
		public GraphiteSession (SKGraphiteContext context, Action releaseBackend)
		{
			Context = context;
			this.releaseBackend = releaseBackend;
			try { Recorder = CreateCachedRecorder (null); }
			catch { context.Dispose (); throw; }
		}
		public SKGraphiteContext Context { get; }
		public SKGraphiteRecorder? Recorder { get; private set; }
		public void ReleasePrimaryRecorder ()
		{
			CheckThread ();
			Recorder?.Dispose ();
			Recorder = null;
		}
		public void RestorePrimaryRecorder ()
		{
			CheckThread ();
			if (closing) throw new ObjectDisposedException ("Graphite view context");
			Recorder ??= CreateCachedRecorder (null);
		}
		public bool IsClosed { get; private set; }
		public bool IsClosing => closing;
		public SKGraphiteRecorder CreateRecorder ()
		{
			CheckThread ();
			if (closing) throw new ObjectDisposedException ("Graphite view context");
			lock (recordingsLock) {
				PruneRecorders ();
				if (recorderLifetimes.Count >= 8)
					throw new InvalidOperationException ("Dispose an existing caller recorder before creating another (limit: 8).");
			}
			Interlocked.Increment (ref externalRecorders);
			try {
				var recorder = CreateCachedRecorder (() => Interlocked.Decrement (ref externalRecorders));
				registeredRecorders.Add (recorder, this);
				// Cache cleanup precedes native recorder destruction. A resurrection-
				// tracking weak reference keeps retirement from observing "done" during
				// that cleanup callback or while a recorder finalizer is still running.
				lock (recordingsLock) recorderLifetimes.Add (new WeakReference<SKGraphiteRecorder> (recorder, true));
				return recorder;
			}
			catch { Interlocked.Decrement (ref externalRecorders); throw; }
		}
		private SKGraphiteRecorder CreateCachedRecorder (Action? disposed)
		{
			var cache = new SKGraphiteImageCache ();
			try {
				return Context.CreateRecorder (64 * 1024 * 1024, cache.FindOrCreate, () => {
					try { cache.Dispose (); }
					finally { disposed?.Invoke (); }
				}) ?? throw new InvalidOperationException ("Graphite recorder creation failed.");
			} catch { cache.Dispose (); throw; }
		}
		public SKGraphiteInsertStatus InsertRecording (SKGraphiteRecording recording)
		{
			CheckThread ();
			if (recording is null) throw new ArgumentNullException (nameof (recording));
			lock (recordingsLock) {
				if (closing) throw new ObjectDisposedException ("Graphite view context");
				if (recording.Handle == IntPtr.Zero) throw new ObjectDisposedException (nameof (recording));
				if (!recordings.Contains (recording))
					throw new InvalidOperationException ("Use SnapRecording with a recorder from this view's current context generation.");
			}
			return InsertPrimaryRecording (recording);
		}
		// This helper may run on a producer's exclusive recorder thread. It never
		// touches the context; registration prevents stale work reaching a new one.
		public SKGraphiteRecording SnapRecording (SKGraphiteRecorder recorder)
		{
			if (recorder is null) throw new ArgumentNullException (nameof (recorder));
			lock (recordingsLock) {
				if (closing) throw new ObjectDisposedException ("Graphite view context");
				if (recorder.Handle == IntPtr.Zero) throw new ObjectDisposedException (nameof (recorder));
				if (!registeredRecorders.TryGetValue (recorder, out _))
					throw new InvalidOperationException ("The recorder belongs to another view or context generation.");
				recordings.RemoveWhere (r => r.Handle == IntPtr.Zero);
				if (recordings.Count >= 32)
					throw new InvalidOperationException ("Dispose an existing caller recording before snapping another (limit: 32).");
				var recording = recorder.Snap () ?? throw new InvalidOperationException ("Graphite recorder snap failed.");
				recordings.Add (recording);
				return recording;
			}
		}
		public SKGraphiteInsertStatus InsertPrimaryRecording (SKGraphiteRecording recording)
		{
			CheckThread ();
			if (closing) throw new ObjectDisposedException ("Graphite view context");
			if (insertionFailure is not null) throw new InvalidOperationException ("The Graphite context must be recreated after insertion failure.", insertionFailure);
			var status = Context.InsertRecording (recording);
			if (status != SKGraphiteInsertStatus.Success) {
				insertionFailure = new InvalidOperationException ($"Graphite recording insertion failed: {status}.");
				throw insertionFailure;
			}
			return status;
		}
		public void Close ()
		{
			CheckThread ();
			if (closing) return;
			lock (recordingsLock) closing = true;
			try {
				if (!Context.IsDeviceLost)
					Context.Submit (new SKGraphiteSubmitInfo { Sync = true });
				Context.CheckAsyncWorkCompletion ();
			} finally {
				ReleasePrimaryRecorder ();
			}
		}
		public bool TryFinishClose ()
		{
			CheckThread ();
			if (IsClosed) return true;
			if (!closing || Volatile.Read (ref externalRecorders) != 0) return false;
			lock (recordingsLock) {
				PruneRecorders ();
				if (recorderLifetimes.Count != 0) return false;
				recordings.RemoveWhere (r => r.Handle == IntPtr.Zero);
				if (recordings.Count != 0) return false;
			}
			try { Context.Dispose (); }
			finally { IsClosed = true; releaseBackend (); }
			return true;
		}
		private void PruneRecorders () => recorderLifetimes.RemoveAll (
			weak => !weak.TryGetTarget (out var recorder) || recorder.Handle == IntPtr.Zero);
		public void CheckThread ()
		{
			if (Environment.CurrentManagedThreadId != ownerThread)
				throw new InvalidOperationException ("Graphite operations require the context owner thread. Use a separate caller-owned recorder for producer work.");
		}
	}
}
