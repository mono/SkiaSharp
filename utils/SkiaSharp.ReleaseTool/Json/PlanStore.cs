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
			Write(path, plan, ReleaseJsonContext.Strict.PreparePlan, PreparePlanValidator.Validate);

		public static void Write(string path, PrepareApplyResult result) =>
			Write(
				path,
				result,
				ReleaseJsonContext.Strict.PrepareApplyResult,
				PrepareApplyResultValidator.Validate);

		public static void Write(string path, FinishPlan plan) =>
			Write(path, plan, ReleaseJsonContext.Strict.FinishPlan, FinishPlanValidator.Validate);

		public static void Write(string path, FinishPendingReport report) =>
			Write(
				path,
				report,
				ReleaseJsonContext.Strict.FinishPendingReport,
				FinishPendingReportValidator.Validate);

		public static PreparePlan ReadPrepare(string path, Guid expectedPlanId) =>
			Read(path, ReleaseJsonContext.Strict.PreparePlan, PreparePlanValidator.Validate, expectedPlanId);

		private static void Write<T>(
			string path,
			T plan,
			JsonTypeInfo<T> typeInfo,
			Action<T> validate)
		{
			validate(plan);
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
			Guid expectedPlanId)
		{
			if (!File.Exists(path))
				throw new ValidationException($"plan file not found: {path}");

			T plan;
			try
			{
				plan = JsonSerializer.Deserialize(File.ReadAllBytes(path), typeInfo)
					?? throw new ValidationException("plan file must contain a JSON object");
			}
			catch (JsonException ex)
			{
				throw new ValidationException($"plan file failed shape validation: {ex.Message}", ex);
			}

			validate(plan);
			var actualPlanId = plan switch
			{
				PreparePlan prepare => prepare.PlanId,
				_ => throw new InvalidOperationException($"Unsupported plan type {typeof(T).Name}."),
			};
			if (actualPlanId != expectedPlanId)
				throw new ValidationException($"planId '{actualPlanId}' does not match expected correlation id '{expectedPlanId}'");
			return plan;
		}
	}
}
