#nullable disable
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Xunit;

namespace SkiaSharp.Tests
{
	// Regression guard for the issue #3817 / PR #4080 native use-after-free.
	//
	// A native wrapper passed to a native call must stay alive for the duration of that
	// call. `SKPathMeasure(path)` reads `path.Handle` and then calls
	// sk_pathmeasure_new_with_path(...). If nothing keeps `path` rooted across the
	// P/Invoke, a concurrent GC can collect + FINALIZE it mid-call, freeing the native
	// SkPath while the native ctor is still reading it (SkPath::isFinite) -> use-after-free
	// -> the process is killed by an AccessViolation.
	//
	// This test forces that race with a GC/finalizer hammer thread. On a correct build
	// (the ctor keeps `path` alive) it runs to completion and passes. On a regressed build
	// it crashes the test host within the time budget below.
	//
	// It lives in the serialized HandleDictionary threading collection so its GC hammer
	// does not perturb tests running in parallel.
	[Collection (HandleDictionaryThreadingCollection.Name)]
	public class Pr4080UseAfterFreeTest : SKTest
	{
		// Long enough to land the race on a regressed build, short enough for CI on a fixed one.
		private static readonly TimeSpan Budget = TimeSpan.FromSeconds (4);

		[SkippableFact]
		public void PathMeasureCtorKeepsPathAliveAcrossNativeCall ()
		{
			var stop = 0;

			// Continuously collect and drain finalizers so any wrapper that becomes unrooted
			// mid-native-call is freed immediately, deterministically exposing a missing KeepAlive.
			var hammer = new Thread (() => {
				while (Volatile.Read (ref stop) == 0) {
					GC.Collect ();
					GC.WaitForPendingFinalizers ();
				}
			}) {
				IsBackground = true,
				Name = "gc-hammer",
			};
			hammer.Start ();

			try {
				var sw = Stopwatch.StartNew ();
				while (sw.Elapsed < Budget) {
					// The argument is a temporary with no other root: once its Handle is read
					// inside the ctor it is collectible, so the hammer thread can finalize it
					// (freeing the native SkPath) while the native ctor is still reading it -
					// unless the ctor keeps it alive.
					for (var i = 0; i < 200; i++)
						(new SKPathMeasure (MakeDoomedPath ())).Dispose ();
				}
			} finally {
				Volatile.Write (ref stop, 1);
				hammer.Join ();
			}

			// Reaching here means the wrapper survived every native call: no use-after-free.
			Assert.True (true);
		}

		[MethodImpl (MethodImplOptions.NoInlining)]
		private static SKPath MakeDoomedPath ()
		{
			var p = new SKPath ();

			// A non-trivial path makes the native ctor (SkContourMeasureIter::reset ->
			// SkPath::isFinite) spend longer reading the path, widening the use-after-free
			// window so a regression is caught quickly.
			for (var i = 0; i < 1500; i++)
				p.LineTo (i, (i & 1) == 0 ? 0 : 10);

			return p;
		}
	}
}
