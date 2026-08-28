using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Model;
using Xunit;

namespace SkiaSharp.ReleaseTool.Tests.Model
{
	/// <summary>
	/// Ported test-for-test from Python's <c>tests/test_release_github.py</c>
	/// (<c>TagVersionOrderingTests</c>), the tag-ordering half of which
	/// belongs to the release-version grammar, not the GitHub client that
	/// is out of scope for this slice.
	/// </summary>
	public class TagOrderingTests
	{
		[Fact]
		public void Parses_stable_preview_and_rc()
		{
			Assert.True(ReleaseGrammar.TryParseReleaseTag("v3.119.0", out _));
			Assert.True(ReleaseGrammar.TryParseReleaseTag("v3.119.0-preview.2", out _));
			Assert.True(ReleaseGrammar.TryParseReleaseTag("v3.119.0-rc.1", out _));
			Assert.False(ReleaseGrammar.TryParseReleaseTag("not-a-tag", out _));
		}

		[Fact]
		public void Channel_orders_before_stable()
		{
			ReleaseGrammar.TryParseReleaseTag("v3.119.0-preview.1", out var preview);
			ReleaseGrammar.TryParseReleaseTag("v3.119.0-rc.1", out var rc);
			ReleaseGrammar.TryParseReleaseTag("v3.119.0", out var stable);

			Assert.True(preview!.CompareSortKeyTo(rc!) < 0);
			Assert.True(rc!.CompareSortKeyTo(stable!) < 0);
		}

		[Fact]
		public void Previous_release_tag_across_channels()
		{
			string[] tags = ["v3.119.0-preview.1", "v3.119.0-preview.2", "v3.119.0-rc.1", "v3.118.0"];

			Assert.Equal("v3.119.0-preview.2", TagOrdering.SelectPreviousTag("v3.119.0-rc.1", tags));
			Assert.Equal("v3.118.0", TagOrdering.SelectPreviousTag("v3.119.0-preview.1", tags));
		}

		[Fact]
		public void Previous_release_tag_stable_looks_across_all_channels()
		{
			string[] tags = ["v3.119.0-preview.1", "v3.119.0-rc.1", "v3.119.0"];

			Assert.Equal("v3.119.0-rc.1", TagOrdering.SelectPreviousTag("v3.119.0", tags));
		}

		[Fact]
		public void Previous_release_tag_returns_null_when_first_ever()
		{
			Assert.Null(TagOrdering.SelectPreviousTag("v1.0.0-preview.1", ["v1.0.0-preview.1"]));
		}

		[Fact]
		public void Previous_release_tag_rejects_invalid_current_tag()
		{
			Assert.Throws<PlanException>(() => TagOrdering.SelectPreviousTag("not-a-tag", ["v1.0.0"]));
		}
	}
}
