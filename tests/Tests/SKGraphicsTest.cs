using System.Collections.Generic;
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

		[Trait(CategoryKey, GpuCategory)]
		[SkippableFact]
		public void CreateDefaultContextIsValid()
		{
			using var ctx = CreateGlContext();
			ctx.MakeCurrent();

			using var grContext = GRContext.CreateGl();
			using var surface = SKSurface.Create(grContext, true, new SKImageInfo(100, 100));
			var canvas = surface.Canvas;

			using var dump = new TextWriterDump(true, true);

			SKGraphics.DumpMemoryStatistics(grContext, dump);

			Assert.NotEmpty(dump.Lines);
		}

		private sealed class TextWriterDump : SKTraceMemoryDump
		{
			public TextWriterDump(bool detailedDump, bool dumpWrappedObjects)
				: base(detailedDump, dumpWrappedObjects)
			{
			}

			public List<string> Lines { get; } = new List<string>();

			protected override void OnDumpNumericValue(string dumpName, string valueName, string units, ulong value) =>
				Lines.Add($"{dumpName}.{valueName} = {value} {units}");

			protected override void OnDumpStringValue(string dumpName, string valueName, string value) =>
				Lines.Add($"{dumpName}.{valueName} = '{value}'");
		}
	}
}
