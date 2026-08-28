using System.Net;
using System.Text;
using Octokit;
using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Planning;
using Xunit;

namespace SkiaSharp.ReleaseTool.Tests.Planning
{
	public sealed class OctokitPrepareGitHubClientTests
	{
		[Fact]
		public async Task Raw_singular_ref_lookup_returns_exact_commit_SHA()
		{
			var sha = new string('a', 40);
			var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent(
					"{\"ref\":\"refs/heads/release/3.119.0\",\"node_id\":\"ignored\"," +
					$"\"object\":{{\"type\":\"commit\",\"sha\":\"{sha}\",\"url\":\"ignored\"}}}}",
					Encoding.UTF8,
					"application/json"),
			});
			using var httpClient = new HttpClient(handler)
			{
				BaseAddress = new Uri("https://api.github.test/"),
			};
			var client = new OctokitPrepareGitHubClient(httpClient, "token");

			var result = await client.GetRefShaAsync(
				"mono/skia",
				"refs/heads/release/3.119.0",
				TestContext.Current.CancellationToken);

			Assert.Equal(sha, result);
			Assert.Equal(
				"https://api.github.test/repos/mono/skia/git/ref/heads/release/3.119.0",
				Assert.Single(handler.Requests).RequestUri!.AbsoluteUri);
		}

		[Fact]
		public async Task Raw_ref_lookup_maps_only_404_to_missing()
		{
			using var notFoundHttp = new HttpClient(new StubHandler(
				_ => new HttpResponseMessage(HttpStatusCode.NotFound)))
			{
				BaseAddress = new Uri("https://api.github.test/"),
			};
			using var fatalHttp = new HttpClient(new StubHandler(
				_ => new HttpResponseMessage(HttpStatusCode.Forbidden)))
			{
				BaseAddress = new Uri("https://api.github.test/"),
			};

			Assert.Null(await new OctokitPrepareGitHubClient(notFoundHttp, "token").GetRefShaAsync(
				"mono/skia",
				"refs/heads/missing",
				TestContext.Current.CancellationToken));
			await Assert.ThrowsAnyAsync<ReleaseToolException>(
				() => new OctokitPrepareGitHubClient(fatalHttp, "token").GetRefShaAsync(
					"mono/skia",
					"refs/heads/fatal",
					TestContext.Current.CancellationToken));
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task Raw_ref_body_IO_failures_are_typed(bool httpFailure)
		{
			Exception failure = httpFailure
				? new HttpRequestException("body failed")
				: new IOException("body failed");
			using var httpClient = new HttpClient(new StubHandler(
				_ => new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StreamContent(new ThrowingReadStream(failure)),
				}))
			{
				BaseAddress = new Uri("https://api.github.test/"),
			};
			var client = new OctokitPrepareGitHubClient(httpClient, "token");

			var exception = await Assert.ThrowsAsync<GitHubException>(
				() => client.GetRefShaAsync(
					"mono/skia",
					"refs/heads/release/3.119.0",
					TestContext.Current.CancellationToken));
			Assert.Contains("could not be read", exception.Message);
			Assert.Same(failure, exception.InnerException);
		}

		[Fact]
		public async Task Raw_ref_body_timeout_is_typed_but_caller_cancellation_is_preserved()
		{
			using var timeoutHttp = new HttpClient(new StubHandler(
				_ => new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StreamContent(
						new ThrowingReadStream(new OperationCanceledException("timeout"))),
				}))
			{
				BaseAddress = new Uri("https://api.github.test/"),
			};
			var client = new OctokitPrepareGitHubClient(timeoutHttp, "token");
			var timeout = await Assert.ThrowsAsync<GitHubException>(
				() => client.GetRefShaAsync(
					"mono/skia",
					"refs/heads/release/3.119.0",
					TestContext.Current.CancellationToken));
			Assert.Contains("timed out", timeout.Message);

			using var canceled = new CancellationTokenSource();
			canceled.Cancel();
			await Assert.ThrowsAnyAsync<OperationCanceledException>(
				() => client.GetRefShaAsync(
					"mono/skia",
					"refs/heads/release/3.119.0",
					canceled.Token));
		}

		[Fact]
		public void Token_resolution_prefers_GH_TOKEN()
		{
			var variables = new Dictionary<string, string?>
			{
				["GH_TOKEN"] = "gh",
				["GITHUB_TOKEN"] = "github",
			};

			Assert.Equal(
				"gh",
				OctokitPrepareGitHubClient.ResolveToken(name => variables[name]));
			variables["GH_TOKEN"] = "";
			Assert.Equal(
				"github",
				OctokitPrepareGitHubClient.ResolveToken(name => variables[name]));
		}

		[Fact]
		public async Task Pull_request_search_qualifies_the_head_owner()
		{
			PullRequestRequest? captured = null;
			using var httpClient = new HttpClient(new StubHandler(
				_ => new HttpResponseMessage(HttpStatusCode.NotFound)))
			{
				BaseAddress = new Uri("https://api.github.test/"),
			};
			var client = new OctokitPrepareGitHubClient(
				httpClient,
				"token",
				(request, cancellationToken) =>
				{
					cancellationToken.ThrowIfCancellationRequested();
					captured = request;
					return Task.FromResult<IReadOnlyList<PullRequest>>([]);
				});

			Assert.Null(await client.FindOpenPullRequestAsync(
				"bump-version-3.119.1",
				"release/3.119.x",
				TestContext.Current.CancellationToken));
			Assert.NotNull(captured);
			Assert.Equal("mono:bump-version-3.119.1", captured.Head);
			Assert.Equal("release/3.119.x", captured.Base);
			Assert.Equal(ItemStateFilter.Open, captured.State);
		}

		private sealed class StubHandler(
			Func<HttpRequestMessage, HttpResponseMessage> response) : HttpMessageHandler
		{
			public List<HttpRequestMessage> Requests { get; } = [];

			protected override Task<HttpResponseMessage> SendAsync(
				HttpRequestMessage request,
				CancellationToken cancellationToken)
			{
				Requests.Add(request);
				return Task.FromResult(response(request));
			}
		}

		private sealed class ThrowingReadStream(Exception exception) : Stream
		{
			public override bool CanRead => true;
			public override bool CanSeek => false;
			public override bool CanWrite => false;
			public override long Length => throw new NotSupportedException();
			public override long Position
			{
				get => throw new NotSupportedException();
				set => throw new NotSupportedException();
			}

			public override void Flush() => throw new NotSupportedException();
			public override int Read(byte[] buffer, int offset, int count) => throw exception;
			public override ValueTask<int> ReadAsync(
				Memory<byte> buffer,
				CancellationToken cancellationToken = default) =>
				ValueTask.FromException<int>(
					cancellationToken.IsCancellationRequested
						? new OperationCanceledException(cancellationToken)
						: exception);
			public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
			public override void SetLength(long value) => throw new NotSupportedException();
			public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
		}
	}
}
