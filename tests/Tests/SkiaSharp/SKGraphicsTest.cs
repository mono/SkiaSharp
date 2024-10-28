using System.Collections.Generic;
using System.IO;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKGraphicsTest : SKTest
	{
		public SKGraphicsTest()
		{
			SKGraphics.Init();
		}

		[SkippableFact]
		public unsafe void GetFontCacheLimitIsNotZero()
		{
			var limit = SKGraphics.GetFontCacheLimit();

			Assert.NotEqual(0, limit);
		}

		[SkippableFact]
		public unsafe void GetFontCacheLimitUpdatesAndReturnsPrevious()
		{
			var limit = SKGraphics.GetFontCacheLimit();
			Assert.NotEqual(0, limit);

			var oldLimit = SKGraphics.SetFontCacheLimit(limit + 1);
			Assert.Equal(limit, oldLimit);

			var newLimit = SKGraphics.SetFontCacheLimit(limit);
			Assert.Equal(limit + 1, newLimit);
		}

		[SkippableFact]
		public unsafe void GetMemoryDump()
		{
			using var dump = new TextWriterDump(true, true);

			SKGraphics.DumpMemoryStatistics(dump);

			Assert.NotEmpty(dump.Lines);
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[SkippableFact]
		public void CanGetMemoryDumpOnGpuSurface()
		{
			using var ctx = CreateGlContext();
			ctx.MakeCurrent();

			using var grContext = GRContext.CreateGl();
			using var surface = SKSurface.Create(grContext, true, new SKImageInfo(100, 100));
			var canvas = surface.Canvas;

			using var dump = new TextWriterDump(true, true);

			grContext.DumpMemoryStatistics(dump);

			Assert.NotEmpty(dump.Lines);
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[SkippableFact]
		public void CanGetMemoryDumpOnGpuImages()
		{
			using var ctx = CreateGlContext();
			ctx.MakeCurrent();

			using var grContext = GRContext.CreateGl();
			using var surface = SKSurface.Create(grContext, true, new SKImageInfo(100, 100));
			var canvas = surface.Canvas;

			using var data = SKData.Create(Path.Combine(PathToImages, "baboon.jpg"));
			using var image = SKImage.FromEncodedData(data);

			canvas.DrawImage(image, 0, 0);

			using var dump = new TextWriterDump(true, true);

			grContext.DumpMemoryStatistics(dump);

			Assert.NotEmpty(dump.Lines);
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[SkippableFact]
		public void CanGetMemoryDumpOnGpuImagesAfterPurge()
		{
			using var ctx = CreateGlContext();
			ctx.MakeCurrent();

			using var grContext = GRContext.CreateGl();
			using var surface = SKSurface.Create(grContext, true, new SKImageInfo(100, 100));
			var canvas = surface.Canvas;

			using var data = SKData.Create(Path.Combine(PathToImages, "baboon.jpg"));
			using var image = SKImage.FromEncodedData(data);

			canvas.DrawImage(image, 0, 0);

			using var dump = new TextWriterDump(true, true);

			grContext.PurgeResources();
			grContext.DumpMemoryStatistics(dump);

			Assert.NotEmpty(dump.Lines);
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.Gpu)]
		[SkippableFact]
		public void CanGetMemoryDumpOnGpuImagesAfterPurgeUnlocked()
		{
			using var ctx = CreateGlContext();
			ctx.MakeCurrent();

			using var grContext = GRContext.CreateGl();
			using var surface = SKSurface.Create(grContext, true, new SKImageInfo(100, 100));
			var canvas = surface.Canvas;

			using var data = SKData.Create(Path.Combine(PathToImages, "baboon.jpg"));
			using var image = SKImage.FromEncodedData(data);

			canvas.DrawImage(image, 0, 0);

			using var dump = new TextWriterDump(true, true);

			grContext.PurgeUnlockedResources(1024 * 1024, true);
			grContext.DumpMemoryStatistics(dump);

			Assert.NotEmpty(dump.Lines);
		}

		private sealed class TextWriterDump : SKTraceMemoryDump
		{
			public TextWriterDump(bool detailedDump, bool dumpWrappedObjects)
				: base(detailedDump, dumpWrappedObjects)
			{
			}

			public List<string> Lines { get; } = new List<string>();

			protected internal override void OnDumpNumericValue(string dumpName, string valueName, string units, ulong value) =>
				Lines.Add($"{dumpName}.{valueName} = {value} {units}");

			protected internal override void OnDumpStringValue(string dumpName, string valueName, string value) =>
				Lines.Add($"{dumpName}.{valueName} = '{value}'");
		}
	}
}
