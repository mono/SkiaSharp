using System.Text.Json;
using SkiaSharp.ReleaseTool.Contracts;
using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Json;
using SkiaSharp.ReleaseTool.Planning;
using SkiaSharp.ReleaseTool.Tests.Planning;
using Xunit;

namespace SkiaSharp.ReleaseTool.Tests.Cli
{
	public sealed class ProgramTests
	{
		[Fact]
		public async Task Prepare_plan_writes_and_prints_source_generated_JSON()
		{
			using var root = new TestDirectory("cli-plan");
			var repository = new FakePrepareRepository(root.Path);
			var sha = new string('a', 40);
			repository.AddRef(
				"refs/remotes/origin/main",
				sha,
				new TestVersionState("3.119.0", "1.8.8", "preview.0"));
			var environment = new FakeEnvironment(repository, new FakePrepareGitHubClient());

			var exitCode = await Program.InvokeAsync(
				[
					"prepare", "plan",
					"--integration-target", "main",
					"--repo", root.Path,
				],
				environment);

			Assert.Equal(ExitCodes.Success, exitCode);
			Assert.True(repository.FetchCalled);
			Assert.Equal(root.Path, environment.OpenedPath);
			var outputPath = Path.Combine(root.Path, "prepare-plan.json");
			Assert.True(File.Exists(outputPath));
			var printed = JsonSerializer.Deserialize(
				environment.Output.ToString(),
				ReleaseJsonContext.Strict.PreparePlan);
			Assert.NotNull(printed);
			Assert.Equal("3.119.0-preview.1", printed.Release.Version);
			Assert.Equivalent(
				printed,
				PlanStore.ReadPrepare(outputPath, printed.PlanId),
				strict: true);
			Assert.Empty(environment.Error.ToString());
		}

		[Fact]
		public async Task Release_tool_errors_are_clean_and_map_to_one()
		{
			using var root = new TestDirectory("cli-error");
			var repository = new FakePrepareRepository(root.Path);
			var sha = new string('a', 40);
			repository.AddRef(
				"refs/remotes/origin/main",
				sha,
				new TestVersionState("3.119.0", "1.8.8", "preview.0"));
			var environment = new FakeEnvironment(repository, new FakePrepareGitHubClient());

			var exitCode = await Program.InvokeAsync(
				[
					"prepare", "plan",
					"--integration-target", "main",
					"--version", "invalid",
				],
				environment);

			Assert.Equal(ExitCodes.GenericError, exitCode);
			Assert.StartsWith("error: invalid release version", environment.Error.ToString());
			Assert.DoesNotContain(" at ", environment.Error.ToString(), StringComparison.Ordinal);
		}

		[Fact]
		public async Task Cancellation_maps_to_distinct_nonzero_exit()
		{
			var environment = new CancelingEnvironment();

			var exitCode = await Program.InvokeAsync(
				["prepare", "plan", "--integration-target", "main"],
				environment);

			Assert.Equal(ExitCodes.Canceled, exitCode);
			Assert.Equal("error: operation canceled" + Environment.NewLine, environment.Error.ToString());
		}

		[Fact]
		public async Task Prepare_apply_requires_correlation_and_writes_typed_result()
		{
			using var fixture = await PreparePlanApplierTests.ApplyFixture.CreateAsync(
				"cli-apply");
			var plan = await fixture.PlanAsync();
			var planPath = Path.Combine(fixture.Repository.Root, "approved-plan.json");
			PlanStore.Write(planPath, plan);
			var environment = new FakeEnvironment(
				fixture.Repository,
				fixture.GitHub);

			var missingCorrelation = await Program.InvokeAsync(
				["prepare", "apply", "--plan", "approved-plan.json"],
				environment);
			Assert.NotEqual(ExitCodes.Success, missingCorrelation);

			var mismatch = await Program.InvokeAsync(
				[
					"prepare", "apply",
					"--plan", "approved-plan.json",
					"--expected-plan-id", Guid.NewGuid().ToString(),
				],
				environment);
			Assert.Equal(ExitCodes.GenericError, mismatch);

			environment.Error.GetStringBuilder().Clear();
			var exitCode = await Program.InvokeAsync(
				[
					"prepare", "apply",
					"--plan", "approved-plan.json",
					"--expected-plan-id", plan.PlanId.ToString(),
					"--output", "apply-result.json",
				],
				environment);

			Assert.Equal(ExitCodes.Success, exitCode);
			var outputPath = Path.Combine(
				fixture.Repository.Root,
				"apply-result.json");
			var result = JsonSerializer.Deserialize(
				File.ReadAllText(outputPath),
				ReleaseJsonContext.Strict.PrepareApplyResult);
			Assert.NotNull(result);
			Assert.Equal(plan.PlanId, result.PlanId);
			PrepareApplyResultValidator.Validate(result);
		}

		private sealed class FakeEnvironment(
			IReleaseRepository repository,
			IPrepareGitHubClient github) : IReleaseCommandEnvironment
		{
			public StringWriter Output { get; } = new();
			public StringWriter Error { get; } = new();
			public string? OpenedPath { get; private set; }
			public TextWriter StandardOutput => Output;
			public TextWriter StandardError => Error;
			public TimeProvider TimeProvider { get; } =
				new FixedTimeProvider(new DateTimeOffset(2026, 8, 28, 12, 0, 0, TimeSpan.Zero));
			public Func<Guid> NewPlanId => () =>
				Guid.Parse("4965cc7e-38e9-4ce5-af34-356e8c74aa7e");

			public Task<IReleaseRepository> OpenRepositoryAsync(
				string? path,
				CancellationToken cancellationToken)
			{
				OpenedPath = path;
				return Task.FromResult(repository);
			}

			public IPrepareGitHubClient CreateGitHubClient() => github;
		}

		private sealed class CancelingEnvironment : IReleaseCommandEnvironment
		{
			public StringWriter Error { get; } = new();
			public TextWriter StandardOutput => TextWriter.Null;
			public TextWriter StandardError => Error;
			public TimeProvider TimeProvider => TimeProvider.System;
			public Func<Guid> NewPlanId => Guid.NewGuid;

			public Task<IReleaseRepository> OpenRepositoryAsync(
				string? path,
				CancellationToken cancellationToken) =>
				Task.FromException<IReleaseRepository>(new OperationCanceledException());

			public IPrepareGitHubClient CreateGitHubClient() =>
				throw new InvalidOperationException();
		}

		private sealed class FixedTimeProvider(DateTimeOffset value) : TimeProvider
		{
			public override DateTimeOffset GetUtcNow() => value;
		}
	}
}
