using System;
using System.Threading;
using Xunit;

namespace SkiaSharp.Tests
{
	// xUnit runs tests from different classes in parallel by default. The tests
	// below read SkiaSharp's process-wide native allocation counter; without
	// serialization, allocations and frees in other test classes would move it
	// while they run. Putting this class in a collection with
	// DisableParallelization=true makes it run without any other test class
	// concurrent with it.
	[CollectionDefinition(NativeMemoryPressureCollection.Name, DisableParallelization = true)]
	public class NativeMemoryPressureCollection
	{
		public const string Name = "Native memory pressure";
	}

	[Collection(NativeMemoryPressureCollection.Name)]
	public class SKNativeMemoryPressureMonitorTest : SKTest
	{
		private const int BitmapCount = 16;
		private const int Dimension = 1024;
		private const int BytesPerPixel = 4;
		private const long AllocatedBytes = (long)BitmapCount * Dimension * Dimension * BytesPerPixel;
		private const long ThresholdBytes = 1024 * 1024;

		// The monitor reports through a ThreadPool work item, so its pressure lags the native counter.
		private static readonly TimeSpan ReconcileTimeout = TimeSpan.FromSeconds(5);

		// Whether the GC then collects is the runtime's decision (.NET does not
		// reliably collect on AddMemoryPressure), so these tests check what the
		// monitor reports, not GC.CollectionCount.
		[SkippableFact]
		public void NativeAllocationsAreReportedAndReleasedWhileTheMonitorIsRunning()
		{
			SkipOnMono();

			SKNativeMemoryPressureMonitor.Stop();
			var bitmaps = new System.Collections.Generic.List<SKBitmap>(BitmapCount);
			try
			{
				// Small threshold so the callback fires for every bitmap, not
				// just at the end.
				SKNativeMemoryPressureMonitor.Start(thresholdBytes: ThresholdBytes);
				var before = SKNativeMemoryPressureMonitor.ReportedPressure;

				for (var i = 0; i < BitmapCount; i++)
					bitmaps.Add(new SKBitmap(Dimension, Dimension, SKColorType.Rgba8888, SKAlphaType.Premul));

				Assert.True(
					SpinWait.SpinUntil(() => SKNativeMemoryPressureMonitor.ReportedPressure >= before + AllocatedBytes, ReconcileTimeout),
					$"The monitor reported {SKNativeMemoryPressureMonitor.ReportedPressure:N0} bytes, expected at least " +
					$"{before + AllocatedBytes:N0} after allocating {AllocatedBytes:N0} bytes of pixels.");

				foreach (var bmp in bitmaps)
					bmp.Dispose();
				bitmaps.Clear();

				// Unrelated native allocations below the threshold may not have been reconciled yet.
				Assert.True(
					SpinWait.SpinUntil(() => SKNativeMemoryPressureMonitor.ReportedPressure <= before + ThresholdBytes, ReconcileTimeout),
					$"The monitor still reported {SKNativeMemoryPressureMonitor.ReportedPressure:N0} bytes after the pixels were " +
					$"freed, expected at most {before + ThresholdBytes:N0}.");
			}
			finally
			{
				foreach (var bmp in bitmaps)
					bmp.Dispose();
				SKNativeMemoryPressureMonitor.Stop();
			}

			Assert.Equal(0, SKNativeMemoryPressureMonitor.ReportedPressure);
		}

		[SkippableFact]
		public void NativeAllocationsAreNotReportedWhileTheMonitorIsStopped()
		{
			SkipOnMono();

			SKNativeMemoryPressureMonitor.Stop();
			var bitmaps = new System.Collections.Generic.List<SKBitmap>(BitmapCount);
			try
			{
				var nativeBefore = (long)SkiaApi.sk_memory_get_native_allocated();

				for (var i = 0; i < BitmapCount; i++)
					bitmaps.Add(new SKBitmap(Dimension, Dimension, SKColorType.Rgba8888, SKAlphaType.Premul));

				// The allocations happened and were counted natively...
				Assert.True((long)SkiaApi.sk_memory_get_native_allocated() - nativeBefore >= AllocatedBytes);

				// ...but nothing reached the GC.
				Assert.False(SKNativeMemoryPressureMonitor.IsRunning);
				Assert.Equal(0, SKNativeMemoryPressureMonitor.ReportedPressure);
			}
			finally
			{
				foreach (var bmp in bitmaps)
					bmp.Dispose();
			}
		}
	}
}
