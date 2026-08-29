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
	[JsonSerializable(typeof(PrepareApplyResult))]
	[JsonSerializable(typeof(FinishPlan))]
	[JsonSerializable(typeof(FinishPendingReport))]
	[JsonSerializable(typeof(FinishCreateDraftResult))]
	[JsonSerializable(typeof(FinishPublicationPlan))]
	[JsonSerializable(typeof(FinishPublishResult))]
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
			options.Converters.Add(new JsonStringEnumConverter<MaintenanceBranchAction>(allowIntegerValues: false));
			options.Converters.Add(new JsonStringEnumConverter<RemoteState>(allowIntegerValues: false));
			options.Converters.Add(new JsonStringEnumConverter<PlanOperationId>(allowIntegerValues: false));
			options.Converters.Add(new JsonStringEnumConverter<PlanOperationKind>(allowIntegerValues: false));
			options.Converters.Add(new JsonStringEnumConverter<PlanOperationStatus>(allowIntegerValues: false));
			options.Converters.Add(new JsonStringEnumConverter<ApplyOperationStatus>(allowIntegerValues: false));
			options.Converters.Add(new JsonStringEnumConverter<FinishPendingOperation>(allowIntegerValues: false));
			options.Converters.Add(new JsonStringEnumConverter<FinishNextAction>(allowIntegerValues: false));
			options.Converters.Add(new JsonStringEnumConverter<PendingNextAction>(allowIntegerValues: false));
			options.Converters.Add(new JsonStringEnumConverter<FinishState>(allowIntegerValues: false));
			options.Converters.Add(new JsonStringEnumConverter<ManagedMarkerState>(allowIntegerValues: false));
			options.Converters.Add(new JsonStringEnumConverter<FinishOperationId>(allowIntegerValues: false));
			options.Converters.Add(new JsonStringEnumConverter<FinishOperationKind>(allowIntegerValues: false));
			options.Converters.Add(new JsonStringEnumConverter<FinishArtifactOperation>(allowIntegerValues: false));
			options.Converters.Add(new JsonStringEnumConverter<FinishWriteStatus>(allowIntegerValues: false));
			options.Converters.Add(new JsonStringEnumConverter<BodyHashAlgorithm>(allowIntegerValues: false));
			options.Converters.Add(new JsonStringEnumConverter<ReleaseKind>(allowIntegerValues: false));
			return options;
		}
	}
}
