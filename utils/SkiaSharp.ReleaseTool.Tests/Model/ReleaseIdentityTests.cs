using NuGet.Versioning;
using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Model;
using Xunit;

namespace SkiaSharp.ReleaseTool.Tests.Model
{
	public sealed class ReleaseIdentityTests
	{
		[Theory]
		[InlineData("3.119.0", "stable", false, true)]
		[InlineData("3.119.0-preview.1", "preview.1", false, false)]
		[InlineData("3.119.0-rc.2", "rc.2", false, false)]
		[InlineData("3.119.0.1", "stable", true, true)]
		[InlineData("3.119.0.1-preview.2", "preview.2", true, false)]
		public void Parses_supported_release_identities(
			string value,
			string label,
			bool hotfix,
			bool stable)
		{
			var identity = SkiaSharpReleaseIdentity.Parse(value);

			Assert.Equal(value, identity.Raw);
			Assert.Equal(label, identity.Label);
			Assert.Equal(hotfix, identity.IsHotfix);
			Assert.Equal(stable, identity.Stable);
			Assert.Equal($"release/{value}", identity.ReleaseBranch);
			Assert.Equal($"v{value}", identity.Tag);
			Assert.Equal("release/3.119.x", identity.IntegrationBranch);
		}

		[Theory]
		[InlineData("3.119")]
		[InlineData("3.119.0.1.2")]
		[InlineData("3.119.0-preview.0")]
		[InlineData("3.119.0-preview")]
		[InlineData("3.119.0-beta.1")]
		[InlineData("3.119.0+metadata")]
		[InlineData("3.119.0.0")]
		[InlineData("2147483648.0.0")]
		public void Rejects_versions_outside_release_policy(string value)
		{
			Assert.False(SkiaSharpReleaseIdentity.TryParse(value, out _));
			Assert.Throws<PlanException>(() => SkiaSharpReleaseIdentity.Parse(value));
		}

		[Fact]
		public void Branch_and_tag_parsers_strip_only_the_exact_prefix()
		{
			Assert.Equal(
				"3.119.0-preview.1",
				SkiaSharpReleaseIdentity.ParseBranch("release/3.119.0-preview.1").Raw);
			Assert.Equal(
				"3.119.0-rc.1",
				SkiaSharpReleaseIdentity.ParseTag("v3.119.0-rc.1").Raw);
			Assert.Throws<PlanException>(
				() => SkiaSharpReleaseIdentity.ParseBranch("origin/release/3.119.0"));
			Assert.Throws<PlanException>(
				() => SkiaSharpReleaseIdentity.ParseTag("release/v3.119.0"));
		}

		[Theory]
		[InlineData("main", "main")]
		[InlineData("refs/heads/main", "main")]
		[InlineData("origin/release/3.119.x", "release/3.119.x")]
		[InlineData("refs/remotes/origin/release/3.119.x", "release/3.119.x")]
		public void Normalizes_supported_integration_branches(string input, string expected)
		{
			Assert.Equal(expected, ReleaseVersionPolicy.NormalizeIntegrationBranch(input));
		}

		[Theory]
		[InlineData("release/3.119.0")]
		[InlineData("refs/pull/123/head")]
		[InlineData("feature/test")]
		public void Rejects_unsupported_integration_branches(string value)
		{
			Assert.Throws<PlanException>(
				() => ReleaseVersionPolicy.NormalizeIntegrationBranch(value));
		}

		[Fact]
		public void NuGet_version_release_ordering_covers_channels_stable_and_hotfix()
		{
			var ordered = new[]
			{
				"3.119.0.1",
				"3.119.0",
				"3.119.0-rc.1",
				"3.119.0-preview.2",
				"3.119.0-preview.1",
			}
				.Select(SkiaSharpReleaseIdentity.Parse)
				.OrderBy(identity => identity.Version, VersionComparer.VersionRelease)
				.Select(identity => identity.Raw)
				.ToArray();

			Assert.Equal(
				[
					"3.119.0-preview.1",
					"3.119.0-preview.2",
					"3.119.0-rc.1",
					"3.119.0",
					"3.119.0.1",
				],
				ordered);
		}

		[Fact]
		public void Tag_ordering_uses_NuGet_version_release_comparer()
		{
			string[] tags =
			[
				"not-a-release",
				"v3.119.0-preview.1",
				"v3.119.0-preview.2",
				"v3.119.0-rc.1",
				"v3.118.0",
			];

			Assert.Equal(
				"v3.119.0-preview.2",
				TagOrdering.SelectPreviousTag("v3.119.0-rc.1", tags));
			Assert.Equal(
				"v3.119.0-rc.1",
				TagOrdering.SelectPreviousTag("v3.119.0", tags));
		}

		[Fact]
		public void Tag_ordering_normalizes_historical_build_revision_tags()
		{
			string[] tags =
			[
				"v4.151.1",
				"v4.152.0-preview.1.1",
				"v4.152.0-preview.2.26426.14",
				"v4.152.0-rc.1.26426.14",
			];

			Assert.Equal(
				"v4.152.0-preview.2.26426.14",
				TagOrdering.SelectPreviousTag("v4.152.0-rc.1", tags));
			Assert.Equal(
				"v4.152.0-rc.1.26426.14",
				TagOrdering.SelectPreviousTag("v4.152.0", tags));
			Assert.Equal(
				"v4.151.1",
				TagOrdering.SelectPreviousTag("v4.152.0-preview.1", tags));
		}

		[Fact]
		public void Tag_ordering_rejects_duplicate_normalized_identities()
		{
			var error = Assert.Throws<PlanException>(() =>
				TagOrdering.SelectPreviousTag(
					"v4.152.0",
					[
						"v4.152.0-preview.1.1",
						"v4.152.0-preview.1.26426.14",
					]));

			Assert.Contains("normalize to the same identity", error.Message);
		}

		[Theory]
		[InlineData("42")]
		[InlineData("12345.7")]
		[InlineData("20250131.3")]
		public void Public_version_composition_round_trips_build_revision(string revision)
		{
			var identity = SkiaSharpReleaseIdentity.Parse("3.119.0-preview.2");
			var publicVersion = identity.ComposePublicVersion(revision);

			var (@base, actualRevision) = identity.ValidatePublicVersion(publicVersion);

			Assert.Equal("3.119.0", @base);
			Assert.Equal(revision, actualRevision);
			Assert.True(NuGetVersion.TryParse(publicVersion, out _));
		}

		[Theory]
		[InlineData("3.119.0-preview.2.1234.1")]
		[InlineData("3.119.0-preview.1.12345.1")]
		[InlineData("3.120.0-preview.2.12345.1")]
		[InlineData("3.119.0-preview.2.1.2.3")]
		[InlineData("3.119.0-preview.2.1+metadata")]
		[InlineData("3.119.0-Preview.2.12345.1")]
		public void Public_version_validation_rejects_inconsistent_versions(string value)
		{
			var identity = SkiaSharpReleaseIdentity.Parse("3.119.0-preview.2");
			Assert.Throws<PlanException>(() => identity.ValidatePublicVersion(value));
		}

		[Fact]
		public void Stable_public_version_has_no_revision()
		{
			var identity = SkiaSharpReleaseIdentity.Parse("3.119.0.1");
			Assert.Equal(("3.119.0.1", null), identity.ValidatePublicVersion("3.119.0.1"));
			Assert.Throws<PlanException>(() => identity.ComposePublicVersion("1"));
		}

		[Fact]
		public void HarfBuzz_increment_uses_numeric_components_and_checks_overflow()
		{
			Assert.Equal("1.8.8.1", HarfBuzzVersioning.IncrementHarfBuzz("1.8.8"));
			Assert.Equal("14.2.1.201", HarfBuzzVersioning.IncrementHarfBuzz("14.2.1.200"));
			Assert.Throws<PlanException>(
				() => HarfBuzzVersioning.IncrementHarfBuzz($"14.2.1.{int.MaxValue}"));
			Assert.Throws<PlanException>(
				() => HarfBuzzVersioning.IncrementHarfBuzz("14.2.1-preview.1"));
			Assert.Throws<PlanException>(
				() => HarfBuzzVersioning.IncrementHarfBuzz("14.2.1+metadata"));
		}

		[Fact]
		public void Next_version_checks_patch_overflow_and_rejects_hotfixes()
		{
			Assert.Equal(
				("3.119.1", "1.8.8.2"),
				HarfBuzzVersioning.CalculateNextVersions("3.119.0", "1.8.8.1"));
			Assert.Throws<PlanException>(
				() => HarfBuzzVersioning.CalculateNextVersions("3.119.0.1", "1.8.8.1"));
			Assert.Throws<PlanException>(
				() => HarfBuzzVersioning.CalculateNextVersions($"3.119.{int.MaxValue}", "1.8.8.1"));
		}
	}
}
