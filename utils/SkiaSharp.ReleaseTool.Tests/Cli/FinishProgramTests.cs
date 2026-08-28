using System.Text.Json;
using SkiaSharp.ReleaseTool.Contracts;
using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Finishing;
using SkiaSharp.ReleaseTool.Json;
using SkiaSharp.ReleaseTool.Model;
using SkiaSharp.ReleaseTool.NuGet;
using SkiaSharp.ReleaseTool.Planning;
using SkiaSharp.ReleaseTool.Tests.Planning;
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

		private sealed class FixedTimeProvider : TimeProvider
		{
			public override DateTimeOffset GetUtcNow() =>
				new(2026, 8, 28, 12, 0, 0, TimeSpan.Zero);
		}
	}
}
