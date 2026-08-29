using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using SkiaSharp.ReleaseTool.Contracts;
using SkiaSharp.ReleaseTool.Errors;

namespace SkiaSharp.ReleaseTool.Json
{
	/// <summary>Strict, source-generated persistence for release plans.</summary>
	public static class PlanStore
	{
		public static void Write(string path, PreparePlan plan) =>
			Write(path, plan, ReleaseJsonContext.Strict.PreparePlan);

		public static void Write(string path, PrepareApplyResult result) =>
			Write(path, result, ReleaseJsonContext.Strict.PrepareApplyResult);

		public static void Write(string path, FinishPlan plan) =>
			Write(path, plan, ReleaseJsonContext.Strict.FinishPlan);

		public static void Write(string path, FinishPendingReport report) =>
			Write(path, report, ReleaseJsonContext.Strict.FinishPendingReport);

		public static void Write(string path, FinishCreateDraftResult result) =>
			Write(path, result, ReleaseJsonContext.Strict.FinishCreateDraftResult);

		public static void Write(string path, FinishPublicationPlan plan) =>
			Write(path, plan, ReleaseJsonContext.Strict.FinishPublicationPlan);

		public static void Write(string path, FinishPublishResult result) =>
			Write(path, result, ReleaseJsonContext.Strict.FinishPublishResult);

		public static void Write(string path, FinishCloseoutResult result) =>
			Write(path, result, ReleaseJsonContext.Strict.FinishCloseoutResult);

		public static PreparePlan ReadPrepare(
			string path,
			Guid expectedPlanId,
			string expectedSha256) =>
			Read(
				path,
				ReleaseJsonContext.Strict.PreparePlan,
				PreparePlanValidator.Validate,
				expectedPlanId,
				expectedSha256);

		public static FinishPlan ReadFinish(
			string path,
			Guid expectedPlanId,
			string expectedSha256) =>
			Read(
				path,
				ReleaseJsonContext.Strict.FinishPlan,
				FinishPlanValidator.Validate,
				expectedPlanId,
				expectedSha256);

		public static FinishPublicationPlan ReadPublication(
			string path,
			Guid expectedPlanId,
			Guid expectedPublicationPlanId,
			string expectedSha256)
		{
			var plan = Read(
				path,
				ReleaseJsonContext.Strict.FinishPublicationPlan,
				FinishPublicationPlanValidator.Validate,
				expectedPlanId,
				expectedSha256);
			if (plan.PublicationPlanId != expectedPublicationPlanId)
			{
				throw new ValidationException(
					$"publicationPlanId '{plan.PublicationPlanId}' does not match expected correlation id '{expectedPublicationPlanId}'");
			}
			return plan;
		}

		private static void Write<T>(
			string path,
			T plan,
			JsonTypeInfo<T> typeInfo)
		{
			var bytes = JsonSerializer.SerializeToUtf8Bytes(plan, typeInfo);
			var fullPath = Path.GetFullPath(path);
			var directory = Path.GetDirectoryName(fullPath)!;
			Directory.CreateDirectory(directory);
			var stagingPath = Path.Combine(
				directory,
				$".{Path.GetFileName(path)}.{Guid.NewGuid():N}.writing");
			try
			{
				File.WriteAllBytes(stagingPath, bytes);
				File.Move(stagingPath, fullPath, overwrite: true);
			}
			finally
			{
				File.Delete(stagingPath);
			}
		}

		private static T Read<T>(
			string path,
			JsonTypeInfo<T> typeInfo,
			Action<T> validate,
			Guid expectedPlanId,
			string expectedSha256)
		{
			T plan;
			try
			{
				var bytes = ArtifactHash.ReadAndVerify(path, expectedSha256);
				plan = JsonSerializer.Deserialize(bytes, typeInfo)
					?? throw new ValidationException("plan file must contain a JSON object");
			}
			catch (JsonException ex)
			{
				throw new ValidationException($"plan file failed shape validation: {ex.Message}", ex);
			}

			try
			{
				validate(plan);
			}
			catch (ReleaseToolException)
			{
				throw;
			}
			catch (Exception ex) when (ex is ArgumentException or FormatException or OverflowException)
			{
				throw new ValidationException($"plan file failed semantic validation: {ex.Message}", ex);
			}
			var actualPlanId = plan switch
			{
				PreparePlan prepare => prepare.PlanId,
				FinishPlan finish => finish.PlanId,
				FinishPublicationPlan publication => publication.PlanId,
				_ => throw new InvalidOperationException($"Unsupported plan type {typeof(T).Name}."),
			};
			if (actualPlanId != expectedPlanId)
				throw new ValidationException($"planId '{actualPlanId}' does not match expected correlation id '{expectedPlanId}'");
			return plan;
		}
	}
}
