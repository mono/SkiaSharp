using System.Text.Json;
using SkiaSharp.ReleaseTool.Contracts;
using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Finishing;
using SkiaSharp.ReleaseTool.Json;
using SkiaSharp.ReleaseTool.Model;
using SkiaSharp.ReleaseTool.NuGet;
using SkiaSharp.ReleaseTool.Planning;
using SkiaSharp.ReleaseTool.Tests.Planning;
using SkiaSharp.ReleaseTool.Tests.Finishing;
using Xunit;

namespace SkiaSharp.ReleaseTool.Tests.Cli
{
	public sealed class FinishProgramTests
	{
		[Fact]
		public async Task Pending_receipt_returns_two_and_writes_default_output()
		{
			using var root = new TestDirectory("finish-cli-pending");
			WritePolicies(root.Path);
			var repository = new FakePrepareRepository(root.Path);
			repository.AddRef(
				"refs/remotes/origin/main",
				new string('a', 40),
				new TestVersionState("4.152.0", "14.2.1.200", "stable"));
			var environment = new FinishEnvironment(repository);

			var exitCode = await Program.InvokeAsync(
				["finish", "plan", "--version", "4.152.0"],
				environment);

			Assert.Equal(ExitCodes.Pending, exitCode);
			Assert.True(repository.FetchCalled);
			var path = Path.Combine(root.Path, "finish-plan.json");
			Assert.True(File.Exists(path));
			var report = JsonSerializer.Deserialize(
				File.ReadAllText(path),
				ReleaseJsonContext.Strict.FinishPendingReport);
			Assert.NotNull(report);
			Assert.Equal(PendingNextAction.Pending, report.NextAction);
			Assert.Equal("SkiaSharp", Assert.Single(report.MissingPackages).Id);
			Assert.Empty(environment.Error.ToString());
		}

		[Fact]
		public async Task Finish_write_commands_require_explicit_correlations_and_publication()
		{
			using var root = new TestDirectory("finish-cli-required");
			var environment = new FinishEnvironment(new FakePrepareRepository(root.Path));

			Assert.NotEqual(
				ExitCodes.Success,
				await Program.InvokeAsync(
					["finish", "create-draft", "--plan", "finish.json"],
					environment));
			Assert.NotEqual(
				ExitCodes.Success,
				await Program.InvokeAsync(
					["finish", "plan-publication", "--plan", "finish.json"],
					environment));
			Assert.NotEqual(
				ExitCodes.Success,
				await Program.InvokeAsync(
					[
						"finish",
						"publish",
						"--plan",
						"finish.json",
						"--expected-plan-id",
						Guid.NewGuid().ToString(),
					],
					environment));
		}

		[Fact]
		public async Task Finish_write_commands_persist_typed_default_outputs_end_to_end()
		{
			using var fixture = await FinishTestFixture.CreateAsync("finish-cli-write");
			var planPath = Path.Combine(fixture.Repository.Root, "approved-finish.json");
			PlanStore.Write(planPath, fixture.Plan);
			var environment = new FinishWriteEnvironment(
				fixture.Repository,
				fixture.GitHub);

			var createExit = await Program.InvokeAsync(
				[
					"finish",
					"create-draft",
					"--repo",
					fixture.Repository.Root,
					"--plan",
					"approved-finish.json",
					"--expected-plan-id",
					fixture.Plan.PlanId.ToString(),
				],
				environment);

			Assert.Equal(ExitCodes.Success, createExit);
			var createPath = Path.Combine(
				fixture.Repository.Root,
				"finish-create-draft-result.json");
			var createResult = JsonSerializer.Deserialize(
				File.ReadAllText(createPath),
				ReleaseJsonContext.Strict.FinishCreateDraftResult);
			Assert.NotNull(createResult);
			Assert.Equal(FinishNextAction.PlanPublication, createResult.NextAction);

			var publicationExit = await Program.InvokeAsync(
				[
					"finish",
					"plan-publication",
					"--repo",
					fixture.Repository.Root,
					"--plan",
					"approved-finish.json",
					"--expected-plan-id",
					fixture.Plan.PlanId.ToString(),
				],
				environment);

			Assert.Equal(ExitCodes.Success, publicationExit);
			var publicationPath = Path.Combine(
				fixture.Repository.Root,
				"finish-publication-plan.json");
			var publication = PlanStore.ReadPublication(
				publicationPath,
				fixture.Plan.PlanId,
				FinishTestFixture.PublicationPlanId);
			Assert.Equal(FinishNextAction.Publish, publication.NextAction);

			var publishExit = await Program.InvokeAsync(
				[
					"finish",
					"publish",
					"--repo",
					fixture.Repository.Root,
					"--plan",
					"approved-finish.json",
					"--expected-plan-id",
					fixture.Plan.PlanId.ToString(),
					"--publication",
					"finish-publication-plan.json",
					"--expected-publication-plan-id",
					publication.PublicationPlanId.ToString(),
				],
				environment);

			Assert.Equal(ExitCodes.Success, publishExit);
			var publishPath = Path.Combine(
				fixture.Repository.Root,
				"finish-publish-result.json");
			var publishResult = JsonSerializer.Deserialize(
				File.ReadAllText(publishPath),
				ReleaseJsonContext.Strict.FinishPublishResult);
			Assert.NotNull(publishResult);
			Assert.Equal(FinishNextAction.Closeout, publishResult.NextAction);
			Assert.Equal(publication.PublicationPlanId, publishResult.PublicationPlanId);
			Assert.True(File.Exists(createPath));
		}

		[Fact]
		public async Task CLI_PlanId_mismatch_is_rejected_before_write_client_creation()
		{
			using var fixture = await FinishTestFixture.CreateAsync("finish-cli-correlation");
			PlanStore.Write(
				Path.Combine(fixture.Repository.Root, "approved-finish.json"),
				fixture.Plan);
			var environment = new FinishWriteEnvironment(
				fixture.Repository,
				fixture.GitHub);

			var exit = await Program.InvokeAsync(
				[
					"finish",
					"create-draft",
					"--repo",
					fixture.Repository.Root,
					"--plan",
					"approved-finish.json",
					"--expected-plan-id",
					Guid.NewGuid().ToString(),
				],
				environment);

			Assert.Equal(ExitCodes.GenericError, exit);
			Assert.Equal(0, environment.WriteClientCreations);
			Assert.Equal(0, fixture.GitHub.GetCount);
		}

		private static void WritePolicies(string root)
		{
			var directory = Path.Combine(root, "scripts", "infra", "release");
			Directory.CreateDirectory(directory);
			File.WriteAllText(
				Path.Combine(directory, "public-packages.json"),
				"""{"$schemaComment":null,"anchorPackages":["SkiaSharp","SkiaSharp.HarfBuzz","HarfBuzzSharp"]}""");
			File.WriteAllText(
				Path.Combine(directory, "trusted-signing-certificates.json"),
				"""
				{"$schemaComment":null,"hashAlgorithm":"SHA256","certificates":[
				  {"fingerprint":"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA","role":"author","subject":"a","description":"a","source":null,"validFrom":null,"validUntil":"2030-01-01"},
				  {"fingerprint":"BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB","role":"repository","subject":"r","description":"r","source":null,"validFrom":null,"validUntil":"2030-01-01"}
				]}
				""");
		}

		private sealed class FinishEnvironment(IReleaseRepository repository) : IReleaseCommandEnvironment
		{
			public StringWriter Output { get; } = new();
			public StringWriter Error { get; } = new();
			public TextWriter StandardOutput => Output;
			public TextWriter StandardError => Error;
			public TimeProvider TimeProvider { get; } = new FixedTimeProvider();
			public Func<Guid> NewPlanId => () =>
				Guid.Parse("5e6addd4-3548-45a7-b8ca-43b56725eca1");

			public Task<IReleaseRepository> OpenRepositoryAsync(
				string? path,
				CancellationToken cancellationToken) =>
				Task.FromResult(repository);

			public IPrepareGitHubClient CreateGitHubClient() =>
				throw new NotSupportedException();

			public IFinishGitHubClient CreateFinishGitHubClient() =>
				new EmptyFinishGitHubClient();

			public IPublicReceiptVerifier CreatePublicReceiptVerifier() =>
				new PendingReceiptVerifier();
		}

		private sealed class EmptyFinishGitHubClient : IFinishGitHubClient
		{
			public Task<FinishGitHubRelease?> GetReleaseAsync(
				string tag,
				CancellationToken cancellationToken = default) =>
				Task.FromResult<FinishGitHubRelease?>(null);
		}

		private sealed class PendingReceiptVerifier : IPublicReceiptVerifier
		{
			public Task<PublicReleaseReceipt> VerifyAsync(
				IFinishRepository repository,
				PublicReleaseVersion requestedVersion,
				ReleasePolicies policies,
				CancellationToken cancellationToken) =>
				Task.FromException<PublicReleaseReceipt>(
					new PackagesPendingException(
						"SkiaSharp is still indexing",
						[new PendingPackage("SkiaSharp", requestedVersion.Text)],
						TimeSpan.FromSeconds(60),
						TimeSpan.FromSeconds(60)));
		}

		private sealed class FinishWriteEnvironment(
			IReleaseRepository repository,
			IFinishGitHubWriteClient github) : IReleaseCommandEnvironment
		{
			public StringWriter Output { get; } = new();
			public StringWriter Error { get; } = new();
			public int WriteClientCreations { get; private set; }
			public TextWriter StandardOutput => Output;
			public TextWriter StandardError => Error;
			public TimeProvider TimeProvider { get; } =
				new FixedTimeProvider();
			public Func<Guid> NewPlanId => () =>
				FinishTestFixture.PublicationPlanId;

			public Task<IReleaseRepository> OpenRepositoryAsync(
				string? path,
				CancellationToken cancellationToken) =>
				Task.FromResult(repository);

			public IPrepareGitHubClient CreateGitHubClient() =>
				throw new NotSupportedException();

			public IFinishGitHubClient CreateFinishGitHubClient() => github;

			public IFinishGitHubWriteClient CreateFinishGitHubWriteClient()
			{
				WriteClientCreations++;
				return github;
			}

			public IPublicReceiptVerifier CreatePublicReceiptVerifier() =>
				throw new NotSupportedException();
		}

		private sealed class FixedTimeProvider : TimeProvider
		{
			public override DateTimeOffset GetUtcNow() =>
				new(2026, 8, 28, 12, 0, 0, TimeSpan.Zero);
		}
	}
}
