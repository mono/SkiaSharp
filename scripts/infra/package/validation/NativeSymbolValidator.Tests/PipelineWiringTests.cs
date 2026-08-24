using System.Text.RegularExpressions;
using Xunit;

namespace SkiaSharp.PackageValidation.Tests;

/// <summary>
/// The validator is only worth anything if it actually runs against the real packed output, so the
/// wiring that makes that happen is asserted here. These tests read the committed pipeline and
/// build definitions: if the scan job stops downloading an artifact set, stops invoking the
/// validator, or starts allowing itself to be skipped, they fail.
/// </summary>
public class PipelineWiringTests
{
	private static readonly string PackageStage =
		RepositoryPaths.ReadFile ("scripts", "azure-templates-stages-package.yml");

	private static readonly string BuildCake =
		RepositoryPaths.ReadFile ("build.cake");

	private static readonly string NuGetCake =
		RepositoryPaths.ReadFile ("scripts", "infra", "package", "nuget.cake");

	private static readonly string ScanJob = ExtractJob (PackageStage, "scan_normal_windows");

	private static readonly string PackageJob = ExtractJob (PackageStage, "package_normal_windows");

	[Fact]
	public void ScanJobUsesTheBootstrapperSoItCanRunCake ()
	{
		// The merger template hard-codes skipSteps, which silently swallows any target it is given.
		Assert.Contains ("azure-templates-jobs-bootstrapper.yml", ScanJob, StringComparison.Ordinal);
		Assert.DoesNotContain ("azure-templates-jobs-merger.yml", ScanJob, StringComparison.Ordinal);
	}

	[Fact]
	public void ScanJobInvokesTheValidationTarget ()
	{
		Assert.Contains ("target: nuget-validate", ScanJob, StringComparison.Ordinal);
	}

	[Fact]
	public void ScanJobFailsClosedOnAMissingPackage ()
	{
		Assert.Contains ("--requireAll=true", ScanJob, StringComparison.Ordinal);
	}

	[Fact]
	public void ScanJobRunsAfterPackaging ()
	{
		Assert.Contains ("dependsOn: package_normal_windows", ScanJob, StringComparison.Ordinal);
	}

	[Theory]
	[InlineData ("nuget", "nugets")]
	[InlineData ("nuget_symbols", "nugets-symbols")]
	public void ScanJobDownloadsTheArtifactSetTheValidatorReads (string artifact, string directory)
	{
		var pattern = new Regex (
			$@"-\s+name:\s+{Regex.Escape (artifact)}\s+dir:\s+{Regex.Escape (directory)}\s+currentRun:\s+true",
			RegexOptions.Singleline);

		Assert.Matches (pattern, Collapse (ScanJob));
	}

	[Theory]
	[InlineData ("nuget")]
	[InlineData ("nuget_symbols")]
	public void PackagingPublishesTheArtifactSetTheScanJobDownloads (string artifact)
	{
		Assert.Contains ($"- name: {artifact}", PackageJob, StringComparison.Ordinal);
	}

	[Fact]
	public void ScanJobIsNeverSkippedByTheBuildCache ()
	{
		// A cached job sets CACHE_SKIP and skips its steps. Validation that can be skipped is not
		// validation, so the scan job deliberately opts out of caching entirely.
		Assert.DoesNotContain ("cacheJob:", ScanJob, StringComparison.Ordinal);
		Assert.DoesNotContain ("enableCaching:", ScanJob, StringComparison.Ordinal);
	}

	[Fact]
	public void BuildCakeRoutesTheValidationTargetToTheNuGetScript ()
	{
		Assert.Matches (
			new Regex (@"Task\s*\(\s*""nuget-validate""\s*\)[\s\S]{0,400}?RunCake\s*\(\s*""\./scripts/infra/package/nuget\.cake""\s*,\s*""nuget-validate""\s*\)"),
			BuildCake);
	}

	[Fact]
	public void PackingLocallyAlsoValidates ()
	{
		// Running `dotnet cake --target=nuget` must validate what it just packed, so a developer
		// sees the same failures CI would report.
		var aggregate = ExtractCakeTask (BuildCake, "nuget");
		Assert.Contains (@"IsDependentOn (""nuget-validate"")", aggregate, StringComparison.Ordinal);
		Assert.Contains (@"IsDependentOn (""nuget-normal"")", aggregate, StringComparison.Ordinal);
	}

	[Theory]
	[InlineData ("--packages")]
	[InlineData ("--symbol-packages")]
	[InlineData ("--versions-file")]
	[InlineData ("--require-all")]
	[InlineData ("--preview-suffix")]
	public void ValidationTargetPassesTheRequiredInput (string argument)
	{
		var target = ExtractCakeTask (NuGetCake, "nuget-validate");
		Assert.Contains ($"\"{argument}\"", target, StringComparison.Ordinal);
	}

	[Fact]
	public void ValidationTargetReadsThePackedOutputDirectories ()
	{
		var target = ExtractCakeTask (NuGetCake, "nuget-validate");

		Assert.Contains ("OUTPUT_NUGETS_PATH", target, StringComparison.Ordinal);
		Assert.Contains ("OUTPUT_SYMBOLS_NUGETS_PATH", target, StringComparison.Ordinal);
	}

	[Fact]
	public void ValidationTargetOnlyValidatesPreviewPackagesWhenTheyArePacked ()
	{
		var target = ExtractCakeTask (NuGetCake, "nuget-validate");

		Assert.Contains ("PREVIEW_NUGET_SUFFIX", target, StringComparison.Ordinal);
	}

	[Fact]
	public void ValidationTargetRunsTheValidatorsOwnTests ()
	{
		var target = ExtractCakeTask (NuGetCake, "nuget-validate");

		Assert.Contains ("NativeSymbolValidator.Tests.csproj", target, StringComparison.Ordinal);
		Assert.Contains ("DotNetTest", target, StringComparison.Ordinal);
	}

	[Fact]
	public void PackingWritesPreviewAndStablePackagesWhereValidationLooksForThem ()
	{
		// Validation reads a single pair of directories, which is only correct because pack writes
		// both channels into them.
		var pack = ExtractCakeTask (NuGetCake, "nuget-normal");

		Assert.Contains ("VersionSuffix", pack, StringComparison.Ordinal);
		Assert.Contains ("OUTPUT_NUGETS_PATH", pack, StringComparison.Ordinal);
		Assert.Contains ("OUTPUT_SYMBOLS_NUGETS_PATH", pack, StringComparison.Ordinal);
	}

	private static string ExtractJob (string yaml, string jobName)
	{
		var nameIndex = yaml.IndexOf ($"name: {jobName}", StringComparison.Ordinal);
		Assert.True (nameIndex >= 0, $"Could not find the job '{jobName}'.");

		var start = yaml.LastIndexOf ("      - template:", nameIndex, StringComparison.Ordinal);
		Assert.True (start >= 0, $"Could not find the template for job '{jobName}'.");

		var end = yaml.IndexOf ("\n      - template:", start + 1, StringComparison.Ordinal);
		return end < 0 ? yaml[start..] : yaml[start..end];
	}

	private static string ExtractCakeTask (string cake, string taskName)
	{
		var start = cake.IndexOf ($"Task (\"{taskName}\")", StringComparison.Ordinal);
		Assert.True (start >= 0, $"Could not find the Cake task '{taskName}'.");

		var end = cake.IndexOf ("\nTask (\"", start + 1, StringComparison.Ordinal);
		return end < 0 ? cake[start..] : cake[start..end];
	}

	private static string Collapse (string value) =>
		Regex.Replace (value, @"\s+", " ");
}
