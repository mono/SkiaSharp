using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Octokit;
using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Git;

namespace SkiaSharp.ReleaseTool.Planning
{
	public sealed class OctokitPrepareGitHubClient : IPrepareGitHubClient
	{
		private readonly GitHubClient octokit;
		private readonly HttpClient httpClient;
		private readonly string? token;
		private readonly Func<
			PullRequestRequest,
			CancellationToken,
			Task<IReadOnlyList<PullRequest>>> searchPullRequests;

		public OctokitPrepareGitHubClient(HttpClient? httpClient = null, string? token = null)
		{
			this.token = token ?? ResolveToken(Environment.GetEnvironmentVariable);
			octokit = new GitHubClient(new Octokit.ProductHeaderValue("SkiaSharp.ReleaseTool"));
			if (!string.IsNullOrEmpty(this.token))
				octokit.Credentials = new Credentials(this.token);
			searchPullRequests = SearchPullRequestsAsync;

			this.httpClient = httpClient ?? new HttpClient
			{
				BaseAddress = new Uri("https://api.github.com/"),
				Timeout = TimeSpan.FromSeconds(60),
			};
		}

		internal OctokitPrepareGitHubClient(
			HttpClient httpClient,
			string? token,
			Func<
				PullRequestRequest,
				CancellationToken,
				Task<IReadOnlyList<PullRequest>>> searchPullRequests)
			: this(httpClient, token)
		{
			this.searchPullRequests = searchPullRequests;
		}

		public async Task<string?> GetRefShaAsync(
			string repository,
			string reference,
			CancellationToken cancellationToken = default)
		{
			if (!TryParseRepository(repository, out _, out _))
				throw new ValidationException($"unsupported GitHub ref repository '{repository}'");
			if (!GitReferencePolicy.IsFullyQualified(reference))
				throw new ValidationException("GitHub ref lookup requires a fully-qualified refs/... name");

			var escapedRef = string.Join(
				'/',
				reference["refs/".Length..].Split('/').Select(Uri.EscapeDataString));
			using var request = new HttpRequestMessage(
				HttpMethod.Get,
				$"repos/{repository}/git/ref/{escapedRef}");
			request.Headers.UserAgent.ParseAdd("SkiaSharp.ReleaseTool");
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
			request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
			if (!string.IsNullOrEmpty(token))
				request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

			HttpResponseMessage response;
			try
			{
				response = await httpClient.SendAsync(
					request,
					HttpCompletionOption.ResponseHeadersRead,
					cancellationToken).ConfigureAwait(false);
			}
			catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
			{
				throw new GitHubException($"GitHub ref lookup timed out for {repository}:{reference}", ex);
			}
			catch (HttpRequestException ex)
			{
				throw new GitHubException($"GitHub ref lookup failed for {repository}:{reference}", ex);
			}
			using (response)
			{
				if (response.StatusCode == HttpStatusCode.NotFound)
					return null;
				if (!response.IsSuccessStatusCode)
				{
					throw new GitHubException(
						$"GitHub ref lookup failed ({(int)response.StatusCode} {response.ReasonPhrase}) for {repository}:{reference}");
				}

				GitReferenceResponse payload;
				try
				{
					payload = await JsonSerializer.DeserializeAsync(
						await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false),
						GitHubJsonContext.Default.GitReferenceResponse,
						cancellationToken).ConfigureAwait(false)
						?? throw new GitHubException("GitHub ref lookup returned an empty response");
				}
				catch (JsonException ex)
				{
					throw new GitHubException("GitHub ref lookup returned an invalid response", ex);
				}
				catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
				{
					throw new GitHubException($"GitHub ref response timed out for {repository}:{reference}", ex);
				}
				catch (Exception ex) when (ex is HttpRequestException or IOException)
				{
					throw new GitHubException($"GitHub ref response could not be read for {repository}:{reference}", ex);
				}

				if (payload.Ref != reference || payload.Object.Type != "commit")
					throw new GitHubException($"GitHub ref lookup returned unexpected ref data for {reference}");
				ValidateSha(payload.Object.Sha);
				return payload.Object.Sha;
			}
		}

		public async Task<PullRequestInfo?> FindOpenPullRequestAsync(
			string head,
			string @base,
			CancellationToken cancellationToken = default)
		{
			var request = new PullRequestRequest
			{
				State = ItemStateFilter.Open,
				Head = $"mono:{head}",
				Base = @base,
			};
			IReadOnlyList<PullRequest> pullRequests;
			try
			{
				pullRequests = await searchPullRequests(
					request,
					cancellationToken).ConfigureAwait(false);
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (Exception ex) when (ex is ApiException or HttpRequestException)
			{
				throw new GitHubException("GitHub pull request search failed", ex);
			}

			var matchingPullRequests = pullRequests
				.Where(pullRequest =>
					pullRequest.Head.Ref == head &&
					pullRequest.Base.Ref == @base &&
					string.Equals(
						pullRequest.Head.Repository?.Owner?.Login,
						"mono",
						StringComparison.OrdinalIgnoreCase))
				.ToArray();
			if (matchingPullRequests.Length > 1)
			{
				throw new GitHubException(
					$"GitHub returned multiple open pull requests for mono:{head} -> {@base}");
			}
			var pullRequest = matchingPullRequests.SingleOrDefault();
			return pullRequest is null
				? null
				: new PullRequestInfo(pullRequest.Number, new Uri(pullRequest.HtmlUrl));
		}

		public async Task CreateRefAsync(
			string repository,
			string reference,
			string sha,
			CancellationToken cancellationToken = default)
		{
			if (!TryParseRepository(repository, out var owner, out var name))
				throw new ValidationException($"unsupported GitHub ref repository '{repository}'");
			if (!GitReferencePolicy.IsFullyQualified(reference))
				throw new ValidationException("GitHub ref creation requires a fully-qualified refs/... name");
			ValidateSha(sha);

			try
			{
				_ = await octokit.Git.Reference
					.Create(owner, name, new NewReference(reference, sha))
					.WaitAsync(cancellationToken)
					.ConfigureAwait(false);
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (ApiException ex) when (
				ex.StatusCode is HttpStatusCode.Conflict or HttpStatusCode.UnprocessableEntity)
			{
				var actual = await GetRefShaAsync(
					repository,
					reference,
					cancellationToken).ConfigureAwait(false);
				if (actual == sha)
					return;
				throw new GitHubException(
					$"GitHub ref {repository}:{reference} already exists at '{actual}', expected '{sha}'",
					ex);
			}
			catch (Exception ex) when (ex is ApiException or HttpRequestException)
			{
				throw new GitHubException($"GitHub ref creation failed for {repository}:{reference}", ex);
			}
		}

		public async Task<PullRequestInfo> CreatePullRequestAsync(
			string head,
			string @base,
			string title,
			string body,
			CancellationToken cancellationToken = default)
		{
			try
			{
				var pullRequest = await octokit.PullRequest
					.Create(
						"mono",
						"SkiaSharp",
						new NewPullRequest(title, head, @base) { Body = body })
					.WaitAsync(cancellationToken)
					.ConfigureAwait(false);
				if (pullRequest.Head.Ref != head ||
					pullRequest.Base.Ref != @base ||
					!string.Equals(
						pullRequest.Head.Repository?.Owner?.Login,
						"mono",
						StringComparison.OrdinalIgnoreCase))
				{
					throw new GitHubException("GitHub returned unexpected pull request data after creation");
				}
				return new PullRequestInfo(pullRequest.Number, new Uri(pullRequest.HtmlUrl));
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (ApiException ex) when (
				ex.StatusCode is HttpStatusCode.Conflict or HttpStatusCode.UnprocessableEntity)
			{
				var existing = await FindOpenPullRequestAsync(
					head,
					@base,
					cancellationToken).ConfigureAwait(false);
				if (existing is not null)
					return existing;
				throw new GitHubException(
					$"GitHub rejected pull request creation for mono:{head} -> {@base}",
					ex);
			}
			catch (GitHubException)
			{
				throw;
			}
			catch (Exception ex) when (ex is ApiException or HttpRequestException)
			{
				throw new GitHubException(
					$"GitHub pull request creation failed for mono:{head} -> {@base}",
					ex);
			}
		}

		private async Task<IReadOnlyList<PullRequest>> SearchPullRequestsAsync(
			PullRequestRequest request,
			CancellationToken cancellationToken) =>
			await octokit.PullRequest
				.GetAllForRepository("mono", "SkiaSharp", request)
				.WaitAsync(cancellationToken)
				.ConfigureAwait(false);

		private static void ValidateSha(string sha)
		{
			if (sha.Length != 40 || sha.Any(static character =>
				character is not (>= '0' and <= '9' or >= 'a' and <= 'f')))
			{
				throw new GitHubException($"GitHub ref lookup returned invalid SHA '{sha}'");
			}
		}

		private static bool TryParseRepository(
			string repository,
			out string owner,
			out string name)
		{
			switch (repository)
			{
				case "mono/skia":
					owner = "mono";
					name = "skia";
					return true;
				case "mono/SkiaSharp":
					owner = "mono";
					name = "SkiaSharp";
					return true;
				default:
					owner = "";
					name = "";
					return false;
			}
		}

		internal static string? ResolveToken(Func<string, string?> getVariable)
		{
			var ghToken = getVariable("GH_TOKEN");
			return string.IsNullOrEmpty(ghToken)
				? getVariable("GITHUB_TOKEN")
				: ghToken;
		}
	}
}
