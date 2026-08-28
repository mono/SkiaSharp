using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Model;
using Xunit;

namespace SkiaSharp.ReleaseTool.Tests.Model
{
	public class VariablesYamlTests
	{
		private const string SampleVariablesText =
			"variables:\n" +
			"  SKIASHARP_VERSION: 3.119.0\n" +
			"  PREVIEW_LABEL: 'preview.0'\n";

		[Fact]
		public void Parses_the_SkiaSharp_version()
		{
			Assert.Equal("3.119.0", VariablesYaml.ParseSkiaSharpVersion(SampleVariablesText));
		}

		[Fact]
		public void Parses_and_trims_the_preview_label()
		{
			Assert.Equal("preview.0", VariablesYaml.ParsePreviewLabel(SampleVariablesText));
		}

		[Fact]
		public void Throws_when_SKIASHARP_VERSION_is_missing()
		{
			Assert.Throws<PlanException>(() => VariablesYaml.ParseSkiaSharpVersion("variables:\n  OTHER: 1\n"));
		}

		[Fact]
		public void Throws_when_PREVIEW_LABEL_is_missing()
		{
			Assert.Throws<PlanException>(() => VariablesYaml.ParsePreviewLabel("variables:\n  OTHER: 1\n"));
		}
	}
}
