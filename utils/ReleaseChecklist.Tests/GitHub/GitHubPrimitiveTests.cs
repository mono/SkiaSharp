using ReleaseChecklist.Core;
using ReleaseChecklist.GitHub;

namespace ReleaseChecklist.Tests.GitHub;

public class GitHubPrimitiveTests
{
	private static readonly GitHubRepositoryIdentity Repository = new("mono", "skia");

	[Fact]
	public async Task BranchMissingMatchingAndConflictAreDistinct()
	{
		var client = new FakeGitHubClient();
		var options = new GitHubBranchOptions
		{
			Id = "branch",
			Title = "Branch",
			Client = client,
			Repository = Repository,
			Branch = "release/test",
			ExpectedSha = "abc",
		};
		var check = new GitHubBranchCheck(options);
		var definition = new ChecklistBuilder().Sequence("root", "Root", root =>
			root.Step(new StepOptions("branch", "Branch") { Check = check }));
		Assert.Equal(
			ChecklistStatus.NotDone,
			(await ChecklistRunner.RunAsync(definition)).Root.Children[0].Status);

		client.Branches["release/test"] = "abc";
		Assert.Equal(
			ChecklistStatus.Done,
			(await ChecklistRunner.RunAsync(definition)).Root.Children[0].Status);

		client.Branches["release/test"] = "different";
		Assert.Equal(
			ChecklistStatus.Blocked,
			(await ChecklistRunner.RunAsync(definition)).Root.Children[0].Status);
	}

	[Fact]
	public async Task BranchCreateRecoversExactRace()
	{
		var client = new FakeGitHubClient { RaceBranchCreate = true };
		var definition = new ChecklistBuilder().Sequence("root", "Root", root =>
			root.GitHubBranch(new GitHubBranchOptions
			{
				Id = "branch",
				Title = "Branch",
				Client = client,
				Repository = Repository,
				Branch = "release/test",
				ExpectedSha = "abc",
			}));

		var report = await ChecklistRunner.RunAsync(
			definition,
			new ChecklistRunOptions { Mode = ChecklistRunMode.Apply });

		Assert.True(report.Successful);
		Assert.Equal("abc", client.Branches["release/test"]);
	}

	[Fact]
	public async Task PullRequestCreateAndAmbiguity()
	{
		var client = new FakeGitHubClient();
		var options = new GitHubPullRequestOptions
		{
			Id = "pr",
			Title = "PR",
			Client = client,
			Repository = Repository,
			Head = "bump",
			Base = "main",
			PullRequestTitle = "Bump",
			Body = "Body",
		};
		var check = new GitHubPullRequestCheck(options);
		var definition = new ChecklistBuilder().Sequence("root", "Root", root =>
			root.Step(new StepOptions("pr", "PR") { Check = check }));
		Assert.Equal(
			ChecklistStatus.NotDone,
			(await ChecklistRunner.RunAsync(definition)).Root.Children[0].Status);
		client.PullRequests.Add(new(1, "bump", "main", "https://example/1"));
		Assert.Equal(
			ChecklistStatus.Done,
			(await ChecklistRunner.RunAsync(definition)).Root.Children[0].Status);
		client.PullRequests.Add(new(2, "bump", "main", "https://example/2"));
		Assert.Equal(
			ChecklistStatus.Blocked,
			(await ChecklistRunner.RunAsync(definition)).Root.Children[0].Status);
	}

	[Fact]
	public async Task MergedPullRequestSatisfiesDesiredState()
	{
		var client = new FakeGitHubClient();
		client.PullRequests.Add(new(
			1,
			"bump",
			"main",
			"https://example/1",
			Merged: true,
			Open: false));
		var definition = new ChecklistBuilder().Sequence("root", "Root", root =>
			root.Step(new StepOptions(
				"pr",
				"PR")
			{
				Check = new GitHubPullRequestCheck(new GitHubPullRequestOptions
				{
					Id = "pr",
					Title = "PR",
					Client = client,
					Repository = Repository,
					Head = "bump",
					Base = "main",
					PullRequestTitle = "Bump",
					Body = "Body",
				}),
			}));

		var report = await ChecklistRunner.RunAsync(definition);

		Assert.Equal(ChecklistStatus.Done, report.Root.Children[0].Status);
	}

	[Fact]
	public async Task BranchLookupUsesExactSingularRefEndpoint()
	{
		var handler = new StubHandler(
			"""
			{
			  "ref": "refs/heads/release/4.152.0",
			  "object": {
			    "type": "commit",
			    "sha": "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"
			  }
			}
			""");
		var httpClient = new HttpClient(handler)
		{
			BaseAddress = new Uri("https://api.github.test/"),
		};
		var client = new OctokitGitHubRepositoryClient(
			"release-checklist-tests",
			httpClient: httpClient);

		var sha = await client.GetBranchShaAsync(
			Repository,
			"release/4.152.0",
			CancellationToken.None);

		Assert.Equal("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", sha);
		Assert.Equal(
			"/repos/mono/skia/git/ref/heads/release/4.152.0",
			handler.RequestUri!.AbsolutePath);
	}
}

internal sealed class StubHandler(string responseBody) : HttpMessageHandler
{
	public Uri? RequestUri { get; private set; }

	protected override Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request,
		CancellationToken cancellationToken)
	{
		RequestUri = request.RequestUri;
		return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
		{
			Content = new StringContent(responseBody),
		});
	}
}

internal sealed class FakeGitHubClient : IGitHubRepositoryClient
{
	public Dictionary<string, string> Branches { get; } = new(StringComparer.Ordinal);
	public List<GitHubPullRequestState> PullRequests { get; } = [];
	public bool RaceBranchCreate { get; init; }

	public Task<string?> GetBranchShaAsync(
		GitHubRepositoryIdentity repository,
		string branch,
		CancellationToken cancellationToken) =>
		Task.FromResult(Branches.GetValueOrDefault(branch));

	public Task CreateBranchAsync(
		GitHubRepositoryIdentity repository,
		string branch,
		string sha,
		CancellationToken cancellationToken)
	{
		Branches[branch] = sha;
		if (RaceBranchCreate)
			throw new InvalidOperationException("Reference already exists.");
		return Task.CompletedTask;
	}

	public Task<IReadOnlyList<GitHubPullRequestState>> FindPullRequestsAsync(
		GitHubRepositoryIdentity repository,
		string head,
		string @base,
		CancellationToken cancellationToken) =>
		Task.FromResult<IReadOnlyList<GitHubPullRequestState>>(
			PullRequests.Where(pr => pr.Head == head && pr.Base == @base).ToArray());

	public Task<GitHubPullRequestState> CreatePullRequestAsync(
		GitHubRepositoryIdentity repository,
		string head,
		string @base,
		string title,
		string body,
		CancellationToken cancellationToken)
	{
		var state = new GitHubPullRequestState(
			PullRequests.Count + 1,
			head,
			@base,
			$"https://example/{PullRequests.Count + 1}");
		PullRequests.Add(state);
		return Task.FromResult(state);
	}
}
