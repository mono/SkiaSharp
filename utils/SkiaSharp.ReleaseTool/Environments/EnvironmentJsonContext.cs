using System.Text.Json;
using System.Text.Json.Serialization;

namespace SkiaSharp.ReleaseTool.Environments
{
	[JsonSourceGenerationOptions(
		GenerationMode = JsonSourceGenerationMode.Metadata,
		PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
		RespectNullableAnnotations = true,
		RespectRequiredConstructorParameters = true)]
	[JsonSerializable(typeof(GitHubEnvironmentResponse))]
	[JsonSerializable(typeof(GitHubBranchPolicyPage))]
	[JsonSerializable(typeof(EnvironmentCheckReport))]
	internal partial class EnvironmentJsonContext : JsonSerializerContext
	{
		public static EnvironmentJsonContext Api { get; } = new(CreateApiOptions());
		public static EnvironmentJsonContext Strict { get; } = new(CreateStrictOptions());

		private static JsonSerializerOptions CreateApiOptions() =>
			new()
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				PropertyNameCaseInsensitive = false,
				RespectNullableAnnotations = true,
				RespectRequiredConstructorParameters = true,
				UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
				WriteIndented = true,
			};

		private static JsonSerializerOptions CreateStrictOptions() =>
			new()
			{
				AllowDuplicateProperties = false,
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				RespectNullableAnnotations = true,
				RespectRequiredConstructorParameters = true,
				UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
				WriteIndented = true,
			};
	}
}
