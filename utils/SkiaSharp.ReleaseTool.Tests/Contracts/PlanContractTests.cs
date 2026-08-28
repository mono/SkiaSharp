using System.Text;
using System.Text.Json;
using SkiaSharp.ReleaseTool.Contracts;
using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Json;
using SkiaSharp.ReleaseTool.Model;
using Xunit;

namespace SkiaSharp.ReleaseTool.Tests.Contracts
{
	public sealed class PlanContractTests
	{
		[Fact]
		public void Source_generated_round_trip_works_with_reflection_disabled()
		{
			Assert.False(JsonSerializer.IsReflectionEnabledByDefault);
			var json = JsonSerializer.Serialize(
				PlanSamples.Prepare(),
				ReleaseJsonContext.Strict.PreparePlan);
			var copy = JsonSerializer.Deserialize(
				json,
				ReleaseJsonContext.Strict.PreparePlan);

			Assert.NotNull(copy);
			Assert.Equal(PlanSamples.PlanId, copy.PlanId);
			Assert.Contains("\"nextAction\": \"done\"", json);
			Assert.DoesNotContain("\"numeric\":", json, StringComparison.Ordinal);
			Assert.DoesNotContain("\"releaseType\":", json, StringComparison.Ordinal);
			Assert.DoesNotContain("\"integrationBranch\":", json, StringComparison.Ordinal);
			Assert.DoesNotContain("\"releaseBranch\":", json, StringComparison.Ordinal);
			Assert.Throws<InvalidOperationException>(
				() => JsonSerializer.Serialize(new UnregisteredType("reflection is disabled")));
		}

		[Theory]
		[MemberData(nameof(InvalidJsonShapes))]
		public void Strict_deserialization_rejects_invalid_shape(Func<string, string> mutate)
		{
			var json = JsonSerializer.Serialize(
				PlanSamples.Prepare(),
				ReleaseJsonContext.Strict.PreparePlan);

			Assert.Throws<JsonException>(
				() => JsonSerializer.Deserialize(
					mutate(json),
					ReleaseJsonContext.Strict.PreparePlan));
		}

		public static TheoryData<Func<string, string>> InvalidJsonShapes() => new()
		{
			json => json.Replace(
				"  \"toolingSha\": \"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa\",\n",
				"",
				StringComparison.Ordinal),
			json => json.Replace(
				"\"toolingSha\": \"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa\"",
				"\"toolingSha\": null",
				StringComparison.Ordinal),
			json => json.Insert(json.IndexOf('{') + 1, "\n  \"unknown\": true,"),
			json => json.Replace(
				"  \"schemaVersion\": 1,",
				"  \"schemaVersion\": 1,\n  \"schemaVersion\": 1,",
				StringComparison.Ordinal),
			json => json.Replace(
				"\"operation\": \"prepare\"",
				"\"operation\": \"launch\"",
				StringComparison.Ordinal),
			json => json.Replace(
				"\"operation\": \"prepare\"",
				"\"operation\": 0",
				StringComparison.Ordinal),
			json => json.Replace(
				"\"generatedAt\": \"2026-08-28T12:00:00+00:00\"",
				"\"generatedAt\": \"not-a-date\"",
				StringComparison.Ordinal),
		};

		[Theory]
		[MemberData(nameof(InvalidPreparePlans))]
		public void Prepare_semantic_validation_rejects_inconsistent_plans(
			PreparePlan plan,
			string message)
		{
			var exception = Assert.Throws<ValidationException>(
				() => PreparePlanValidator.Validate(plan));
			Assert.Contains(message, exception.Message, StringComparison.OrdinalIgnoreCase);
		}

		public static TheoryData<PreparePlan, string> InvalidPreparePlans()
		{
			var valid = PlanSamples.Prepare();
			return new TheoryData<PreparePlan, string>
			{
				{ valid with { SchemaVersion = 2 }, "schemaVersion" },
				{ valid with { Operation = (ReleaseOperation)999 }, "operation" },
				{ valid with { PlanId = Guid.Empty }, "planId" },
				{ valid with { GeneratedAt = valid.GeneratedAt.ToOffset(TimeSpan.FromHours(2)) }, "UTC" },
				{ valid with { ToolingSha = "not-a-sha" }, "40-hex" },
				{ valid with { ToolingSha = new string('A', 40) }, "lowercase" },
				{ valid with { Release = valid.Release with { Identity = "3.119" } }, "invalid release version" },
				{ valid with { Release = valid.Release with { Branch = "release/wrong" } }, "release.branch" },
				{ valid with { Input = valid.Input with { IntegrationTarget = "feature/unsafe" } }, "integration target" },
				{ valid with { Input = valid.Input with { IntegrationTarget = "origin/release/3.119.x" } }, "normalized" },
				{ valid with { Input = valid.Input with { IntegrationTarget = "release/3.118.x" } }, "release.integrationBranch" },
				{ valid with { Operations = [null!] }, "null values" },
				{ valid with { Warnings = null! }, "warnings" },
				{
					valid with
					{
						SkiaSharpRemoteState = RemoteState.Missing,
						NextAction = PrepareNextAction.Done,
						Operations = valid.Operations
							.Select(operation => operation.Id == PlanOperationId.CreateReleaseBranch
								? operation with { Status = PlanOperationStatus.Pending }
								: operation)
							.ToArray(),
					},
					"nextAction"
				},
			};
		}

		[Fact]
		public void Hotfix_preview_and_stable_oracle_plans_validate()
		{
			var preview = HotfixPlan("3.119.0.1-preview.1", "refs/tags/v3.119.0");
			PreparePlanValidator.Validate(preview);
			Assert.Equal(PlanOperationStatus.Skipped, preview.Operations[0].Status);

			var stable = HotfixPlan(
				"3.119.0.1",
				"refs/remotes/origin/release/3.119.0.1-rc.1");
			PreparePlanValidator.Validate(stable);
			Assert.Null(stable.StableBump);
		}

		[Fact]
		public void Hotfix_preview_requires_exact_stable_parent_tag()
		{
			var valid = HotfixPlan("3.119.0.1-preview.1", "refs/tags/v3.119.0");

			Assert.Throws<ValidationException>(
				() => PreparePlanValidator.Validate(valid with
				{
					Base = valid.Base with { Ref = "refs/tags/v3.119.0-preview.1" },
				}));
			Assert.Throws<ValidationException>(
				() => PreparePlanValidator.Validate(valid with
				{
					Base = valid.Base with { Ref = "refs/tags/v3.119.0.1" },
				}));
		}

		[Fact]
		public void Stable_prepare_plan_requires_a_consistent_bump()
		{
			var plan = PlanSamples.StablePrepare();
			PreparePlanValidator.Validate(plan);

			var invalid = plan with
			{
				StableBump = plan.StableBump! with { SkiaSharpVersion = "3.119.2" },
			};
			Assert.Throws<ValidationException>(
				() => PreparePlanValidator.Validate(invalid));

			var invalidUri = plan with
			{
				StableBump = plan.StableBump! with
				{
					PullRequestUrl = new Uri("relative", UriKind.Relative),
				},
			};
			Assert.Throws<ValidationException>(
				() => PreparePlanValidator.Validate(invalidUri));
		}

		[Fact]
		public void Plan_store_validates_writes_reads_and_correlation()
		{
			using var root = new TestDirectory("plan-store");
			var path = Path.Combine(root.Path, "nested", "plan.json");
			var plan = PlanSamples.Prepare();

			PlanStore.Write(path, plan);
			var bytes = File.ReadAllBytes(path);
			Assert.False(bytes.AsSpan().StartsWith(new byte[] { 0xef, 0xbb, 0xbf }));
			Assert.Contains("\n  \"planId\":", Encoding.UTF8.GetString(bytes));
			Assert.Equal(plan.PlanId, PlanStore.ReadPrepare(path, plan.PlanId).PlanId);

			Assert.Throws<ValidationException>(
				() => PlanStore.ReadPrepare(path, Guid.NewGuid()));
			Assert.Throws<ValidationException>(
				() => PlanStore.Write(path, plan with { ToolingSha = "bad" }));
		}

		[Fact]
		public void Plan_store_rejects_unknown_duplicate_and_semantically_invalid_members()
		{
			using var root = new TestDirectory("plan-store-invalid");
			var path = Path.Combine(root.Path, "plan.json");
			var json = JsonSerializer.Serialize(
				PlanSamples.Prepare(),
				ReleaseJsonContext.Strict.PreparePlan);

			File.WriteAllText(path, json.Insert(json.IndexOf('{') + 1, "\n  \"unknown\": true,"));
			Assert.Throws<ValidationException>(
				() => PlanStore.ReadPrepare(path, PlanSamples.PlanId));

			File.WriteAllText(
				path,
				json.Replace(
					"\"toolingSha\": \"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa\"",
					"\"toolingSha\": \"bad\"",
					StringComparison.Ordinal));
			Assert.Throws<ValidationException>(
				() => PlanStore.ReadPrepare(path, PlanSamples.PlanId));
		}

		private sealed record UnregisteredType(string Value);

		private static PreparePlan HotfixPlan(string version, string baseRef)
		{
			var source = PlanSamples.Prepare();
			var identity = SkiaSharpReleaseIdentity.Parse(version);
			return source with
			{
				Input = new PrepareInput("main", version, null),
				Release = new PrepareReleaseInfo(
					version,
					version,
					identity.ReleaseBranch),
				Base = new PrepareBaseInfo(baseRef, PlanSamples.Sha('b')),
				MaintenanceBranch = new MaintenanceBranchInfo(
					identity.IntegrationBranch,
					false,
					MaintenanceBranchAction.None,
					null),
				Skia = new PrepareSkiaInfo(
					PlanSamples.Sha('c'),
					RemoteState.Matching),
				Versions = new PrepareVersionsInfo(false),
				Operations =
				[
					new PlanOperation(
						PlanOperationId.CreateMaintenanceBranch,
						PlanOperationKind.GitRef,
						PlanOperationStatus.Skipped,
						identity.IntegrationBranch),
					new PlanOperation(
						PlanOperationId.CreateSkiaRef,
						PlanOperationKind.GitHubRef,
						PlanOperationStatus.Done,
						null),
					new PlanOperation(
						PlanOperationId.CreateReleaseBranch,
						PlanOperationKind.GitRef,
						PlanOperationStatus.Done,
						null),
				],
				StableBump = null,
			};
		}
	}
}
