using System.Text.Json;
using SkiaSharp.ReleaseTool.Contracts;
using SkiaSharp.ReleaseTool.Environments;
using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Finishing;
using SkiaSharp.ReleaseTool.Json;
using SkiaSharp.ReleaseTool.Model;
using SkiaSharp.ReleaseTool.Planning;
using SkiaSharp.ReleaseTool.Tests.Contracts;
using SkiaSharp.ReleaseTool.Tests.Finishing;
using Xunit;

namespace SkiaSharp.ReleaseTool.Tests.Cli
{
	public sealed class RenderPlanProgramTests
	{
		[Fact]
		public async Task Every_release_artifact_renders_as_strict_JSON_and_markdown()
		{
			using var directory = new TestDirectory("render-plan");
			using var fixture = await FinishTestFixture.CreateAsync("render-finish");
			var createDraft = await fixture.Service.CreateDraftAsync(
				fixture.Plan,
				fixture.Plan.PlanId,
				TestContext.Current.CancellationToken);
			var publication = await fixture.Service.PlanPublicationAsync(
				fixture.Plan,
				fixture.Plan.PlanId,
				TestContext.Current.CancellationToken);
			var publish = await fixture.Service.PublishAsync(
				fixture.Plan,
				fixture.Plan.PlanId,
				publication,
				publication.PublicationPlanId,
				TestContext.Current.CancellationToken);
			var prepare = PlanSamples.Prepare();
			var prepareResult = new PrepareApplyResult(
				1,
				prepare.PlanId,
				prepare.ToolingSha,
				PrepareNextAction.Done,
				prepare.Release,
				[
					new(PlanOperationId.CreateMaintenanceBranch, ApplyOperationStatus.Done, null),
					new(PlanOperationId.CreateSkiaRef, ApplyOperationStatus.Done, null),
					new(PlanOperationId.CreateReleaseBranch, ApplyOperationStatus.Done, null),
				],
				null,
				[]);
			var pending = new FinishPendingReport(
				1,
				FinishPendingOperation.FinishPlanPending,
				FinishTestFixture.Now,
				fixture.Plan.ToolingSha,
				PendingNextAction.Pending,
				FinishTestFixture.Version,
				[new PendingPackage("SkiaSharp", FinishTestFixture.Version)],
				60,
				60,
				"NuGet.org is still indexing");
			var closeoutPlan = CloseoutPlan(fixture.Plan, FinishDispatchStatus.Pending);
			var closeoutResult = new FinishCloseoutResult(
				closeoutPlan.SchemaVersion,
				FinishCloseoutOperation.Apply,
				closeoutPlan.PlanId,
				closeoutPlan.GeneratedAt,
				closeoutPlan.ToolingSha,
				FinishCloseoutNextAction.Done,
				closeoutPlan.Release,
				closeoutPlan.SourceCommit,
				closeoutPlan.SourceBranch,
				closeoutPlan.Tag,
				[],
				[],
				[],
				closeoutPlan.Dispatches
					.Select(value => value with { Status = FinishDispatchStatus.Dispatched })
					.ToArray(),
				[]);
			var environment = GitHubEnvironmentPolicy.Check(
				new GitHubEnvironmentSnapshot(
					"release-tag",
					["required_reviewers", "branch_policy"],
					new EnvironmentRequiredReviewers(1, true),
					false,
					true,
					[new EnvironmentBranchPolicy("main", "branch")]),
				"release-tag",
				"main");

			var artifacts = new (string Name, string Json, string Expected)[]
			{
				("prepare", Json(prepare, ReleaseJsonContext.Strict.PreparePlan), prepare.PlanId.ToString()),
				("prepare-result", Json(prepareResult, ReleaseJsonContext.Strict.PrepareApplyResult), prepare.PlanId.ToString()),
				("finish", Json(fixture.Plan, ReleaseJsonContext.Strict.FinishPlan), fixture.Plan.PlanId.ToString()),
				("pending", Json(pending, ReleaseJsonContext.Strict.FinishPendingReport), "Missing packages"),
				("draft", Json(createDraft, ReleaseJsonContext.Strict.FinishCreateDraftResult), createDraft.ReleaseUrl.ToString()),
				("publication", Json(publication, ReleaseJsonContext.Strict.FinishPublicationPlan), publication.PublicationPlanId.ToString()),
				("publish", Json(publish, ReleaseJsonContext.Strict.FinishPublishResult), publish.PublicationPlanId.ToString()),
				("closeout-plan", Json(closeoutPlan, ReleaseJsonContext.Strict.FinishCloseoutPlan), "Workflow dispatch"),
				("closeout-result", Json(closeoutResult, ReleaseJsonContext.Strict.FinishCloseoutResult), "Workflow dispatch"),
				("environment", Json(environment, EnvironmentJsonContext.Strict.EnvironmentCheckReport), "Policy satisfied"),
			};

			foreach (var artifact in artifacts)
			{
				var input = Path.Combine(directory.Path, $"{artifact.Name}.json");
				var jsonOutput = Path.Combine(directory.Path, $"{artifact.Name}.rendered.json");
				var markdownOutput = Path.Combine(directory.Path, $"{artifact.Name}.md");
				await File.WriteAllTextAsync(
					input,
					artifact.Json,
					TestContext.Current.CancellationToken);
				var commandEnvironment = new RenderEnvironment();

				var jsonExit = await Program.InvokeAsync(
					[
						"render-plan",
						"--plan", input,
						"--format", "json",
						"--output", jsonOutput,
					],
					commandEnvironment);
				var markdownExit = await Program.InvokeAsync(
					[
						"render-plan",
						"--plan", input,
						"--format", "markdown",
						"--output", markdownOutput,
					],
					commandEnvironment);

				Assert.Equal(ExitCodes.Success, jsonExit);
				Assert.Equal(ExitCodes.Success, markdownExit);
				Assert.Equal(
					artifact.Json + Environment.NewLine,
					await File.ReadAllTextAsync(
						jsonOutput,
						TestContext.Current.CancellationToken));
				Assert.Contains(
					artifact.Expected,
					await File.ReadAllTextAsync(
						markdownOutput,
						TestContext.Current.CancellationToken));
				Assert.Empty(commandEnvironment.Error.ToString());
			}
		}

		[Theory]
		[InlineData("{", "header failed shape validation")]
		[InlineData("""{"schemaVersion":1,"operation":"unknown"}""", "unknown release artifact operation")]
		[InlineData("""{"name":"release-tag","exists":true,"ok":true}""", "artifact failed shape validation")]
		public async Task Malformed_and_unknown_artifacts_are_rejected(
			string json,
			string message)
		{
			using var directory = new TestDirectory("render-invalid");
			var input = Path.Combine(directory.Path, "invalid.json");
			await File.WriteAllTextAsync(input, json, TestContext.Current.CancellationToken);
			var environment = new RenderEnvironment();

			var exit = await Program.InvokeAsync(
				[
					"render-plan",
					"--plan", input,
					"--format", "json",
					"--output", Path.Combine(directory.Path, "output.json"),
				],
				environment);

			Assert.Equal(ExitCodes.GenericError, exit);
			Assert.Contains(message, environment.Error.ToString());
		}

		[Fact]
		public async Task Mismatched_and_duplicate_discriminators_are_rejected()
		{
			using var directory = new TestDirectory("render-mismatch");
			var finish = Json(
				FinishCloseoutServiceTests.CreatePlan(stable: true, previousTag: null),
				ReleaseJsonContext.Strict.FinishPlan);
			var mismatched = finish.Replace(
				"\"operation\": \"finish\"",
				"\"operation\": \"finish-create-draft\"",
				StringComparison.Ordinal);
			var duplicate = finish.Replace(
				"\"operation\": \"finish\"",
				"\"operation\": \"finish\",\n  \"operation\": \"finish\"",
				StringComparison.Ordinal);

			foreach (var (name, json) in new[]
			{
				("mismatched", mismatched),
				("duplicate", duplicate),
			})
			{
				var input = Path.Combine(directory.Path, $"{name}.json");
				await File.WriteAllTextAsync(input, json, TestContext.Current.CancellationToken);
				var environment = new RenderEnvironment();

				var exit = await Program.InvokeAsync(
					[
						"render-plan",
						"--plan", input,
						"--format", "markdown",
						"--output", Path.Combine(directory.Path, $"{name}.md"),
					],
					environment);

				Assert.Equal(ExitCodes.GenericError, exit);
				Assert.Contains("shape validation", environment.Error.ToString());
			}
		}

		[Fact]
		public async Task Unsupported_format_does_not_write_output()
		{
			using var directory = new TestDirectory("render-format");
			var input = Path.Combine(directory.Path, "prepare.json");
			var output = Path.Combine(directory.Path, "output.txt");
			await File.WriteAllTextAsync(
				input,
				Json(PlanSamples.Prepare(), ReleaseJsonContext.Strict.PreparePlan),
				TestContext.Current.CancellationToken);
			var environment = new RenderEnvironment();

			var exit = await Program.InvokeAsync(
				[
					"render-plan",
					"--plan", input,
					"--format", "html",
					"--output", output,
				],
				environment);

			Assert.Equal(ExitCodes.GenericError, exit);
			Assert.False(File.Exists(output));
			Assert.Contains("unsupported render format", environment.Error.ToString());
		}

		private static string Json<T>(
			T value,
			System.Text.Json.Serialization.Metadata.JsonTypeInfo<T> typeInfo) =>
			JsonSerializer.Serialize(value, typeInfo);

		private static FinishCloseoutPlan CloseoutPlan(
			FinishPlan finish,
			FinishDispatchStatus status)
		{
			var inputs = new Dictionary<string, string>
			{
				["source_branch"] = "main",
				["min_version"] = finish.Release.Numeric,
				["max_version"] = finish.Release.Numeric,
				["force"] = "false",
			};
			return new(
				1,
				FinishCloseoutOperation.Plan,
				finish.PlanId,
				finish.GeneratedAt,
				finish.ToolingSha,
				FinishCloseoutNextAction.Done,
				finish.Release,
				finish.Receipt.SourceCommit,
				finish.Receipt.SourceBranch,
				finish.Release.Tag,
				[],
				[],
				[],
				[
					new FinishWorkflowDispatch(
						"update-release-notes.lock.yml",
						"main",
						inputs,
						status),
					new FinishWorkflowDispatch(
						"auto-update-issue-template-versions.yml",
						"main",
						new Dictionary<string, string>(),
						status),
				],
				[]);
		}

		private sealed class RenderEnvironment : IReleaseCommandEnvironment
		{
			public StringWriter Error { get; } = new();
			public TextWriter StandardOutput => TextWriter.Null;
			public TextWriter StandardError => Error;
			public TimeProvider TimeProvider => TimeProvider.System;
			public Func<Guid> NewPlanId => Guid.NewGuid;

			public Task<IReleaseRepository> OpenRepositoryAsync(
				string? path,
				CancellationToken cancellationToken) =>
				throw new NotSupportedException();

			public IPrepareGitHubClient CreateGitHubClient() =>
				throw new NotSupportedException();
		}
	}
}
