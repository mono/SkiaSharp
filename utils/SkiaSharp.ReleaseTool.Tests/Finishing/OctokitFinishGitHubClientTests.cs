using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Finishing;
using Xunit;

namespace SkiaSharp.ReleaseTool.Tests.Finishing
{
	public sealed class OctokitFinishGitHubClientTests
	{
		private static readonly Uri DraftUrl =
			new("https://github.com/mono/SkiaSharp/releases/tag/untagged-draft");

		[Fact]
		public async Task Falls_back_to_exact_draft_from_release_listing()
		{
			var listed = false;
			var client = new OctokitFinishGitHubClient(
				(_, _) => Task.FromResult<FinishGitHubRelease?>(null),
				_ =>
				{
					listed = true;
					return Task.FromResult<IReadOnlyList<FinishGitHubRelease>>(
					[
						Release("v4.151.1", isDraft: false),
						Release("v4.152.0-rc.1", isDraft: true),
					]);
				});

			var result = await client.GetReleaseAsync(
				"v4.152.0-rc.1",
				TestContext.Current.CancellationToken);

			Assert.True(listed);
			Assert.NotNull(result);
			Assert.True(result.IsDraft);
			Assert.Equal("v4.152.0-rc.1", result.TagName);
		}

		[Fact]
		public async Task Published_lookup_avoids_release_listing()
		{
			var expected = Release("v4.152.0", isDraft: false);
			var client = new OctokitFinishGitHubClient(
				(_, _) => Task.FromResult<FinishGitHubRelease?>(expected),
				_ => throw new InvalidOperationException("listing should not run"));

			var result = await client.GetReleaseAsync(
				"v4.152.0",
				TestContext.Current.CancellationToken);

			Assert.Same(expected, result);
		}

		[Fact]
		public async Task Multiple_exact_drafts_are_rejected()
		{
			var client = new OctokitFinishGitHubClient(
				(_, _) => Task.FromResult<FinishGitHubRelease?>(null),
				_ => Task.FromResult<IReadOnlyList<FinishGitHubRelease>>(
				[
					Release("v4.152.0", isDraft: true),
					Release("v4.152.0", isDraft: true),
				]));

			await Assert.ThrowsAsync<GitHubException>(() => client.GetReleaseAsync(
				"v4.152.0",
				TestContext.Current.CancellationToken));
		}

		private static FinishGitHubRelease Release(string tag, bool isDraft) =>
			new(
				tag,
				"Version 4.152.0",
				isDraft,
				false,
				new string('a', 40),
				"",
				DraftUrl);
	}
}
