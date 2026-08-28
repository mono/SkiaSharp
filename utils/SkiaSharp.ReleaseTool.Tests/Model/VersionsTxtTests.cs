using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Model;
using Xunit;

namespace SkiaSharp.ReleaseTool.Tests.Model
{
	public class VersionsTxtTests
	{
		private const string SampleVersionsText =
			"# native sources\n" +
			"harfbuzz                                        release     14.2.1\n" +
			"skia                                            release     m152\n" +
			"\n" +
			"# native milestones\n" +
			"libSkiaSharp            milestone   152\n" +
			"libSkiaSharp            increment   0\n" +
			"\n" +
			"# nuget versions\n" +
			"SkiaSharp                                       nuget       4.152.0\n" +
			"SkiaSharp.HarfBuzz                               nuget       4.152.0\n" +
			"HarfBuzzSharp                                   nuget       14.2.1.200\n";

		[Fact]
		public void Parses_the_bare_SkiaSharp_nuget_line_not_a_suffixed_one()
		{
			Assert.Equal("4.152.0", VersionsTxt.ParseSkiaSharpNugetVersion(SampleVersionsText));
		}

		[Fact]
		public void Parses_the_HarfBuzzSharp_nuget_line()
		{
			Assert.Equal("14.2.1.200", VersionsTxt.ParseHarfBuzzSharpNugetVersion(SampleVersionsText));
		}

		[Fact]
		public void Parses_the_current_major_and_milestone()
		{
			var (major, milestone) = VersionsTxt.ParseCurrentMajorAndMilestone(SampleVersionsText);

			Assert.Equal(4, major);
			Assert.Equal(152, milestone);
		}

		[Fact]
		public void Throws_when_the_SkiaSharp_nuget_line_is_missing()
		{
			Assert.Throws<PlanException>(() => VersionsTxt.ParseSkiaSharpNugetVersion("no such line here"));
		}

		[Fact]
		public void Throws_when_the_HarfBuzzSharp_nuget_line_is_missing()
		{
			Assert.Throws<PlanException>(() => VersionsTxt.ParseHarfBuzzSharpNugetVersion("no such line here"));
		}

		[Fact]
		public void Throws_when_the_milestone_line_is_missing()
		{
			Assert.Throws<PlanException>(
				() => VersionsTxt.ParseCurrentMajorAndMilestone("SkiaSharp                                       nuget       4.152.0\n"));
		}
	}
}
