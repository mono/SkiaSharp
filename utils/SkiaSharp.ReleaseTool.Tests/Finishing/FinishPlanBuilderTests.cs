using System.Text.Json;
using NuGet.Versioning;
using SkiaSharp.ReleaseTool.Contracts;
using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Finishing;
using SkiaSharp.ReleaseTool.Model;
using SkiaSharp.ReleaseTool.NuGet;
using SkiaSharp.ReleaseTool.Tests.NuGet;
using Xunit;

namespace SkiaSharp.ReleaseTool.Tests.Finishing
{
	public sealed class FinishPlanBuilderTests
	{
		private const string Commit = "0123456789abcdef0123456789abcdef01234567";
		private static readonly Uri ReleaseUrl = new("https://github.com/mono/SkiaSharp/releases/tag/v4.152.0");

		[Fact]
		public async Task Absent_tag_and_release_route_to_create_draft_and_order_previous_tag()
		{
			var repository = new FinishRepository
			{
				Tags =
				{
					["v4.151.1"] = new string('1', 40),
					["not-a-release"] = new string('2', 40),
				},
			};

			var plan = await BuildAsync(repository, release: null);

			Assert.Equal(FinishNextAction.CreateDraft, plan.NextAction);
			Assert.Equal(FinishState.Pending, plan.Tag.Status);
			Assert.Equal("v4.151.1", plan.PreviousTag);
			Assert.False(plan.Draft.Exists);
		}

		[Fact]
		public async Task Markerless_draft_routes_to_create_draft()
		{
			var release = Release(isDraft: true, body: "legacy draft");

			var plan = await BuildAsync(new FinishRepository(), release);

			Assert.Equal(FinishNextAction.CreateDraft, plan.NextAction);
			Assert.Equal(ManagedMarkerState.None, plan.Draft.MarkerState);
			Assert.Equal("legacy draft", plan.Draft.Body);
			Assert.Equal(ReleaseUrl, plan.Draft.Url);
		}

		[Fact]
		public async Task Marked_draft_routes_to_publication_planning()
		{
			var body =
				$"{ManagedReleaseMarkers.SummaryStart}\n{ManagedReleaseMarkers.SummaryEnd}\n" +
				$"{ManagedReleaseMarkers.GeneratedNotesStart}\nnotes\n{ManagedReleaseMarkers.GeneratedNotesEnd}";

			var plan = await BuildAsync(new FinishRepository(), Release(isDraft: true, body));

			Assert.Equal(FinishNextAction.PlanPublication, plan.NextAction);
			Assert.Equal(ManagedMarkerState.Complete, plan.Draft.MarkerState);
		}

		[Fact]
		public async Task Marked_draft_with_empty_generated_notes_routes_to_repair()
		{
			var body =
				$"{ManagedReleaseMarkers.SummaryStart}\nreviewed\n{ManagedReleaseMarkers.SummaryEnd}\n" +
				$"{ManagedReleaseMarkers.GeneratedNotesStart}\n \n{ManagedReleaseMarkers.GeneratedNotesEnd}";

			var plan = await BuildAsync(
				new FinishRepository(),
				Release(isDraft: true, body));

			Assert.Equal(FinishNextAction.CreateDraft, plan.NextAction);
			Assert.Equal(
				PlanOperationStatus.Pending,
				Assert.Single(
					plan.Operations,
					operation => operation.Id == FinishOperationId.CreateDraft).Status);
			Assert.Equal(
				PlanOperationStatus.Skipped,
				Assert.Single(
					plan.Operations,
					operation => operation.Id == FinishOperationId.PublishRelease).Status);
		}

		[Fact]
		public async Task Published_release_routes_to_closeout_and_accepts_exact_legacy_source_branch()
		{
			var release = Release(
				isDraft: false,
				body: "legacy published notes",
				target: "release/4.152.0");

			var plan = await BuildAsync(new FinishRepository(), release);

			Assert.Equal(FinishNextAction.Closeout, plan.NextAction);
			Assert.True(plan.Draft.IsPublished);
		}

		[Fact]
		public async Task Published_release_accepts_known_legacy_main_target_with_warning()
		{
			var plan = await BuildAsync(
				new FinishRepository(),
				Release(isDraft: false, body: "legacy published notes", target: "main"));

			Assert.Equal(FinishNextAction.Closeout, plan.NextAction);
			Assert.Contains(
				plan.Warnings,
				warning => warning.Contains("target_commitish 'main'", StringComparison.Ordinal));
		}

		[Theory]
		[InlineData("1111111111111111111111111111111111111111", false)]
		[InlineData("release/4.152.0", true)]
		public async Task Wrong_or_unverified_release_targets_block(string target, bool draft)
		{
			await Assert.ThrowsAsync<ConflictException>(() =>
				BuildAsync(new FinishRepository(), Release(draft, "", target)));
		}

		[Fact]
		public async Task Marked_draft_without_tag_routes_back_to_create_draft_stage()
		{
			var body =
				$"{ManagedReleaseMarkers.SummaryStart}\n{ManagedReleaseMarkers.SummaryEnd}\n" +
				$"{ManagedReleaseMarkers.GeneratedNotesStart}\nnotes\n{ManagedReleaseMarkers.GeneratedNotesEnd}";

			var plan = await BuildAsync(
				new FinishRepository(),
				Release(isDraft: true, body),
				addTag: false);

			Assert.Equal(FinishNextAction.CreateDraft, plan.NextAction);
			Assert.Equal(FinishState.Pending, plan.Tag.Status);
			Assert.Equal(
				PlanOperationStatus.Done,
				Assert.Single(
					plan.Operations,
					operation => operation.Id == FinishOperationId.CreateDraft).Status);
		}

		[Fact]
		public async Task Published_release_without_authoritative_tag_blocks()
		{
			await Assert.ThrowsAsync<ConflictException>(() => BuildAsync(
				new FinishRepository(),
				Release(isDraft: false, body: ""),
				addTag: false));
		}

		[Fact]
		public async Task Conflicting_exact_remote_tag_blocks()
		{
			var repository = new FinishRepository();
			repository.Tags["v4.152.0"] = new string('f', 40);

			await Assert.ThrowsAsync<ConflictException>(() =>
				BuildAsync(repository, Release(isDraft: true, body: "")));
		}

		[Theory]
		[InlineData(
			"<!-- SKIASHARP:RELEASE-SUMMARY:START -->",
			"incomplete")]
		[InlineData(
			"<!-- SKIASHARP:RELEASE-SUMMARY:START --><!-- SKIASHARP:RELEASE-SUMMARY:END --><!-- SKIASHARP:GITHUB-GENERATED-NOTES:END --><!-- SKIASHARP:GITHUB-GENERATED-NOTES:START -->",
			"order")]
		public void Malformed_markers_fail_explicitly(string body, string message)
		{
			var error = Assert.Throws<GitHubException>(() => ManagedReleaseMarkers.Inspect(body));
			Assert.Contains(message, error.Message, StringComparison.OrdinalIgnoreCase);
		}

		[Fact]
		public async Task Finish_contract_roundtrips_strictly_and_rejects_unknown_duplicate_and_numeric_enum()
		{
			var plan = await BuildAsync(new FinishRepository(), null);
			var json = JsonSerializer.Serialize(plan, ReleaseJsonContext.Strict.FinishPlan);
			var copy = JsonSerializer.Deserialize(json, ReleaseJsonContext.Strict.FinishPlan);
			Assert.NotNull(copy);
			FinishPlanValidator.Validate(copy);

			Assert.Throws<JsonException>(() => JsonSerializer.Deserialize(
				json.Insert(json.IndexOf('{') + 1, "\"unknown\":true,"),
				ReleaseJsonContext.Strict.FinishPlan));
			Assert.Throws<JsonException>(() => JsonSerializer.Deserialize(
				json.Replace(
					"\"schemaVersion\": 1,",
					"\"schemaVersion\": 1,\"schemaVersion\": 1,",
					StringComparison.Ordinal),
				ReleaseJsonContext.Strict.FinishPlan));
			Assert.Throws<JsonException>(() => JsonSerializer.Deserialize(
				json.Replace("\"nextAction\": \"create-draft\"", "\"nextAction\": 0", StringComparison.Ordinal),
				ReleaseJsonContext.Strict.FinishPlan));
			Assert.Throws<JsonException>(() => JsonSerializer.Deserialize(
				json.Replace($"\"toolingSha\": \"{new string('a', 40)}\"", "\"toolingSha\": null", StringComparison.Ordinal),
				ReleaseJsonContext.Strict.FinishPlan));
		}

		private static async Task<FinishPlan> BuildAsync(
			FinishRepository repository,
			FinishGitHubRelease? release,
			bool addTag = true)
		{
			if (release is not null && addTag)
				repository.Tags.TryAdd("v4.152.0", Commit);
			var builder = new FinishPlanBuilder(
				repository,
				new FixedReceiptVerifier(),
				new FinishGitHubClient { Release = release },
				PackageTestData.Policies(),
				new FixedTimeProvider(),
				() => Guid.Parse("be8cfa29-5c89-46d8-a6bc-f8ec4f5062de"));
			return await builder.BuildAsync(
				new FinishPlanRequest("4.152.0", new string('a', 40)));
		}

		private static FinishGitHubRelease Release(
			bool isDraft,
			string body,
			string target = Commit) =>
			new(
				123,
				"v4.152.0",
				"Version 4.152.0",
				isDraft,
				false,
				target,
				body,
				ReleaseUrl);

		private sealed class FixedReceiptVerifier : IPublicReceiptVerifier
		{
			public Task<PublicReleaseReceipt> VerifyAsync(
				IFinishRepository repository,
				PublicReleaseVersion requestedVersion,
				ReleasePolicies policies,
				CancellationToken cancellationToken) =>
				Task.FromResult(new PublicReleaseReceipt(
					NuGetVersion.Parse("4.152.0"),
					NuGetVersion.Parse("4.152.0"),
					"stable",
					null,
					Commit,
					"release/4.152.0",
					NuGetVersion.Parse("14.2.1.200"),
					[
						Package("SkiaSharp", "4.152.0", Commit, "release/4.152.0"),
						Package("SkiaSharp.HarfBuzz", "4.152.0", Commit, "release/4.152.0"),
						Package(
							"HarfBuzzSharp",
							"14.2.1.200",
							"fedcba9876543210fedcba9876543210fedcba98",
							"release/4.151.1"),
					],
					[]));

			private static VerifiedPackage Package(
				string id,
				string version,
				string commit,
				string branch) =>
				new(id, NuGetVersion.Parse(version), commit, branch, []);
		}

		private sealed class FinishGitHubClient : IFinishGitHubClient
		{
			public FinishGitHubRelease? Release { get; init; }

			public Task<FinishGitHubRelease?> GetReleaseAsync(
				string tag,
				CancellationToken cancellationToken = default) =>
				Task.FromResult(Release);
		}

		private sealed class FinishRepository : IFinishRepository
		{
			public Dictionary<string, string> Tags { get; } = new(StringComparer.Ordinal);

			public Task<IReadOnlyDictionary<string, string>> RemoteTagsAsync(
				string remote = "origin",
				string pattern = "refs/tags/*",
				CancellationToken cancellationToken = default) =>
				Task.FromResult<IReadOnlyDictionary<string, string>>(Tags);

			public Task<bool> RefExistsAsync(string reference, CancellationToken cancellationToken = default) =>
				throw new NotSupportedException();
			public Task<string> ResolveAsync(string reference, CancellationToken cancellationToken = default) =>
				throw new NotSupportedException();
			public Task<bool> CommitExistsAsync(string commit, CancellationToken cancellationToken = default) =>
				throw new NotSupportedException();
			public Task<string> ReadRefFileAsync(string reference, string path, CancellationToken cancellationToken = default) =>
				throw new NotSupportedException();
			public Task<string> ReadGitlinkAsync(string reference, string submodulePath, CancellationToken cancellationToken = default) =>
				throw new NotSupportedException();
			public Task<string?> RemoteShaAsync(string branch, string remote = "origin", CancellationToken cancellationToken = default) =>
				throw new NotSupportedException();
			public Task<IReadOnlyList<string>> ReleaseBranchesAsync(string remote = "origin", CancellationToken cancellationToken = default) =>
				throw new NotSupportedException();
			public Task<bool> IsAncestorAsync(string ancestor, string descendant, CancellationToken cancellationToken = default) =>
				throw new NotSupportedException();
		}

		private sealed class FixedTimeProvider : TimeProvider
		{
			public override DateTimeOffset GetUtcNow() =>
				new(2026, 8, 28, 12, 0, 0, TimeSpan.Zero);
		}
	}
}
