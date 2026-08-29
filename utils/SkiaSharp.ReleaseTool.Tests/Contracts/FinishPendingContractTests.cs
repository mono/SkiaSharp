using System.Text.Json;
using SkiaSharp.ReleaseTool.Contracts;
using SkiaSharp.ReleaseTool.Errors;
using Xunit;

namespace SkiaSharp.ReleaseTool.Tests.Contracts
{
	public sealed class FinishPendingContractTests
	{
		[Fact]
		public void Pending_report_is_strict_source_generated_JSON()
		{
			var report = new FinishPendingReport(
				1,
				FinishPendingOperation.FinishPlanPending,
				new DateTimeOffset(2026, 8, 28, 12, 0, 0, TimeSpan.Zero),
				new string('a', 40),
				PendingNextAction.Pending,
				"4.152.0",
				[new PendingPackage("SkiaSharp", "4.152.0")],
				60,
				60,
				"still indexing");
			var json = JsonSerializer.Serialize(
				report,
				ReleaseJsonContext.Strict.FinishPendingReport);
			Assert.Contains("\"operation\": \"finish-plan-pending\"", json);
			var copy = JsonSerializer.Deserialize(
				json,
				ReleaseJsonContext.Strict.FinishPendingReport);
			Assert.NotNull(copy);

			Assert.Throws<JsonException>(() => JsonSerializer.Deserialize(
				json.Insert(json.IndexOf('{') + 1, "\"unknown\":true,"),
				ReleaseJsonContext.Strict.FinishPendingReport));
			Assert.Throws<JsonException>(() => JsonSerializer.Deserialize(
				json.Replace("\"nextAction\": \"pending\"", "\"nextAction\": 0", StringComparison.Ordinal),
				ReleaseJsonContext.Strict.FinishPendingReport));
			Assert.NotEqual(ExitCodes.Pending, ExitCodes.Canceled);
		}
	}
}
