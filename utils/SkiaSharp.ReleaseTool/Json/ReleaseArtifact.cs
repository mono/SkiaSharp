using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using SkiaSharp.ReleaseTool.Contracts;
using SkiaSharp.ReleaseTool.Environments;
using SkiaSharp.ReleaseTool.Errors;

namespace SkiaSharp.ReleaseTool.Json
{
	internal enum ReleaseArtifactKind
	{
		PreparePlan,
		PrepareApplyResult,
		FinishPlan,
		FinishPendingReport,
		FinishCreateDraftResult,
		FinishPublicationPlan,
		FinishPublishResult,
		FinishCloseoutPlan,
		FinishCloseoutResult,
		EnvironmentCheckReport,
	}

	internal sealed record ReleaseArtifact(ReleaseArtifactKind Kind, object Value)
	{
		public string ToJson() => Kind switch
		{
			ReleaseArtifactKind.PreparePlan => Serialize(
				(PreparePlan)Value,
				ReleaseJsonContext.Strict.PreparePlan),
			ReleaseArtifactKind.PrepareApplyResult => Serialize(
				(PrepareApplyResult)Value,
				ReleaseJsonContext.Strict.PrepareApplyResult),
			ReleaseArtifactKind.FinishPlan => Serialize(
				(FinishPlan)Value,
				ReleaseJsonContext.Strict.FinishPlan),
			ReleaseArtifactKind.FinishPendingReport => Serialize(
				(FinishPendingReport)Value,
				ReleaseJsonContext.Strict.FinishPendingReport),
			ReleaseArtifactKind.FinishCreateDraftResult => Serialize(
				(FinishCreateDraftResult)Value,
				ReleaseJsonContext.Strict.FinishCreateDraftResult),
			ReleaseArtifactKind.FinishPublicationPlan => Serialize(
				(FinishPublicationPlan)Value,
				ReleaseJsonContext.Strict.FinishPublicationPlan),
			ReleaseArtifactKind.FinishPublishResult => Serialize(
				(FinishPublishResult)Value,
				ReleaseJsonContext.Strict.FinishPublishResult),
			ReleaseArtifactKind.FinishCloseoutPlan => Serialize(
				(FinishCloseoutPlan)Value,
				ReleaseJsonContext.Strict.FinishCloseoutPlan),
			ReleaseArtifactKind.FinishCloseoutResult => Serialize(
				(FinishCloseoutResult)Value,
				ReleaseJsonContext.Strict.FinishCloseoutResult),
			ReleaseArtifactKind.EnvironmentCheckReport => Serialize(
				(EnvironmentCheckReport)Value,
				EnvironmentJsonContext.Strict.EnvironmentCheckReport),
			_ => throw new InvalidOperationException($"Unsupported artifact kind {Kind}."),
		};

		private static string Serialize<T>(T value, JsonTypeInfo<T> typeInfo) =>
			JsonSerializer.Serialize(value, typeInfo);
	}

	internal static class ReleaseArtifactReader
	{
		public static ReleaseArtifact Read(string path)
		{
			if (!File.Exists(path))
				throw new ValidationException($"artifact file not found: {path}");

			byte[] bytes;
			try
			{
				bytes = File.ReadAllBytes(path);
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
			{
				throw new ValidationException($"could not read artifact '{path}'", ex);
			}

			ArtifactHeader header;
			try
			{
				header = JsonSerializer.Deserialize(
					bytes,
					ArtifactHeaderJsonContext.Strict.ArtifactHeader)
					?? throw new ValidationException("artifact must contain a JSON object");
			}
			catch (JsonException ex)
			{
				throw new ValidationException($"artifact header failed shape validation: {ex.Message}", ex);
			}

			return header.Operation switch
			{
				"prepare" => Read(
					bytes,
					ReleaseArtifactKind.PreparePlan,
					ReleaseJsonContext.Strict.PreparePlan,
					PreparePlanValidator.Validate),
				"finish" => Read(
					bytes,
					ReleaseArtifactKind.FinishPlan,
					ReleaseJsonContext.Strict.FinishPlan,
					FinishPlanValidator.Validate),
				"finish-plan-pending" => Read(
					bytes,
					ReleaseArtifactKind.FinishPendingReport,
					ReleaseJsonContext.Strict.FinishPendingReport,
					FinishPendingReportValidator.Validate),
				"finish-create-draft" => Read(
					bytes,
					ReleaseArtifactKind.FinishCreateDraftResult,
					ReleaseJsonContext.Strict.FinishCreateDraftResult,
					FinishCreateDraftResultValidator.Validate),
				"finish-plan-publication" => Read(
					bytes,
					ReleaseArtifactKind.FinishPublicationPlan,
					ReleaseJsonContext.Strict.FinishPublicationPlan,
					FinishPublicationPlanValidator.Validate),
				"finish-publish" => Read(
					bytes,
					ReleaseArtifactKind.FinishPublishResult,
					ReleaseJsonContext.Strict.FinishPublishResult,
					FinishPublishResultValidator.Validate),
				"finish-plan-closeout" => Read(
					bytes,
					ReleaseArtifactKind.FinishCloseoutPlan,
					ReleaseJsonContext.Strict.FinishCloseoutPlan,
					FinishCloseoutPlanValidator.Validate),
				"finish-closeout" => Read(
					bytes,
					ReleaseArtifactKind.FinishCloseoutResult,
					ReleaseJsonContext.Strict.FinishCloseoutResult,
					FinishCloseoutResultValidator.Validate),
				null when header.SchemaVersion is not null && header.PlanId is not null => Read(
					bytes,
					ReleaseArtifactKind.PrepareApplyResult,
					ReleaseJsonContext.Strict.PrepareApplyResult,
					PrepareApplyResultValidator.Validate),
				null when header.Name is not null && header.Exists is not null && header.Ok is not null => Read(
					bytes,
					ReleaseArtifactKind.EnvironmentCheckReport,
					EnvironmentJsonContext.Strict.EnvironmentCheckReport,
					EnvironmentCheckReportValidator.Validate),
				null => throw new ValidationException("unknown release artifact shape"),
				_ => throw new ValidationException($"unknown release artifact operation '{header.Operation}'"),
			};
		}

		private static ReleaseArtifact Read<T>(
			ReadOnlySpan<byte> bytes,
			ReleaseArtifactKind kind,
			JsonTypeInfo<T> typeInfo,
			Action<T> validate)
		{
			T value;
			try
			{
				value = JsonSerializer.Deserialize(bytes, typeInfo)
					?? throw new ValidationException("artifact must contain a JSON object");
			}
			catch (JsonException ex)
			{
				throw new ValidationException($"artifact failed shape validation: {ex.Message}", ex);
			}

			try
			{
				validate(value);
			}
			catch (ReleaseToolException)
			{
				throw;
			}
			catch (Exception ex) when (ex is ArgumentException or FormatException or OverflowException)
			{
				throw new ValidationException($"artifact failed semantic validation: {ex.Message}", ex);
			}
			return new ReleaseArtifact(kind, value);
		}
	}

	internal sealed record ArtifactHeader(
		int? SchemaVersion,
		string? Operation,
		Guid? PlanId,
		string? Name,
		bool? Exists,
		bool? Ok);

	[JsonSourceGenerationOptions(
		GenerationMode = JsonSourceGenerationMode.Metadata,
		PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
		RespectNullableAnnotations = true,
		UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip)]
	[JsonSerializable(typeof(ArtifactHeader))]
	internal partial class ArtifactHeaderJsonContext : JsonSerializerContext
	{
		public static ArtifactHeaderJsonContext Strict { get; } = new(
			new JsonSerializerOptions
			{
				AllowDuplicateProperties = false,
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				RespectNullableAnnotations = true,
				RespectRequiredConstructorParameters = false,
				UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
			});
	}
}
