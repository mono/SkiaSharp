using SkiaSharp;
using Xunit;

namespace SkiaSharp.Views.Gtk4.Tests
{
	// The other tests in this assembly initialise native GTK4 in their constructors and skip every
	// test when those libraries are unavailable (e.g. a headless agent). This managed-only test
	// exercises pure-managed SkiaSharp geometry types (no GTK, no native SkiaSharp call) so the
	// suite always has at least one executed test — Microsoft.Testing.Platform fails a run that
	// executes zero tests with exit code 8.
	public class SmokeTest
	{
		[Fact]
		public void ManagedGeometryTypesRoundTrip()
		{
			var point = new SKPointI(3, 4);
			var size = new SKSizeI(point.X, point.Y);

			Assert.Equal(3, point.X);
			Assert.Equal(4, point.Y);
			Assert.Equal(3, size.Width);
			Assert.Equal(4, size.Height);
		}
	}
}
