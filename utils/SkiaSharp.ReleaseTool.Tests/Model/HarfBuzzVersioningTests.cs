using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Model;
using Xunit;

namespace SkiaSharp.ReleaseTool.Tests.Model
{
	/// <summary>Ported test-for-test from Python's <c>tests/test_release_model.py</c>.</summary>
	public class HarfBuzzVersioningTests
	{
		[Fact]
		public void Three_part_version_gains_a_revision()
		{
			Assert.Equal("1.8.8.1", HarfBuzzVersioning.IncrementHarfBuzz("1.8.8"));
		}

		[Theory]
		[InlineData("1.8.8.1", "1.8.8.2")]
		[InlineData("14.2.1.200", "14.2.1.201")]
		public void Four_part_version_increments_last_component(string input, string expected)
		{
			Assert.Equal(expected, HarfBuzzVersioning.IncrementHarfBuzz(input));
		}

		[Fact]
		public void Rejects_non_numeric_version()
		{
			Assert.Throws<PlanException>(() => HarfBuzzVersioning.IncrementHarfBuzz("1.8.x"));
		}

		[Fact]
		public void Calculate_next_versions_bumps_patch_and_harfbuzz()
		{
			var (skia, harfbuzz) = HarfBuzzVersioning.CalculateNextVersions("3.119.0", "1.8.8.1");

			Assert.Equal("3.119.1", skia);
			Assert.Equal("1.8.8.2", harfbuzz);
		}

		[Fact]
		public void Calculate_next_versions_rejects_hotfix_numeric()
		{
			Assert.Throws<PlanException>(() => HarfBuzzVersioning.CalculateNextVersions("3.119.0.1", "1.8.8.1"));
		}
	}
}
