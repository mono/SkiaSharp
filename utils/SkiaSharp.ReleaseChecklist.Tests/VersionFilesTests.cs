namespace SkiaSharp.ReleaseChecklist.Tests;

public class VersionFilesTests
{
	[Fact]
	public void ParseRejectsMismatchedFileAndNuGetRows()
	{
		const string variables =
			"""
			variables:
			  SKIASHARP_VERSION: 4.152.0
			  PREVIEW_LABEL: 'preview.0'

			""";
		const string versions =
			"""
			SkiaSharp file 4.153.0.0
			HarfBuzzSharp file 8.0.0.1
			SkiaSharp nuget 4.152.0
			HarfBuzzSharp nuget 8.0.0.1

			""";

		var exception = Assert.Throws<ReleasePolicyException>(() =>
			VersionFiles.Parse(variables, versions));

		Assert.Contains("inconsistent", exception.Message);
	}
}
