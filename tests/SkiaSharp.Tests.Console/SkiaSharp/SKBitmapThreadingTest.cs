using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKBitmapThreadingTest : SKTest
	{
		// This test intentionally hammers the GC and finalizer pipeline: each iteration
		// creates SkiaSharp objects (SKBitmap/SKImage/SKData) and deliberately does NOT
		// dispose them, so that they are reclaimed only by the garbage collector and the
		// finalizer thread. The goal is to verify that SkiaSharp's objects can be finalized
		// safely from many threads concurrently without crashing or corrupting native state.
		// Do NOT add `using`/Dispose to ComputeThumbnail - deterministic disposal would
		// short-circuit finalization and defeat the entire purpose of the test.
		[SkippableTheory]
		[InlineData(10, 10)]
		[InlineData(10, 100)]
		[InlineData(100, 1000)]
		public static void ImageScalingMultipleThreadsTest(int numThreads, int numIterationsPerThread)
		{
			// The (100, 1000) variant queues up to 100K undisposed native allocations to stress
			// GC finalizer throughput. On x86 (2GB address space) the finalizer cannot keep up
			// and Skia's native allocator fails ("Unable to allocate pixels"). This is a genuine
			// address-space limit, not a leak, so skip the heaviest combination on x86. The
			// lighter (10, 100) variant still exercises concurrent GC/finalizer behavior. See #3608.
			if (IntPtr.Size == 4 && numThreads >= 100 && numIterationsPerThread >= 1000)
				throw new SkipException("Stress test skipped on x86 due to address space limit.");

			var referenceFile = Path.Combine(PathToImages, "baboon.jpg");

			var tasks = new List<Task>();

			var complete = false;
			var exceptions = new ConcurrentBag<Exception>();

			for (int i = 0; i < numThreads; i++)
			{
				var task = Task.Run(() =>
				{
					try
					{
						for (int j = 0; j < numIterationsPerThread && exceptions.IsEmpty; j++)
						{
							var imageData = ComputeThumbnail(referenceFile);
							Assert.NotEmpty(imageData);
						}
					}
					catch (Exception ex)
					{
						exceptions.Add(ex);
					}
				});
				tasks.Add(task);
			}

			Task.Run(async () =>
			{
				while (!complete && exceptions.IsEmpty)
				{
					GC.Collect();
					await Task.Delay(500);
				}
			});

			Task.WaitAll(tasks.ToArray());

			complete = true;

			if (!exceptions.IsEmpty)
				throw new AggregateException(exceptions);

			// Intentionally NOT disposed: these native objects are left for the GC and
			// finalizer thread to reclaim, which is exactly what this test is exercising.
			static byte[] ComputeThumbnail(string fileName)
			{
				var ms = new MemoryStream();
				var bitmap = SKBitmap.Decode(fileName);
				var scaledBitmap = new SKBitmap(60, 40, bitmap.ColorType, bitmap.AlphaType);

				bitmap.ScalePixels(scaledBitmap, new SKSamplingOptions(SKCubicResampler.Mitchell));

				var image = SKImage.FromBitmap(scaledBitmap);
				var data = image.Encode(SKEncodedImageFormat.Png, 80);

				data.SaveTo(ms);

				return ms.ToArray();
			}
		}
	}
}
