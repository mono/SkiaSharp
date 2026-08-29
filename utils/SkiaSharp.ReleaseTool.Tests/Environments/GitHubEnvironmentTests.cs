using System.Net;
using System.Text;
using System.Text.Json;
using SkiaSharp.ReleaseTool;
using SkiaSharp.ReleaseTool.Cli;
using SkiaSharp.ReleaseTool.Environments;
using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Finishing;
using SkiaSharp.ReleaseTool.NuGet;
using SkiaSharp.ReleaseTool.Planning;
using Xunit;

namespace SkiaSharp.ReleaseTool.Tests.Environments
{
	public sealed class GitHubEnvironmentTests
	{
		[Fact]
		public void Safe_environment_passes()
		{
			var report = GitHubEnvironmentPolicy.Check(SafeSnapshot(), "release-tag", "main");

			Assert.True(report.Ok);
			Assert.True(report.Exists);
			Assert.Empty(report.Reasons);
			Assert.Equal(["main"], report.AllowedBranches);
		}

		[Fact]
		public void Missing_environment_is_an_explicit_unsafe_report()
		{
			var report = GitHubEnvironmentPolicy.Check(null, "release-tag", "main");

			Assert.False(report.Ok);
			Assert.False(report.Exists);
			Assert.Contains("does not exist", Assert.Single(report.Reasons));
		}

		[Theory]
		[InlineData(0, true, "no reviewers")]
		[InlineData(1, false, "prevent_self_review")]
		public void Reviewer_policy_failures_are_reported(
			int reviewerCount,
			bool preventSelfReview,
			string expected)
		{
			var snapshot = SafeSnapshot() with
			{
				RequiredReviewers = new(reviewerCount, preventSelfReview),
			};

			var report = GitHubEnvironmentPolicy.Check(snapshot, "release-tag", "main");

			Assert.False(report.Ok);
			Assert.Contains(report.Reasons, reason => reason.Contains(expected, StringComparison.Ordinal));
		}

		[Fact]
		public void Branch_policy_must_be_exactly_default_branch_without_tags()
		{
			var snapshot = SafeSnapshot() with
			{
				BranchPolicies =
				[
					new("main", "branch"),
					new("release/*", "branch"),
					new("v*", "tag"),
				],
			};

			var report = GitHubEnvironmentPolicy.Check(snapshot, "release-tag", "main");

			Assert.False(report.Ok);
			Assert.Contains(report.Reasons, reason => reason.Contains("tag deployment", StringComparison.Ordinal));
			Assert.Contains(report.Reasons, reason => reason.Contains("allowed deployment branches", StringComparison.Ordinal));
		}

		[Fact]
		public async Task Http_client_accepts_unknown_GitHub_fields_and_parses_typed_shape()
		{
			var handler = new QueueHandler(
				Json("""
					{
					  "id": 1,
					  "name": "release-tag",
					  "future_field": {"anything": true},
					  "protection_rules": [{
					    "type": "required_reviewers",
					    "prevent_self_review": true,
					    "reviewers": [{"type": "User", "reviewer": {"login": "octocat"}}],
					    "unknown": 42
					  }],
					  "deployment_branch_policy": {
					    "protected_branches": false,
					    "custom_branch_policies": true,
					    "future": "ignored"
					  }
					}
					"""),
				Json("""
					{"total_count":1,"branch_policies":[
					  {"name":"main","type":"branch","unknown":true}
					],"future":[]}
					"""));
			using var http = new HttpClient(handler) { BaseAddress = new Uri("https://api.github.test/") };

			var snapshot = await new HttpGitHubEnvironmentClient(http, "token")
				.GetEnvironmentAsync("release-tag", TestContext.Current.CancellationToken);

			Assert.NotNull(snapshot);
			Assert.Equal(1, snapshot.RequiredReviewers!.ReviewerCount);
			Assert.True(snapshot.RequiredReviewers.PreventSelfReview);
			Assert.Equal(new EnvironmentBranchPolicy("main", "branch"), Assert.Single(snapshot.BranchPolicies));
			Assert.All(handler.Requests, request => Assert.Equal(HttpMethod.Get, request.Method));
		}

		[Fact]
		public async Task Http_client_maps_only_environment_404_to_missing_and_types_403()
		{
			using var missingHttp = new HttpClient(new QueueHandler(new HttpResponseMessage(HttpStatusCode.NotFound)))
			{
				BaseAddress = new Uri("https://api.github.test/"),
			};
			Assert.Null(await new HttpGitHubEnvironmentClient(missingHttp, "token")
				.GetEnvironmentAsync("missing", TestContext.Current.CancellationToken));

			using var deniedHttp = new HttpClient(new QueueHandler(new HttpResponseMessage(HttpStatusCode.Forbidden)))
			{
				BaseAddress = new Uri("https://api.github.test/"),
			};
			await Assert.ThrowsAsync<GitHubException>(() =>
				new HttpGitHubEnvironmentClient(deniedHttp, "token")
					.GetEnvironmentAsync("denied", TestContext.Current.CancellationToken));
		}

		[Fact]
		public async Task Http_client_types_transport_and_JSON_failures()
		{
			using var transportHttp = new HttpClient(
				new ThrowingHandler(new HttpRequestException("offline")))
			{
				BaseAddress = new Uri("https://api.github.test/"),
			};
			var transport = await Assert.ThrowsAsync<GitHubException>(() =>
				new HttpGitHubEnvironmentClient(transportHttp, "token")
					.GetEnvironmentAsync("release-tag", TestContext.Current.CancellationToken));
			Assert.IsType<HttpRequestException>(transport.InnerException);

			using var malformedHttp = new HttpClient(new QueueHandler(Json("{")))
			{
				BaseAddress = new Uri("https://api.github.test/"),
			};
			var malformed = await Assert.ThrowsAsync<GitHubException>(() =>
				new HttpGitHubEnvironmentClient(malformedHttp, "token")
					.GetEnvironmentAsync("release-tag", TestContext.Current.CancellationToken));
			Assert.IsType<JsonException>(malformed.InnerException);
		}

		[Fact]
		public async Task Http_client_preserves_caller_cancellation()
		{
			using var http = new HttpClient(new ThrowingHandler(new OperationCanceledException()))
			{
				BaseAddress = new Uri("https://api.github.test/"),
			};
			using var canceled = new CancellationTokenSource();
			canceled.Cancel();

			await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
				new HttpGitHubEnvironmentClient(http, "token")
					.GetEnvironmentAsync("release-tag", canceled.Token));
		}

		[Fact]
		public void Report_JSON_is_strict_and_source_generated()
		{
			var report = GitHubEnvironmentPolicy.Check(SafeSnapshot(), "release-tag", "main");
			var json = JsonSerializer.Serialize(report, EnvironmentJsonContext.Strict.EnvironmentCheckReport);
			var withUnknown = json.Replace("{", "{\"unknown\":true,", StringComparison.Ordinal);

			Assert.Throws<JsonException>(() => JsonSerializer.Deserialize(
				withUnknown,
				EnvironmentJsonContext.Strict.EnvironmentCheckReport));
		}

		[Fact]
		public async Task Cli_writes_unsafe_report_and_returns_nonzero()
		{
			using var directory = new TestDirectory("environment-cli");
			var environment = new CommandEnvironment(null);

			var exit = await Program.InvokeAsync(
				[
					"check-environment",
					"--name", "release-tag",
					"--default-branch", "main",
					"--output", Path.Combine(directory.Path, "report.json"),
				],
				environment);

			Assert.Equal(ExitCodes.GenericError, exit);
			Assert.True(File.Exists(Path.Combine(directory.Path, "report.json")));
			Assert.Contains("\"exists\": false", environment.Output.ToString());
		}

		private static GitHubEnvironmentSnapshot SafeSnapshot() =>
			new(
				"release-tag",
				["required_reviewers", "branch_policy"],
				new(1, true),
				ProtectedBranches: false,
				CustomBranchPolicies: true,
				[new("main", "branch")]);

		private static HttpResponseMessage Json(string value) =>
			new(HttpStatusCode.OK)
			{
				Content = new StringContent(value, Encoding.UTF8, "application/json"),
			};

		private sealed class QueueHandler(params HttpResponseMessage[] responses) : HttpMessageHandler
		{
			private readonly Queue<HttpResponseMessage> responses = new(responses);
			public List<HttpRequestMessage> Requests { get; } = [];

			protected override Task<HttpResponseMessage> SendAsync(
				HttpRequestMessage request,
				CancellationToken cancellationToken)
			{
				Requests.Add(request);
				return Task.FromResult(responses.Dequeue());
			}

		}

		private sealed class ThrowingHandler(Exception exception) : HttpMessageHandler
		{
			protected override Task<HttpResponseMessage> SendAsync(
				HttpRequestMessage request,
				CancellationToken cancellationToken) =>
				Task.FromException<HttpResponseMessage>(
					cancellationToken.IsCancellationRequested
						? new OperationCanceledException(cancellationToken)
						: exception);
		}

		private sealed class CommandEnvironment(GitHubEnvironmentSnapshot? snapshot) : IReleaseCommandEnvironment
		{
			public StringWriter Output { get; } = new();
			public StringWriter Error { get; } = new();
			public TextWriter StandardOutput => Output;
			public TextWriter StandardError => Error;
			public TimeProvider TimeProvider => TimeProvider.System;
			public Func<Guid> NewPlanId => Guid.NewGuid;
			public Task<IReleaseRepository> OpenRepositoryAsync(string? path, CancellationToken cancellationToken) =>
				throw new NotSupportedException();
			public IPrepareGitHubClient CreateGitHubClient() => throw new NotSupportedException();
			public IGitHubEnvironmentClient CreateEnvironmentGitHubClient() =>
				new FakeEnvironmentClient(snapshot);
		}

		private sealed class FakeEnvironmentClient(GitHubEnvironmentSnapshot? snapshot) : IGitHubEnvironmentClient
		{
			public Task<GitHubEnvironmentSnapshot?> GetEnvironmentAsync(
				string name,
				CancellationToken cancellationToken = default) =>
				Task.FromResult(snapshot);
		}
	}
}
