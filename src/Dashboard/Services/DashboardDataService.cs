using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SkiaSharp.Dashboard.Services;

public class DashboardDataService(HttpClient http)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public async Task<GitHubStats?> GetGitHubStatsAsync() =>
        await http.GetFromJsonAsync<GitHubStats>("data/github-stats.json", JsonOptions);

    public async Task<NuGetStats?> GetNuGetStatsAsync() =>
        await http.GetFromJsonAsync<NuGetStats>("data/nuget-stats.json", JsonOptions);

    public async Task<CommunityStats?> GetCommunityStatsAsync() =>
        await http.GetFromJsonAsync<CommunityStats>("data/community-stats.json", JsonOptions);

    public async Task<PrTriageStats?> GetPrTriageStatsAsync() =>
        await http.GetFromJsonAsync<PrTriageStats>("data/pr-triage.json", JsonOptions);
}
