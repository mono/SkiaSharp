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
		[SkippableTheory]
		[InlineData(100, 1000)]
		public static void ImageScalingMultipleThreadsTest(int numThreads, int numIterationsPerThread)
		{
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
