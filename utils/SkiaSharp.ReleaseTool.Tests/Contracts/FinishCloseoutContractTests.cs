using System.Text.Json;
using SkiaSharp.ReleaseTool.Contracts;
using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Json;
using SkiaSharp.ReleaseTool.Model;
using Xunit;

namespace SkiaSharp.ReleaseTool.Tests.Contracts
{
	public sealed class FinishCloseoutContractTests
	{
		[Fact]
		public void Closeout_plan_round_trips_strictly_with_parent_correlation()
		{
			using var directory = new TestDirectory("closeout-contract");
			var path = Path.Combine(directory.Path, "closeout.json");
			var plan = CreatePlan();
			PlanStore.Write(path, plan);

			var roundTrip = PlanStore.ReadCloseout(path, PlanSamples.PlanId);

			Assert.Equal(plan.PlanId, roundTrip.PlanId);
			Assert.Equal(plan.Release, roundTrip.Release);
			Assert.Equal(plan.Dispatches.Select(value => value.Workflow), roundTrip.Dispatches.Select(value => value.Workflow));
			Assert.Equal(
				plan.Dispatches[0].Inputs.OrderBy(value => value.Key),
				roundTrip.Dispatches[0].Inputs.OrderBy(value => value.Key));
			Assert.Throws<ValidationException>(() => PlanStore.ReadCloseout(path, Guid.NewGuid()));
		}

		[Fact]
		public void Unknown_closeout_JSON_fields_are_rejected()
		{
			var json = JsonSerializer.Serialize(
				CreatePlan(),
				ReleaseJsonContext.Strict.FinishCloseoutPlan);
			var invalid = json.Replace("{", "{\"unknown\":true,", StringComparison.Ordinal);

			Assert.Throws<JsonException>(() => JsonSerializer.Deserialize(
				invalid,
				ReleaseJsonContext.Strict.FinishCloseoutPlan));
		}

		[Fact]
		public void Result_requires_all_stable_dispatches()
		{
			var plan = CreatePlan();
			var result = new FinishCloseoutResult(
				1,
				FinishCloseoutOperation.Apply,
				plan.PlanId,
				plan.GeneratedAt,
				plan.ToolingSha,
				FinishCloseoutNextAction.Done,
				plan.Release,
				plan.SourceCommit,
				plan.SourceBranch,
				plan.Tag,
				[],
				[],
				[],
				[plan.Dispatches[0] with { Status = FinishDispatchStatus.Dispatched }],
				[]);

			Assert.Throws<ValidationException>(() => FinishCloseoutResultValidator.Validate(result));
		}

		private static FinishCloseoutPlan CreatePlan()
		{
			var identity = SkiaSharpReleaseIdentity.Parse("4.152.0");
			var release = new FinishReleaseInfo(
				identity.Raw,
				identity.Raw,
				identity.ReleaseBranch,
				identity.Raw,
				identity.Numeric,
				identity.Label,
				identity.ReleaseType,
				identity.Stable,
				identity.Title,
				identity.Tag);
			var dispatches = new[]
			{
				new FinishWorkflowDispatch(
					"update-release-notes.lock.yml",
					"main",
					new Dictionary<string, string>
					{
						["source_branch"] = "main",
						["min_version"] = "4.152.0",
						["max_version"] = "4.152.0",
						["force"] = "false",
					},
					FinishDispatchStatus.Pending),
				new FinishWorkflowDispatch(
					"auto-update-issue-template-versions.yml",
					"main",
					new Dictionary<string, string>(),
					FinishDispatchStatus.Pending),
			};
			return new(
				1,
				FinishCloseoutOperation.Plan,
				PlanSamples.PlanId,
				new DateTimeOffset(2026, 8, 29, 0, 0, 0, TimeSpan.Zero),
				PlanSamples.Sha('a'),
				FinishCloseoutNextAction.Done,
				release,
				PlanSamples.Sha('b'),
				identity.ReleaseBranch,
				identity.Tag,
				[],
				[],
				[],
				dispatches,
				[]);
		}
	}
}
