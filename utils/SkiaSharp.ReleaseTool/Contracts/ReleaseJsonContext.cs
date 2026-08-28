using System.Text.Json;
using System.Text.Json.Serialization;
using SkiaSharp.ReleaseTool.Model;

namespace SkiaSharp.ReleaseTool.Contracts
{
	[JsonSourceGenerationOptions(
		GenerationMode = JsonSourceGenerationMode.Metadata,
		PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
		RespectNullableAnnotations = true,
		RespectRequiredConstructorParameters = true,
		UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
		WriteIndented = true)]
	[JsonSerializable(typeof(PreparePlan))]
	[JsonSerializable(typeof(FinishPlan))]
	public partial class ReleaseJsonContext : JsonSerializerContext
	{
		public static ReleaseJsonContext Strict { get; } = new(CreateOptions());

		private static JsonSerializerOptions CreateOptions()
		{
			var options = new JsonSerializerOptions
			{
				AllowDuplicateProperties = false,
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				RespectNullableAnnotations = true,
				RespectRequiredConstructorParameters = true,
				UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
				WriteIndented = true,
			};
			options.Converters.Add(new JsonStringEnumConverter<ReleaseOperation>(allowIntegerValues: false));
			options.Converters.Add(new JsonStringEnumConverter<PrepareNextAction>(allowIntegerValues: false));
			options.Converters.Add(new JsonStringEnumConverter<FinishNextAction>(allowIntegerValues: false));
			options.Converters.Add(new JsonStringEnumConverter<MaintenanceBranchAction>(allowIntegerValues: false));
			options.Converters.Add(new JsonStringEnumConverter<RemoteState>(allowIntegerValues: false));
			options.Converters.Add(new JsonStringEnumConverter<PlanOperationId>(allowIntegerValues: false));
			options.Converters.Add(new JsonStringEnumConverter<PlanOperationKind>(allowIntegerValues: false));
			options.Converters.Add(new JsonStringEnumConverter<PlanOperationStatus>(allowIntegerValues: false));
			options.Converters.Add(new JsonStringEnumConverter<CompletionStatus>(allowIntegerValues: false));
			options.Converters.Add(new JsonStringEnumConverter<ReleaseKind>(allowIntegerValues: false));
			return options;
		}
	}
}
