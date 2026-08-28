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
		private const string SkiaRepository = "mono/skia";
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
			if (repository != SkiaRepository)
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

		internal static string? ResolveToken(Func<string, string?> getVariable)
		{
			var ghToken = getVariable("GH_TOKEN");
			return string.IsNullOrEmpty(ghToken)
				? getVariable("GITHUB_TOKEN")
				: ghToken;
		}
	}
}
