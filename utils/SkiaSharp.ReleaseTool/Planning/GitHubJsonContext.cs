using System.Text.Json.Serialization;

namespace SkiaSharp.ReleaseTool.Planning
{
	internal sealed record GitReferenceResponse(
		[property: JsonPropertyName("ref")] string Ref,
		[property: JsonPropertyName("object")] GitObjectResponse Object);

	internal sealed record GitObjectResponse(
		[property: JsonPropertyName("type")] string Type,
		[property: JsonPropertyName("sha")] string Sha);

	[JsonSourceGenerationOptions(
		GenerationMode = JsonSourceGenerationMode.Metadata,
		PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
		RespectNullableAnnotations = true,
		RespectRequiredConstructorParameters = true)]
	[JsonSerializable(typeof(GitReferenceResponse))]
	internal partial class GitHubJsonContext : JsonSerializerContext
	{
	}
}
