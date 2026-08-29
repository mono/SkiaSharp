using System.Text.Json;
using SkiaSharp.ReleaseTool.Contracts;
using SkiaSharp.ReleaseTool.Environments;
using SkiaSharp.ReleaseTool.Finishing;
using SkiaSharp.ReleaseTool.Json;
using SkiaSharp.ReleaseTool.Milestones;
using SkiaSharp.ReleaseTool.NuGet;
using SkiaSharp.ReleaseTool.Planning;
using SkiaSharp.ReleaseTool.Tests.Finishing;
using Xunit;

namespace SkiaSharp.ReleaseTool.Tests.Cli
{
	public sealed class CloseoutProgramTests
	{
		[Fact]
		public async Task Dry_run_writes_strict_closeout_plan_without_dispatching()
		{
			using var directory = new TestDirectory("closeout-cli-plan");
			var plan = FinishCloseoutServiceTests.CreatePlan(stable: true, previousTag: null);
			PlanStore.Write(Path.Combine(directory.Path, "finish.json"), plan);
			var environment = new CloseoutEnvironment(
				new FakeCloseoutRepository(plan, directory.Path),
				CreateGitHub(plan),
				new FakeChromiumScheduleClient());

			var exit = await Program.InvokeAsync(
				[
					"--repo", directory.Path,
					"finish", "closeout",
					"--plan", "finish.json",
					"--expected-plan-id", plan.PlanId.ToString(),
					"--dry-run",
					"--output", "closeout.json",
				],
				environment);

			Assert.Equal(0, exit);
			var json = await File.ReadAllTextAsync(
				Path.Combine(directory.Path, "closeout.json"),
				TestContext.Current.CancellationToken);
			var result = JsonSerializer.Deserialize(
				json,
				ReleaseJsonContext.Strict.FinishCloseoutPlan);
			Assert.NotNull(result);
			Assert.Equal(plan.PlanId, result.PlanId);
			Assert.Empty(environment.GitHub.Dispatches);
		}

		[Fact]
		public async Task Apply_writes_result_and_wrong_correlation_is_rejected()
		{
			using var directory = new TestDirectory("closeout-cli-apply");
			var plan = FinishCloseoutServiceTests.CreatePlan(stable: true, previousTag: null);
			PlanStore.Write(Path.Combine(directory.Path, "finish.json"), plan);
			var environment = new CloseoutEnvironment(
				new FakeCloseoutRepository(plan, directory.Path),
				CreateGitHub(plan),
				new FakeChromiumScheduleClient());

			var rejected = await Program.InvokeAsync(
				[
					"--repo", directory.Path,
					"finish", "closeout",
					"--plan", "finish.json",
					"--expected-plan-id", Guid.NewGuid().ToString(),
					"--output", "rejected.json",
				],
				environment);
			var applied = await Program.InvokeAsync(
				[
					"--repo", directory.Path,
					"finish", "closeout",
					"--plan", "finish.json",
					"--expected-plan-id", plan.PlanId.ToString(),
					"--output", "result.json",
				],
				environment);

			Assert.Equal(1, rejected);
			Assert.False(File.Exists(Path.Combine(directory.Path, "rejected.json")));
			Assert.Equal(0, applied);
			Assert.Equal(2, environment.GitHub.Dispatches.Count);
			var result = JsonSerializer.Deserialize(
				await File.ReadAllTextAsync(
					Path.Combine(directory.Path, "result.json"),
					TestContext.Current.CancellationToken),
				ReleaseJsonContext.Strict.FinishCloseoutResult);
			Assert.NotNull(result);
			Assert.Equal(FinishCloseoutNextAction.Done, result.NextAction);
		}

		private static FakeCloseoutGitHubClient CreateGitHub(FinishPlan plan) =>
			new()
			{
				Release = new(
					42,
					plan.Tag.Name,
					plan.Release.Title,
					false,
					!plan.Release.Stable,
					plan.Receipt.SourceCommit,
					"",
					new Uri($"https://github.com/mono/SkiaSharp/releases/tag/{plan.Tag.Name}")),
			};

		private sealed class CloseoutEnvironment(
			IReleaseRepository repository,
			FakeCloseoutGitHubClient github,
			FakeChromiumScheduleClient chromium) : IReleaseCommandEnvironment
		{
			public FakeCloseoutGitHubClient GitHub => github;
			public StringWriter Output { get; } = new();
			public StringWriter Error { get; } = new();
			public TextWriter StandardOutput => Output;
			public TextWriter StandardError => Error;
			public TimeProvider TimeProvider { get; } =
				new FixedTimeProvider(new DateTimeOffset(2026, 8, 29, 12, 0, 0, TimeSpan.Zero));
			public Func<Guid> NewPlanId => Guid.NewGuid;
			public Task<IReleaseRepository> OpenRepositoryAsync(
				string? path,
				CancellationToken cancellationToken) =>
				Task.FromResult(repository);
			public IPrepareGitHubClient CreateGitHubClient() => throw new NotSupportedException();
			public ICloseoutGitHubClient CreateCloseoutGitHubClient() => github;
			public IChromiumScheduleClient CreateChromiumScheduleClient() => chromium;
		}
	}
}
