using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Octokit;

namespace ReleaseChecklist.GitHub;

/// <summary>Implements checklist GitHub operations with Octokit and exact REST ref lookup.</summary>
public sealed class OctokitGitHubRepositoryClient : IGitHubRepositoryClient
{
	private readonly GitHubClient client;
	private readonly HttpClient httpClient;
	private readonly string? token;

	/// <summary>Initializes a new instance of the <see cref="OctokitGitHubRepositoryClient" /> class.</summary>
	/// <param name="productName">The product name sent in GitHub user-agent headers.</param>
	/// <param name="token">The GitHub token, or <see langword="null" /> for anonymous reads.</param>
	/// <param name="httpClient">The HTTP client used for exact ref lookup, or <see langword="null" /> to create one.</param>
	public OctokitGitHubRepositoryClient(
		string productName,
		string? token = null,
		HttpClient? httpClient = null)
	{
		this.token = token;
		client = new GitHubClient(new Octokit.ProductHeaderValue(productName));
		if (!string.IsNullOrWhiteSpace(token))
			client.Credentials = new Credentials(token);
		this.httpClient = httpClient ?? new HttpClient
		{
			BaseAddress = new Uri("https://api.github.com/"),
			Timeout = TimeSpan.FromSeconds(60),
		};
	}

	/// <inheritdoc />
	public async Task<string?> GetBranchShaAsync(
		GitHubRepositoryIdentity repository,
		string branch,
		CancellationToken cancellationToken)
	{
		var fullRef = $"refs/heads/{branch}";
		var escapedRef = string.Join(
			'/',
			fullRef["refs/".Length..].Split('/').Select(Uri.EscapeDataString));
		using var request = new HttpRequestMessage(
			HttpMethod.Get,
			$"repos/{repository}/git/ref/{escapedRef}");
		request.Headers.UserAgent.ParseAdd("SkiaSharp.ReleaseChecklist");
		request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
		request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
		if (!string.IsNullOrWhiteSpace(token))
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

		using var response = await httpClient.SendAsync(
			request,
			HttpCompletionOption.ResponseHeadersRead,
			cancellationToken).ConfigureAwait(false);
		if (response.StatusCode == HttpStatusCode.NotFound)
			return null;
		if (!response.IsSuccessStatusCode)
		{
			throw new GitHubProtocolException(
				$"GitHub ref lookup failed ({(int)response.StatusCode} {response.ReasonPhrase}) " +
				$"for {repository}:{fullRef}.");
		}

		using var payload = await JsonDocument.ParseAsync(
			await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false),
			cancellationToken: cancellationToken).ConfigureAwait(false);
		var root = payload.RootElement;
		var actualRef = root.GetProperty("ref").GetString();
		var target = root.GetProperty("object");
		var type = target.GetProperty("type").GetString();
		var sha = target.GetProperty("sha").GetString();
		if (actualRef != fullRef ||
			type != "commit" ||
			sha is null ||
			sha.Length != 40 ||
			sha.Any(static c => c is not (>= '0' and <= '9' or >= 'a' and <= 'f')))
		{
			throw new GitHubProtocolException(
				$"GitHub ref lookup returned unexpected data for {repository}:{fullRef}.");
		}
		return sha;
	}

	/// <inheritdoc />
	public async Task CreateBranchAsync(
		GitHubRepositoryIdentity repository,
		string branch,
		string sha,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		_ = await client.Git.Reference.Create(
			repository.Owner,
			repository.Name,
			new NewReference($"refs/heads/{branch}", sha))
			.WaitAsync(cancellationToken).ConfigureAwait(false);
	}

	/// <inheritdoc />
	public async Task<IReadOnlyList<GitHubPullRequestState>> FindPullRequestsAsync(
		GitHubRepositoryIdentity repository,
		string head,
		string @base,
		CancellationToken cancellationToken)
	{
		var request = new PullRequestRequest
		{
			State = ItemStateFilter.All,
			Head = $"{repository.Owner}:{head}",
			Base = @base,
		};
		var pullRequests = await client.PullRequest.GetAllForRepository(
			repository.Owner,
			repository.Name,
			request).WaitAsync(cancellationToken).ConfigureAwait(false);
		return pullRequests
			.Where(pr =>
				pr.Head.Ref == head &&
				pr.Base.Ref == @base &&
				string.Equals(
					pr.Head.Repository?.Owner?.Login,
					repository.Owner,
					StringComparison.OrdinalIgnoreCase))
			.Select(static pr => new GitHubPullRequestState(
				pr.Number,
				pr.Head.Ref,
				pr.Base.Ref,
				pr.HtmlUrl.ToString(),
				pr.MergedAt is not null,
				pr.State == ItemState.Open))
			.ToArray();
	}

	/// <inheritdoc />
	public async Task<GitHubPullRequestState> CreatePullRequestAsync(
		GitHubRepositoryIdentity repository,
		string head,
		string @base,
		string title,
		string body,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		var pr = await client.PullRequest.Create(
			repository.Owner,
			repository.Name,
			new NewPullRequest(title, head, @base) { Body = body })
			.WaitAsync(cancellationToken).ConfigureAwait(false);
		return new GitHubPullRequestState(
			pr.Number,
			pr.Head.Ref,
			pr.Base.Ref,
			pr.HtmlUrl.ToString(),
			pr.MergedAt is not null,
			pr.State == ItemState.Open);
	}
}
