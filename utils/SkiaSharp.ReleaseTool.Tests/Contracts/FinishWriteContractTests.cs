using System.Text.Json;
using SkiaSharp.ReleaseTool.Contracts;
using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Finishing;
using SkiaSharp.ReleaseTool.Json;
using SkiaSharp.ReleaseTool.Tests.Finishing;
using Xunit;

namespace SkiaSharp.ReleaseTool.Tests.Contracts
{
	public sealed class FinishWriteContractTests
	{
		[Fact]
		public async Task Finish_and_publication_store_reads_require_correlations_and_digests()
		{
			using var fixture = await FinishTestFixture.CreateAsync("finish-store");
			await fixture.EnsureTagAsync();
			fixture.SetRelease(ManagedReleaseMarkers.BuildInitialBody("notes"));
			var publication = await fixture.Service.PlanPublicationAsync(
				fixture.Plan,
				fixture.Plan.PlanId,
				TestContext.Current.CancellationToken);
			var finishPath = Path.Combine(fixture.Repository.Root, "finish.json");
			var publicationPath = Path.Combine(fixture.Repository.Root, "publication.json");

			PlanStore.Write(finishPath, fixture.Plan);
			PlanStore.Write(publicationPath, publication);
			var finishHash = ArtifactHash.ComputeFile(finishPath);
			var publicationHash = ArtifactHash.ComputeFile(publicationPath);

			Assert.Equal(
				fixture.Plan.PlanId,
				PlanStore.ReadFinish(
					finishPath,
					fixture.Plan.PlanId,
					finishHash).PlanId);
			Assert.Equal(
				publication.PublicationPlanId,
				PlanStore.ReadPublication(
					publicationPath,
					fixture.Plan.PlanId,
					publication.PublicationPlanId,
					publicationHash).PublicationPlanId);
			Assert.Throws<ValidationException>(() =>
				PlanStore.ReadFinish(finishPath, Guid.NewGuid(), finishHash));
			Assert.Throws<ValidationException>(() =>
				PlanStore.ReadPublication(
					publicationPath,
					Guid.NewGuid(),
					publication.PublicationPlanId,
					publicationHash));
			Assert.Throws<ValidationException>(() =>
				PlanStore.ReadPublication(
					publicationPath,
					fixture.Plan.PlanId,
					Guid.NewGuid(),
					publicationHash));
			Assert.Throws<ValidationException>(() =>
				PlanStore.ReadPublication(
					publicationPath,
					fixture.Plan.PlanId,
					publication.PublicationPlanId,
					new string('0', 64)));
			Assert.DoesNotContain(
				Directory.EnumerateFiles(
					fixture.Repository.Root,
					"*.writing",
					SearchOption.AllDirectories),
				static _ => true);
		}

		[Fact]
		public async Task All_finish_write_artifacts_roundtrip_with_source_generated_strict_JSON()
		{
			using var fixture = await FinishTestFixture.CreateAsync("finish-roundtrip");
			var created = await fixture.Service.CreateDraftAsync(
				fixture.Plan,
				fixture.Plan.PlanId,
				TestContext.Current.CancellationToken);
			var publication = await fixture.Service.PlanPublicationAsync(
				fixture.Plan,
				fixture.Plan.PlanId,
				TestContext.Current.CancellationToken);
			var published = await fixture.Service.PublishAsync(
				fixture.Plan,
				fixture.Plan.PlanId,
				publication,
				publication.PublicationPlanId,
				TestContext.Current.CancellationToken);

			var createdJson = JsonSerializer.Serialize(
				created,
				ReleaseJsonContext.Strict.FinishCreateDraftResult);
			var publicationJson = JsonSerializer.Serialize(
				publication,
				ReleaseJsonContext.Strict.FinishPublicationPlan);
			var publishedJson = JsonSerializer.Serialize(
				published,
				ReleaseJsonContext.Strict.FinishPublishResult);

			Assert.NotNull(JsonSerializer.Deserialize(
				createdJson,
				ReleaseJsonContext.Strict.FinishCreateDraftResult));
			Assert.NotNull(JsonSerializer.Deserialize(
				publicationJson,
				ReleaseJsonContext.Strict.FinishPublicationPlan));
			Assert.NotNull(JsonSerializer.Deserialize(
				publishedJson,
				ReleaseJsonContext.Strict.FinishPublishResult));
			Assert.Contains("\"bodyHashAlgorithm\": \"SHA256\"", publicationJson);
			Assert.Contains("\"nextAction\": \"publish\"", publicationJson);
			Assert.Throws<JsonException>(() => JsonSerializer.Deserialize(
				publicationJson.Insert(publicationJson.IndexOf('{') + 1, "\"unknown\":true,"),
				ReleaseJsonContext.Strict.FinishPublicationPlan));
			Assert.Throws<JsonException>(() => JsonSerializer.Deserialize(
				publicationJson.Replace(
					"\"bodyHashAlgorithm\": \"SHA256\"",
					"\"bodyHashAlgorithm\": 0",
					StringComparison.Ordinal),
				ReleaseJsonContext.Strict.FinishPublicationPlan));
		}

		[Fact]
		public async Task Publication_validator_rejects_cross_field_and_hash_inconsistencies()
		{
			using var fixture = await FinishTestFixture.CreateAsync("finish-contract-invalid");
			await fixture.EnsureTagAsync();
			fixture.SetRelease(ManagedReleaseMarkers.BuildInitialBody("notes"));
			var publication = await fixture.Service.PlanPublicationAsync(
				fixture.Plan,
				fixture.Plan.PlanId,
				TestContext.Current.CancellationToken);

			Assert.Throws<ValidationException>(() =>
				FinishPublicationPlanValidator.Validate(publication with
				{
					PublicationPlanId = publication.PlanId,
				}));
			Assert.Throws<ValidationException>(() =>
				FinishPublicationPlanValidator.Validate(publication with
				{
					BodyHash = publication.BodyHash.ToUpperInvariant(),
				}));
			Assert.Throws<ValidationException>(() =>
				FinishPublicationPlanValidator.Validate(publication with
				{
					ReadyToPublish = false,
				}));
			Assert.Throws<ValidationException>(() =>
				FinishPublicationPlanValidator.Validate(publication with
				{
					MarkerState = ManagedMarkerState.None,
				}));
		}
	}
}
