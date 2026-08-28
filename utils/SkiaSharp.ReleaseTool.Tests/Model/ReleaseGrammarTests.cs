using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Model;
using Xunit;

namespace SkiaSharp.ReleaseTool.Tests.Model
{
	/// <summary>Ported test-for-test from Python's <c>tests/test_release_model.py</c>.</summary>
	public class ReleaseGrammarTests
	{
		[Fact]
		public void Stable_version_has_expected_properties()
		{
			var version = ReleaseGrammar.ParseReleaseVersion("3.119.0");

			Assert.Equal("3.119.0", version.Numeric);
			Assert.Null(version.Channel);
			Assert.True(version.Stable);
			Assert.False(version.IsHotfix);
			Assert.Equal("stable", version.Label);
			Assert.Equal("stable", version.ReleaseType);
			Assert.Equal("3.119", version.Line);
			Assert.Equal("release/3.119.x", version.IntegrationBranch);
			Assert.Equal("release/3.119.0", version.ReleaseBranch);
			Assert.Equal("v3.119.0", version.Tag);
			Assert.Equal("Version 3.119.0", version.Title);
		}

		[Fact]
		public void Preview_version_has_expected_properties()
		{
			var version = ReleaseGrammar.ParseReleaseVersion("3.119.0-preview.1");

			Assert.Equal("preview", version.Channel);
			Assert.Equal(1, version.Iteration);
			Assert.False(version.Stable);
			Assert.Equal("preview.1", version.Label);
			Assert.Equal("preview", version.ReleaseType);
			Assert.Equal("Version 3.119.0 (Preview 1)", version.Title);
		}

		[Fact]
		public void Rc_version_has_expected_properties()
		{
			var version = ReleaseGrammar.ParseReleaseVersion("3.119.0-rc.2");

			Assert.Equal("rc", version.Channel);
			Assert.Equal("rc.2", version.Label);
			Assert.Equal("rc", version.ReleaseType);
			Assert.Equal("Version 3.119.0 (RC 2)", version.Title);
		}

		[Fact]
		public void Hotfix_stable_version_has_expected_properties()
		{
			var version = ReleaseGrammar.ParseReleaseVersion("3.119.0.1");

			Assert.True(version.IsHotfix);
			Assert.True(version.Stable);
			Assert.Equal("hotfix stable", version.ReleaseType);
			Assert.Equal([3, 119, 0, 1], version.Parts);
		}

		[Fact]
		public void Hotfix_preview_version_has_expected_release_type()
		{
			var version = ReleaseGrammar.ParseReleaseVersion("3.119.0.1-preview.1");

			Assert.True(version.IsHotfix);
			Assert.Equal("hotfix preview", version.ReleaseType);
		}

		[Theory]
		[InlineData("3.119")]
		[InlineData("v3.119.0")]
		[InlineData("3.119.0-beta.1")]
		[InlineData("3.119.0.1.2")]
		[InlineData("3.119.0-preview")]
		public void Rejects_invalid_grammar(string bad)
		{
			Assert.Throws<PlanException>(() => ReleaseGrammar.ParseReleaseVersion(bad));
		}

		[Fact]
		public void Rejects_zero_iteration()
		{
			Assert.Throws<PlanException>(() => ReleaseGrammar.ParseReleaseVersion("3.119.0-preview.0"));
		}

		[Fact]
		public void Sort_key_orders_channels_before_stable()
		{
			var preview = ReleaseGrammar.ParseReleaseVersion("3.119.0-preview.1");
			var rc = ReleaseGrammar.ParseReleaseVersion("3.119.0-rc.1");
			var stable = ReleaseGrammar.ParseReleaseVersion("3.119.0");

			Assert.True(preview.CompareSortKeyTo(rc) < 0);
			Assert.True(rc.CompareSortKeyTo(stable) < 0);
		}

		[Fact]
		public void Sort_key_orders_a_hotfix_after_its_three_part_base()
		{
			var baseVersion = ReleaseGrammar.ParseReleaseVersion("3.119.0");
			var hotfix = ReleaseGrammar.ParseReleaseVersion("3.119.0.1");

			Assert.True(baseVersion.CompareSortKeyTo(hotfix) < 0);
		}

		[Fact]
		public void Parse_release_branch_strips_the_prefix()
		{
			var version = ReleaseGrammar.ParseReleaseBranch("release/3.119.0-preview.1");

			Assert.Equal("3.119.0-preview.1", version.Raw);
		}

		[Fact]
		public void Parse_release_branch_rejects_missing_prefix()
		{
			Assert.Throws<PlanException>(() => ReleaseGrammar.ParseReleaseBranch("3.119.0-preview.1"));
		}

		[Fact]
		public void Parse_release_tag_strips_the_prefix()
		{
			var version = ReleaseGrammar.ParseReleaseTag("v3.119.0-rc.2");

			Assert.Equal("3.119.0-rc.2", version.Raw);
		}

		[Fact]
		public void Parse_release_tag_rejects_missing_prefix()
		{
			Assert.Throws<PlanException>(() => ReleaseGrammar.ParseReleaseTag("3.119.0"));
		}

		[Fact]
		public void Normalize_integration_branch_accepts_main()
		{
			Assert.Equal("main", ReleaseGrammar.NormalizeIntegrationBranch("main"));
		}

		[Fact]
		public void Normalize_integration_branch_accepts_maintenance_branch()
		{
			Assert.Equal("release/3.119.x", ReleaseGrammar.NormalizeIntegrationBranch("release/3.119.x"));
		}

		[Theory]
		[InlineData("refs/remotes/origin/main", "main")]
		[InlineData("origin/release/3.119.x", "release/3.119.x")]
		[InlineData("refs/heads/main", "main")]
		public void Normalize_integration_branch_strips_known_prefixes(string input, string expected)
		{
			Assert.Equal(expected, ReleaseGrammar.NormalizeIntegrationBranch(input));
		}

		[Fact]
		public void Normalize_integration_branch_rejects_exact_release_branch()
		{
			Assert.Throws<PlanException>(() => ReleaseGrammar.NormalizeIntegrationBranch("release/3.119.0-preview.1"));
		}

		[Fact]
		public void Normalize_integration_branch_rejects_pr_ref()
		{
			Assert.Throws<PlanException>(() => ReleaseGrammar.NormalizeIntegrationBranch("refs/pull/123/head"));
		}

		[Fact]
		public void Stable_public_version_requires_bare_equality()
		{
			var version = ReleaseGrammar.ParseReleaseVersion("3.119.0");

			var (@base, build) = version.ValidatePublicVersion("3.119.0");

			Assert.Equal("3.119.0", @base);
			Assert.Null(build);
		}

		[Fact]
		public void Stable_public_version_rejects_suffixed_version()
		{
			var version = ReleaseGrammar.ParseReleaseVersion("3.119.0");

			Assert.Throws<PlanException>(() => version.ValidatePublicVersion("3.119.0-preview.1.12345.1"));
		}

		[Fact]
		public void Preview_public_version_accepts_bare_build_number()
		{
			var version = ReleaseGrammar.ParseReleaseVersion("3.119.0-preview.1");

			var (@base, build) = version.ValidatePublicVersion("3.119.0-preview.1.42");

			Assert.Equal("3.119.0", @base);
			Assert.Equal("42", build);
		}

		[Theory]
		[InlineData("3.119.0-preview.1.12345.7", "12345.7")]
		[InlineData("3.119.0-preview.1.20250131.3", "20250131.3")]
		public void Preview_public_version_accepts_date_prefixed_build(string publicVersion, string expectedBuild)
		{
			var version = ReleaseGrammar.ParseReleaseVersion("3.119.0-preview.1");

			var (_, build) = version.ValidatePublicVersion(publicVersion);

			Assert.Equal(expectedBuild, build);
		}

		[Fact]
		public void Rc_public_version_rejects_wrong_base()
		{
			var version = ReleaseGrammar.ParseReleaseVersion("3.119.0-rc.1");

			Assert.Throws<PlanException>(() => version.ValidatePublicVersion("3.119.0-rc.2.5"));
		}

		[Theory]
		[InlineData("3.119.0-preview.1.abc")]
		[InlineData("3.119.0-preview.1.")]
		[InlineData("3.119.0-preview.1.1.2")]
		public void Rejects_malformed_build_revision(string bad)
		{
			var version = ReleaseGrammar.ParseReleaseVersion("3.119.0-preview.1");

			Assert.Throws<PlanException>(() => version.ValidatePublicVersion(bad));
		}

		[Fact]
		public void Four_part_hotfix_stable_composition()
		{
			var version = ReleaseGrammar.ParseReleaseVersion("3.119.0.1");

			var (@base, build) = version.ValidatePublicVersion("3.119.0.1");

			Assert.Equal("3.119.0.1", @base);
			Assert.Null(build);
		}

		[Fact]
		public void Four_part_hotfix_preview_composition()
		{
			var version = ReleaseGrammar.ParseReleaseVersion("3.119.0.1-preview.1");

			var (@base, build) = version.ValidatePublicVersion("3.119.0.1-preview.1.9");

			Assert.Equal("3.119.0.1", @base);
			Assert.Equal("9", build);
		}

		[Fact]
		public void Compose_public_version_matches_validate()
		{
			var composed = ReleaseVersion.ComposePublicVersion("3.119.0", "preview.1", "42");
			var version = ReleaseGrammar.ParseReleaseVersion("3.119.0-preview.1");

			var (@base, build) = version.ValidatePublicVersion(composed);

			Assert.Equal("3.119.0", @base);
			Assert.Equal("42", build);
		}

		[Fact]
		public void Compose_public_version_rejects_stable_label()
		{
			Assert.Throws<PlanException>(() => ReleaseVersion.ComposePublicVersion("3.119.0", "stable", "1"));
		}

		[Theory]
		[InlineData("5", true)]
		[InlineData("12345.7", true)]
		[InlineData("20250131.3", true)]
		[InlineData("1234.5", false)]
		[InlineData("123456.7", false)]
		[InlineData("1234567.8", false)]
		[InlineData("abc.1", false)]
		[InlineData("1.2.3", false)]
		public void Build_revision_grammar(string candidate, bool expected)
		{
			var match = ReleaseGrammar.BuildRevisionPattern().Match(candidate);
			Assert.Equal(expected, match.Success && match.Length == candidate.Length);
		}
	}
}
