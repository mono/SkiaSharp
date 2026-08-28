using NuGet.Packaging;
using NuGet.Versioning;
using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.NuGet;
using Xunit;

namespace SkiaSharp.ReleaseTool.Tests.NuGet
{
	public sealed class PackageVerifierTests
	{
		private const string Commit = "0123456789abcdef0123456789abcdef01234567";

		[Fact]
		public async Task Uses_package_archive_nuspec_repository_and_version_range()
		{
			var bytes = PackageTestData.Create(
				"SkiaSharp.HarfBuzz",
				"4.152.0",
				Commit,
				"release/4.152.0",
				("net8.0", "HarfBuzzSharp", "[14.2.1.200, )"),
				("net9.0", "HarfBuzzSharp", "14.2.1.200"));
			var signatures = new RecordingSignatureVerifier();

			var package = await PackageVerifier.VerifyAsync(
				"SkiaSharp.HarfBuzz",
				NuGetVersion.Parse("4.152.0"),
				PackageTestData.Catalog("SkiaSharp.HarfBuzz", "4.152.0", bytes),
				bytes,
				verifySignature: true,
				signatures,
				PackageTestData.Policies(),
				CancellationToken.None);

			Assert.Equal(Commit, package.SourceCommit);
			Assert.Equal("release/4.152.0", package.SourceBranch);
			Assert.Equal(
				"14.2.1.200",
				PublicReceiptVerifier.CollapseDependencyMinimum(
					package.DependencyGroups,
					"HarfBuzzSharp").ToNormalizedString());
			Assert.Equal(["SkiaSharp.HarfBuzz"], signatures.VerifiedIds);
		}

		[Theory]
		[InlineData("size")]
		[InlineData("hash")]
		[InlineData("identity")]
		public async Task Rejects_catalog_or_package_mismatch(string kind)
		{
			var bytes = PackageTestData.Create(
				"SkiaSharp",
				"4.152.0",
				Commit,
				"release/4.152.0");
			var catalog = PackageTestData.Catalog("SkiaSharp", "4.152.0", bytes);
			catalog = kind switch
			{
				"size" => catalog with { PackageSize = catalog.PackageSize + 1 },
				"hash" => catalog with { PackageHash = Convert.ToBase64String(new byte[64]) },
				"identity" => catalog with { Id = "Other" },
				_ => catalog,
			};

			await Assert.ThrowsAsync<NuGetReceiptException>(() => PackageVerifier.VerifyAsync(
				"SkiaSharp",
				NuGetVersion.Parse("4.152.0"),
				catalog,
				bytes,
				false,
				new RecordingSignatureVerifier(),
				PackageTestData.Policies(),
				CancellationToken.None));
		}

		[Fact]
		public void Dependency_minimum_must_agree_across_frameworks()
		{
			var bytes = PackageTestData.Create(
				"SkiaSharp.HarfBuzz",
				"4.152.0",
				Commit,
				"release/4.152.0",
				("net8.0", "HarfBuzzSharp", "[14.2.1.200, )"),
				("net9.0", "HarfBuzzSharp", "[14.2.1.201, )"));
			using var archive = new PackageArchiveReader(new MemoryStream(bytes));

			Assert.Throws<NuGetReceiptException>(() =>
				PublicReceiptVerifier.CollapseDependencyMinimum(
					archive.NuspecReader.GetDependencyGroups().ToArray(),
					"HarfBuzzSharp"));
		}

		[Fact]
		public void Dependency_minimum_must_be_included_by_every_range()
		{
			var bytes = PackageTestData.Create(
				"SkiaSharp.HarfBuzz",
				"4.152.0",
				Commit,
				"release/4.152.0",
				("net8.0", "HarfBuzzSharp", "(14.2.1.200, )"));
			using var archive = new PackageArchiveReader(new MemoryStream(bytes));

			Assert.Throws<NuGetReceiptException>(() =>
				PublicReceiptVerifier.CollapseDependencyMinimum(
					archive.NuspecReader.GetDependencyGroups().ToArray(),
					"HarfBuzzSharp"));
		}

		[Fact]
		public async Task Hash_matching_malformed_package_is_a_typed_receipt_failure()
		{
			byte[] bytes = "not a package"u8.ToArray();

			await Assert.ThrowsAsync<NuGetReceiptException>(() =>
				PackageVerifier.VerifyAsync(
					"SkiaSharp",
					NuGetVersion.Parse("4.152.0"),
					PackageTestData.Catalog("SkiaSharp", "4.152.0", bytes),
					bytes,
					false,
					new RecordingSignatureVerifier(),
					PackageTestData.Policies(),
					CancellationToken.None));
		}

		[Fact]
		public async Task Signature_verifier_failure_is_not_suppressed()
		{
			var bytes = PackageTestData.Create(
				"SkiaSharp",
				"4.152.0",
				Commit,
				"release/4.152.0");
			var signatures = new RecordingSignatureVerifier
			{
				Failure = new NuGetReceiptException("untrusted signature"),
			};

			var error = await Assert.ThrowsAsync<NuGetReceiptException>(() =>
				PackageVerifier.VerifyAsync(
					"SkiaSharp",
					NuGetVersion.Parse("4.152.0"),
					PackageTestData.Catalog("SkiaSharp", "4.152.0", bytes),
					bytes,
					true,
					signatures,
					PackageTestData.Policies(),
					CancellationToken.None));
			Assert.Contains("untrusted", error.Message);
		}
	}
}
