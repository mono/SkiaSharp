using System;
using System.Threading;
using Xunit;

namespace SkiaSharp.Tests
{
	// xUnit runs tests from different classes in parallel by default. The tests
	// below assert on process-wide GC.CollectionCount; without serialization,
	// allocations in other test classes can pollute those counts. Putting this
	// class in a collection with DisableParallelization=true makes it run
	// without any other test class concurrent with it.
	[CollectionDefinition(NativeMemoryPressureCollection.Name, DisableParallelization = true)]
	public class NativeMemoryPressureCollection
	{
		public const string Name = "Native memory pressure";
	}

	[Collection(NativeMemoryPressureCollection.Name)]
	public class SKNativeMemoryPressureMonitorTest : SKTest
	{
		// When SKNativeMemoryPressureMonitor is enabled, the native allocator's
		// threshold callback drives GC.AddMemoryPressure, so allocating ~400 MB
		// of native pixel buffers should prompt the GC to collect at least once.
		// When the monitor is disabled, the same allocation is invisible to the
		// GC and no collection is triggered.
		[SkippableTheory]
		[InlineData(false)]
		[InlineData(true)]
		public void GarbageCollectionFiresOnlyWhenNativeMemoryPressureMonitorIsEnabled(bool monitorEnabled)
		{
			SkipOnMono();

			// Start each run from a known state.
			SKNativeMemoryPressureMonitor.Stop();

			const int BitmapCount = 100;
			const int Dimension = 1024;
			const int BytesPerPixel = 4;
			const long ExpectedUnmanagedBytes = (long)BitmapCount * Dimension * Dimension * BytesPerPixel;

			var bitmaps = new System.Collections.Generic.List<SKBitmap>(BitmapCount);
			try
			{
				if (monitorEnabled)
				{
					// Small threshold so the callback fires repeatedly during
					// the loop, not just at the end.
					SKNativeMemoryPressureMonitor.Start(thresholdBytes: 1024 * 1024);
				}

				CollectGarbage();
				var baselineGen0 = GC.CollectionCount(0);
				var baselineGen1 = GC.CollectionCount(1);
				var baselineGen2 = GC.CollectionCount(2);

				for (var i = 0; i < BitmapCount; i++)
					bitmaps.Add(new SKBitmap(Dimension, Dimension, SKColorType.Rgba8888, SKAlphaType.Premul));

				if (monitorEnabled)
				{
					// The native push is async (ThreadPool work item). Wait
					// for it to drain so AddMemoryPressure has nudged the GC.
					var deadline = DateTime.UtcNow.AddSeconds(2);
					while (SKNativeMemoryPressureMonitor.ReportedPressure < ExpectedUnmanagedBytes
						&& DateTime.UtcNow < deadline)
					{
						Thread.Yield();
					}
				}

				var totalCollections = (GC.CollectionCount(0) - baselineGen0)
					+ (GC.CollectionCount(1) - baselineGen1)
					+ (GC.CollectionCount(2) - baselineGen2);

				if (monitorEnabled)
				{
					Assert.True(totalCollections > 0,
						$"With the monitor running, ~{ExpectedUnmanagedBytes / 1024 / 1024} MB of " +
						$"native pressure was reported (reported={SKNativeMemoryPressureMonitor.ReportedPressure:N0} " +
						$"bytes) but the GC did not collect (Gen0+1+2 deltas summed to 0).");
				}
				else
				{
					Assert.Equal(0, totalCollections);
				}

				GC.KeepAlive(bitmaps);
			}
			finally
			{
				foreach (var bmp in bitmaps)
					bmp.Dispose();
				SKNativeMemoryPressureMonitor.Stop();
			}
		}
	}
}
