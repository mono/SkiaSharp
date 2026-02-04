using Octokit;
using Spectre.Console;

namespace SkiaSharp.Collector.Services;

/// <summary>
/// Shared GitHub API client with rate limiting and error handling.
/// </summary>
public sealed class GitHubService : IDisposable
{
    private readonly GitHubClient _client;
    private readonly string _owner;
    private readonly string _repo;
    private readonly bool _verbose;

    public string Owner => _owner;
    public string Repo => _repo;

    public GitHubService(string owner, string repo, bool verbose = false)
    {
        _owner = owner;
        _repo = repo;
        _verbose = verbose;

        _client = new GitHubClient(new ProductHeaderValue("SkiaSharp-Collector"));

        var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        if (!string.IsNullOrEmpty(token))
        {
            _client.Credentials = new Credentials(token);
            if (_verbose)
                AnsiConsole.MarkupLine("[dim]Using GITHUB_TOKEN for authentication[/]");
        }
        else if (_verbose)
        {
            AnsiConsole.MarkupLine("[yellow]Warning: GITHUB_TOKEN not set, rate limits will be lower[/]");
        }
    }

    public async Task<Repository> GetRepositoryAsync()
    {
        return await _client.Repository.Get(_owner, _repo);
    }

    public async Task<SearchIssuesResult> SearchIssuesAsync(string query)
    {
        var request = new SearchIssuesRequest(query);
        return await _client.Search.SearchIssues(request);
    }

    public async Task<IReadOnlyList<GitHubCommit>> GetCommitsAsync(int count = 10)
    {
        var request = new CommitRequest { };
        return await _client.Repository.Commit.GetAll(_owner, _repo, new ApiOptions { PageSize = count, PageCount = 1 });
    }

    public async Task<IReadOnlyList<GitHubCommit>> GetCommitsSinceAsync(DateTimeOffset since, int maxCount = 100)
    {
        var request = new CommitRequest { Since = since };
        return await _client.Repository.Commit.GetAll(_owner, _repo, request, new ApiOptions { PageSize = maxCount });
    }

    public async Task<IReadOnlyList<Issue>> GetOpenIssuesAsync(int page, int pageSize = 100)
    {
        var request = new RepositoryIssueRequest
        {
            State = ItemStateFilter.Open,
            Filter = IssueFilter.All
        };
        return await _client.Issue.GetAllForRepository(_owner, _repo, request, 
            new ApiOptions { PageSize = pageSize, PageCount = 1, StartPage = page });
    }

    public async Task<IReadOnlyList<PullRequest>> GetOpenPullRequestsAsync(int pageSize = 100)
    {
        var request = new PullRequestRequest { State = ItemStateFilter.Open };
        return await _client.PullRequest.GetAllForRepository(_owner, _repo, request, 
            new ApiOptions { PageSize = pageSize });
    }

    public async Task<PullRequest> GetPullRequestAsync(int number)
    {
        return await _client.PullRequest.Get(_owner, _repo, number);
    }

    public async Task<IReadOnlyList<PullRequestReview>> GetPullRequestReviewsAsync(int number)
    {
        return await _client.PullRequest.Review.GetAll(_owner, _repo, number);
    }

    public async Task<IReadOnlyList<RepositoryContributor>> GetContributorsAsync(int count = 100)
    {
        return await _client.Repository.GetAllContributors(_owner, _repo, 
            new ApiOptions { PageSize = count });
    }

    public async Task<User> GetUserAsync(string login)
    {
        return await _client.User.Get(login);
    }

    public async Task<IReadOnlyList<Organization>> GetUserOrganizationsAsync(string login)
    {
        return await _client.Organization.GetAllForUser(login);
    }

    public async Task<IReadOnlyList<Label>> GetLabelsAsync(int count = 100)
    {
        return await _client.Issue.Labels.GetAllForRepository(_owner, _repo,
            new ApiOptions { PageSize = count });
    }

    /// <summary>
    /// Check if a user is affiliated with Microsoft (via company or org membership).
    /// </summary>
    public async Task<bool> IsMicrosoftMemberAsync(string login)
    {
        try
        {
            var user = await GetUserAsync(login);
            if (!string.IsNullOrEmpty(user.Company))
            {
                var company = user.Company.ToLowerInvariant();
                if (company.Contains("microsoft") || company.Contains("@microsoft") || company.Contains("msft"))
                    return true;
            }

            var orgs = await GetUserOrganizationsAsync(login);
            var msOrgs = new[] { "microsoft", "dotnet", "xamarin", "mono", "azure" };
            return orgs.Any(o => msOrgs.Contains(o.Login.ToLowerInvariant()));
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Wait briefly to avoid hitting rate limits.
    /// </summary>
    public static async Task RateLimitDelayAsync(int milliseconds = 100)
    {
        await Task.Delay(milliseconds);
    }

    public void Dispose()
    {
        // GitHubClient doesn't implement IDisposable, but we might add HttpClient later
    }
}
