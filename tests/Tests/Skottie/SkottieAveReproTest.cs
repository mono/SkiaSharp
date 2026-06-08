using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp.Resources;
using SkiaSharp.Skottie;
using Xunit;
using Xunit.Abstractions;

namespace SkiaSharp.Tests
{
	// Repro harness for the intermittent skottie_animation_render AccessViolationException
	// seen on net48/x86 CI (AnimationBuilderTest.CanRenderWithBase64). The goal is to make
	// the crash deterministic on the dev box so it can be root-caused, not papered over.
	public class SkottieAveReproTest : SKTest
	{
		private readonly ITestOutputHelper output;

		public SkottieAveReproTest(ITestOutputHelper output)
		{
			this.output = output;
		}

		public static TheoryData<string> Base64Files =>
			new TheoryData<string>
			{
				"lottie-base64_dotnet-bot.json",
				"lottie-base64_women-thinking.json",
			};

		private void RenderOnce(string filename)
		{
			var animation = Animation
				.CreateBuilder()
				.SetResourceProvider(new DataUriResourceProvider())
				.Build(Path.Combine(PathToImages, filename));

			using var bmp = new SKBitmap((int)animation.Size.Width, (int)animation.Size.Height);
			bmp.Erase(SKColors.Red);

			using var canvas = new SKCanvas(bmp);
			animation.Seek(0.1);
			animation.Render(canvas, bmp.Info.Rect);
		}

		// EXP1: single-threaded, but force aggressive GC between build and render so any
		// premature collection of the builder/provider/animation native backing is exposed.
		[SkippableFact]
		public void Exp1_BuildThenGcThenRender()
		{
			foreach (var file in new[] { "lottie-base64_dotnet-bot.json", "lottie-base64_women-thinking.json" })
			{
				for (var i = 0; i < 200; i++)
				{
					var animation = Animation
						.CreateBuilder()
						.SetResourceProvider(new DataUriResourceProvider())
						.Build(Path.Combine(PathToImages, file));

					// drop builder + provider references, then collect hard
					GC.Collect();
					GC.WaitForPendingFinalizers();
					GC.Collect();

					using var bmp = new SKBitmap((int)animation.Size.Width, (int)animation.Size.Height);
					bmp.Erase(SKColors.Red);
					using var canvas = new SKCanvas(bmp);
					animation.Seek(0.1);
					animation.Render(canvas, bmp.Info.Rect);
				}
			}
			output.WriteLine("Exp1 completed without crash");
		}

		// EXP2: parallel renders racing on Skia globals/caches + GC pressure thread.
		[SkippableFact]
		public void Exp2_ParallelRenderWithGcPressure()
		{
			var cts = new CancellationTokenSource();
			var gcThread = new Thread(() =>
			{
				while (!cts.IsCancellationRequested)
				{
					GC.Collect();
					GC.WaitForPendingFinalizers();
				}
			});
			gcThread.IsBackground = true;
			gcThread.Start();

			try
			{
				Parallel.For(0, 500, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
				{
					var file = (i % 2 == 0) ? "lottie-base64_dotnet-bot.json" : "lottie-base64_women-thinking.json";
					RenderOnce(file);
				});
			}
			finally
			{
				cts.Cancel();
				gcThread.Join();
			}
			output.WriteLine("Exp2 completed without crash");
		}

		// EXP3: the real CI condition — skottie base64 render running concurrently with the
		// heterogeneous Skia work other xUnit test classes perform at the same time
		// (codec/bitmap decode, typeface creation, text paint). Targets a shared-global race
		// in the lazy image-decode path during render.
		[SkippableFact]
		public void Exp3_SkottieRenderConcurrentWithMixedSkiaWork()
		{
			var botPath = Path.Combine(PathToImages, "lottie-base64_dotnet-bot.json");
			var womenPath = Path.Combine(PathToImages, "lottie-base64_women-thinking.json");
			var jpegPath = Path.Combine(PathToImages, "baboon.jpg");
			var pngPath = Path.Combine(PathToImages, "baboon.png");

			var cts = new CancellationTokenSource();

			void SkottieLoop(string path)
			{
				while (!cts.IsCancellationRequested)
				{
					var animation = Animation
						.CreateBuilder()
						.SetResourceProvider(new DataUriResourceProvider())
						.Build(path);
					using var bmp = new SKBitmap((int)animation.Size.Width, (int)animation.Size.Height);
					using var canvas = new SKCanvas(bmp);
					animation.Seek(0.1);
					animation.Render(canvas, bmp.Info.Rect);
				}
			}

			void CodecLoop(string path)
			{
				while (!cts.IsCancellationRequested)
				{
					if (!File.Exists(path)) return;
					using var data = SKData.Create(path);
					using var codec = SKCodec.Create(data);
					if (codec == null) continue;
					using var bmp = SKBitmap.Decode(codec);
				}
			}

			void TypefaceTextLoop()
			{
				while (!cts.IsCancellationRequested)
				{
					using var tf = SKTypeface.FromFamilyName("Arial");
					using var paint = new SKPaint { Typeface = tf, TextSize = 24 };
					using var bmp = new SKBitmap(200, 60);
					using var canvas = new SKCanvas(bmp);
					canvas.DrawText("hello world", 5, 30, paint);
				}
			}

			var threads = new[]
			{
				new Thread(() => SkottieLoop(botPath)),
				new Thread(() => SkottieLoop(womenPath)),
				new Thread(() => CodecLoop(jpegPath)),
				new Thread(() => CodecLoop(pngPath)),
				new Thread(TypefaceTextLoop),
				new Thread(TypefaceTextLoop),
			};

			foreach (var t in threads) { t.IsBackground = true; t.Start(); }

			// run the storm for a fixed window
			Thread.Sleep(TimeSpan.FromSeconds(30));
			cts.Cancel();
			foreach (var t in threads) t.Join();

			output.WriteLine("Exp3 completed without crash");
		}
	}
}
