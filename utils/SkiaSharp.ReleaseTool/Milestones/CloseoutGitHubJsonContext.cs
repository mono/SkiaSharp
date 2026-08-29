using System.Text.Json;
using System.Text.Json.Serialization;

namespace SkiaSharp.ReleaseTool.Milestones
{
	internal sealed record ClosingIssuesGraphQlRequest(
		string Query,
		ClosingIssuesGraphQlVariables Variables);

	internal sealed record ClosingIssuesGraphQlVariables(
		string Owner,
		string Name,
		int Number,
		string? After);

	internal sealed record ClosingIssuesGraphQlResponse(
		ClosingIssuesGraphQlData? Data,
		IReadOnlyList<ClosingIssuesGraphQlError>? Errors);

	internal sealed record ClosingIssuesGraphQlData(
		ClosingIssuesGraphQlRepository? Repository);

	internal sealed record ClosingIssuesGraphQlRepository(
		ClosingIssuesGraphQlPullRequest? PullRequest);

	internal sealed record ClosingIssuesGraphQlPullRequest(
		ClosingIssuesGraphQlConnection? ClosingIssuesReferences);

	internal sealed record ClosingIssuesGraphQlConnection(
		IReadOnlyList<ClosingIssuesGraphQlIssue?>? Nodes,
		ClosingIssuesGraphQlPageInfo? PageInfo);

	internal sealed record ClosingIssuesGraphQlIssue(int Number);

	internal sealed record ClosingIssuesGraphQlPageInfo(
		bool HasNextPage,
		string? EndCursor);

	internal sealed record ClosingIssuesGraphQlError(string? Message);

	[JsonSourceGenerationOptions(
		GenerationMode = JsonSourceGenerationMode.Metadata,
		PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
		RespectNullableAnnotations = true,
		RespectRequiredConstructorParameters = true,
		UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip)]
	[JsonSerializable(typeof(ClosingIssuesGraphQlRequest))]
	[JsonSerializable(typeof(ClosingIssuesGraphQlResponse))]
	internal partial class CloseoutGitHubJsonContext : JsonSerializerContext;
}
