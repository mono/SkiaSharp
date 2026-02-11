using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using SkiaSharp.Triage.Models;

namespace SkiaSharp.Triage.Dashboard.Services;

public class DashboardDataService(HttpClient http)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public async Task<MultiRepoGitHubStats?> GetGitHubStatsAsync() =>
        await http.GetFromJsonAsync<MultiRepoGitHubStats>("data/github-stats.json", JsonOptions);

    public async Task<NuGetStats?> GetNuGetStatsAsync() =>
        await http.GetFromJsonAsync<NuGetStats>("data/nuget-stats.json", JsonOptions);

    public async Task<NuGetChartsData?> GetNuGetChartsDataAsync() =>
        await http.GetFromJsonAsync<NuGetChartsData>("data/nuget-charts.json", JsonOptions);

    public async Task<CommunityStats?> GetCommunityStatsAsync() =>
        await http.GetFromJsonAsync<CommunityStats>("data/community-stats.json", JsonOptions);

    public async Task<PrTriageStats?> GetPrTriageStatsAsync() =>
        await http.GetFromJsonAsync<PrTriageStats>("data/pr-triage.json", JsonOptions);

    public async Task<IssuesData?> GetIssuesDataAsync() =>
        await http.GetFromJsonAsync<IssuesData>("data/issues.json", JsonOptions);

    public async Task<TrendData?> GetTrendDataAsync() =>
        await http.GetFromJsonAsync<TrendData>("data/github-trends.json", JsonOptions);

    public async Task<TriageData?> GetTriageDataAsync() =>
        await http.GetFromJsonAsync<TriageData>("data/triage.json", TriageJsonOptions.Default);

    public async Task<TriageIndex?> GetTriageIndexAsync() =>
        await http.GetFromJsonAsync<TriageIndex>("data/triage-index.json", TriageJsonOptions.Default);

    public async Task<TriagedIssue?> GetTriageDetailAsync(int number)
    {
        try
        {
            return await http.GetFromJsonAsync<TriagedIssue>($"data/triage/{number}.json", TriageJsonOptions.Default);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<ReproResult?> GetReproDetailAsync(int number)
    {
        try
        {
            return await http.GetFromJsonAsync<ReproResult>($"data/repro/{number}.json", TriageJsonOptions.Default);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }
}
