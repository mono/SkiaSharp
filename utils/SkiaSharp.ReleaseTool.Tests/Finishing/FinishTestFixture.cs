using NuGet.Versioning;
using SkiaSharp.ReleaseTool.Contracts;
using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Finishing;
using SkiaSharp.ReleaseTool.Git;
using SkiaSharp.ReleaseTool.Model;
using SkiaSharp.ReleaseTool.Tests.Git;
using Xunit;

namespace SkiaSharp.ReleaseTool.Tests.Finishing
{
	internal sealed class FinishTestFixture : IDisposable
	{
		public static readonly Guid PlanId =
			Guid.Parse("10db9914-2f93-4e4d-8f4d-227595c7ac7c");
		public static readonly Guid PublicationPlanId =
			Guid.Parse("9e010c46-4992-4ec4-a3a6-6a620d9c69fc");
		public static readonly DateTimeOffset Now =
			new(2026, 8, 29, 0, 0, 0, TimeSpan.Zero);
		public const string Version = "4.152.0";
		public const string Branch = "release/4.152.0";
		public const string Tag = "v4.152.0";
		public const string Title = "Version 4.152.0";

		private FinishTestFixture(
			TestDirectory directory,
			GitRepository repository,
			string sourceCommit)
		{
			Directory = directory;
			Repository = repository;
			SourceCommit = sourceCommit;
			Plan = CreatePlan(sourceCommit);
		}

		public TestDirectory Directory { get; }
		public GitRepository Repository { get; }
		public string SourceCommit { get; }
		public FinishPlan Plan { get; set; }
		public FakeFinishGitHubClient GitHub { get; } = new();
		public FixedTimeProvider TimeProvider { get; } = new(Now);

		public FinishService Service => new(
			Repository,
			GitHub,
			TimeProvider,
			() => PublicationPlanId);

		public static async Task<FinishTestFixture> CreateAsync(string purpose)
		{
			var directory = new TestDirectory(purpose);
			try
			{
				var (_, worktree) = await GitRepoTestHelper.CreateBareAndWorktreeAsync(
					directory.Path,
					"repo",
					TestContext.Current.CancellationToken);
				File.WriteAllText(Path.Combine(worktree, "file.txt"), "source\n");
				var source = await GitRepoTestHelper.CommitAllAsync(
					worktree,
					"source",
					TestContext.Current.CancellationToken);
				var repository = new GitRepository(worktree);
				await repository.PushBranchAsync(
					"main",
					cancellationToken: TestContext.Current.CancellationToken);
				await repository.UpdateLocalBranchAsync(
					Branch,
					source,
					TestContext.Current.CancellationToken);
				await repository.PushBranchAsync(
					Branch,
					cancellationToken: TestContext.Current.CancellationToken);
				await repository.FetchAsync(
					cancellationToken: TestContext.Current.CancellationToken);
				return new FinishTestFixture(directory, repository, source);
			}
			catch
			{
				directory.Dispose();
				throw;
			}
		}

		public async Task EnsureTagAsync() =>
			await Repository.PushTagAsync(
				Tag,
				SourceCommit,
				cancellationToken: TestContext.Current.CancellationToken);

		public FinishGitHubRelease SetRelease(
			string body,
			bool isDraft = true,
			string title = Title,
			string? target = null,
			bool prerelease = false,
			long id = 42)
		{
			var release = new FinishGitHubRelease(
				id,
				Tag,
				title,
				isDraft,
				prerelease,
				target ?? SourceCommit,
				body,
				new Uri($"https://github.com/mono/SkiaSharp/releases/tag/{Tag}"));
			GitHub.Release = release;
			return release;
		}

		public void Dispose() => Directory.Dispose();

		private static FinishPlan CreatePlan(string sourceCommit)
		{
			var identity = SkiaSharpReleaseIdentity.Parse(Version);
			var release = new FinishReleaseInfo(
				identity.Raw,
				Version,
				Branch,
				identity.Raw,
				identity.Numeric,
				identity.Label,
				identity.ReleaseType,
				identity.Stable,
				identity.Title,
				identity.Tag);
			var packages = new[]
			{
				new FinishPackageReceipt("SkiaSharp", Version, sourceCommit, Branch),
				new FinishPackageReceipt("SkiaSharp.HarfBuzz", Version, sourceCommit, Branch),
				new FinishPackageReceipt("HarfBuzzSharp", "14.2.1.200", sourceCommit, Branch),
			};
			var plan = new FinishPlan(
				SchemaVersion: 1,
				Operation: ReleaseOperation.Finish,
				PlanId: PlanId,
				GeneratedAt: Now,
				ToolingSha: sourceCommit,
				NextAction: FinishNextAction.CreateDraft,
				Input: new FinishInput(Version),
				Receipt: new FinishReceiptInfo(
					Version,
					NuGetVersion.Parse(Version).ToNormalizedString(),
					"stable",
					null,
					sourceCommit,
					Branch,
					"14.2.1.200",
					packages),
				Release: release,
				Tag: new FinishTagInfo(
					Tag,
					sourceCommit,
					null,
					FinishState.Pending),
				PreviousTag: "v4.151.1",
				Draft: new FinishDraftInfo(
					false,
					false,
					FinishState.Pending,
					ManagedMarkerState.None,
					null,
					null,
					null),
				Operations:
				[
					new(
						FinishOperationId.CreateTag,
						FinishOperationKind.GitTag,
						PlanOperationStatus.Pending,
						null),
					new(
						FinishOperationId.CreateDraft,
						FinishOperationKind.GitHubRelease,
						PlanOperationStatus.Pending,
						null),
					new(
						FinishOperationId.PublishRelease,
						FinishOperationKind.GitHubRelease,
						PlanOperationStatus.Skipped,
						null),
					new(
						FinishOperationId.Closeout,
						FinishOperationKind.ReleaseCloseout,
						PlanOperationStatus.Skipped,
						null),
				],
				Warnings: []);
			FinishPlanValidator.Validate(plan);
			return plan;
		}
	}

	internal sealed class FakeFinishGitHubClient : IFinishGitHubWriteClient
	{
		public FinishGitHubRelease? Release { get; set; }
		public string GeneratedNotes { get; set; } = "## What's Changed\n\n* generated";
		public int GetCount { get; private set; }
		public int GenerateCount { get; private set; }
		public int CreateCount { get; private set; }
		public int UpdateCount { get; private set; }
		public int PublishCount { get; private set; }
		public int? ThrowOnGetCount { get; set; }
		public (string Tag, string Target, string? Previous)? GeneratedRequest { get; private set; }
		public string? PublishedBody { get; private set; }

		public Task<FinishGitHubRelease?> GetReleaseAsync(
			string tag,
			CancellationToken cancellationToken = default)
		{
			GetCount++;
			if (ThrowOnGetCount == GetCount)
				throw new GitHubException("simulated release reread failure");
			return Task.FromResult(Release);
		}

		public Task<string> GenerateReleaseNotesAsync(
			string tag,
			string targetCommitish,
			string? previousTag,
			CancellationToken cancellationToken = default)
		{
			GenerateCount++;
			GeneratedRequest = (tag, targetCommitish, previousTag);
			return Task.FromResult(GeneratedNotes);
		}

		public Task<FinishGitHubRelease> CreateDraftAsync(
			string tag,
			string title,
			string targetCommitish,
			string body,
			bool prerelease,
			CancellationToken cancellationToken = default)
		{
			CreateCount++;
			Release = new FinishGitHubRelease(
				42,
				tag,
				title,
				true,
				prerelease,
				targetCommitish,
				body,
				new Uri($"https://github.com/mono/SkiaSharp/releases/tag/{tag}"));
			return Task.FromResult(Release);
		}

		public Task<FinishGitHubRelease> UpdateDraftBodyAsync(
			FinishGitHubRelease draft,
			string body,
			CancellationToken cancellationToken = default)
		{
			UpdateCount++;
			Release = draft with { Body = body };
			return Task.FromResult(Release);
		}

		public Task<FinishGitHubRelease> PublishDraftAsync(
			FinishGitHubRelease draft,
			CancellationToken cancellationToken = default)
		{
			PublishCount++;
			PublishedBody = draft.Body;
			Release = draft with { IsDraft = false };
			return Task.FromResult(Release);
		}
	}

	internal sealed class FixedTimeProvider(DateTimeOffset value) : TimeProvider
	{
		public override DateTimeOffset GetUtcNow() => value;
	}
}
