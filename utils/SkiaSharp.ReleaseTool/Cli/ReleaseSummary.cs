using System.Text;
using SkiaSharp.ReleaseTool.Contracts;
using SkiaSharp.ReleaseTool.Errors;

namespace SkiaSharp.ReleaseTool.Cli
{
	internal static class ReleaseSummary
	{
		public static void Write(string? path, PreparePlan plan)
		{
			if (path is null)
				return;
			var output = new MarkdownBuilder();
			output.Title($"Prepare release {plan.Release.Identity}");
			output.Overview(
				("Plan ID", plan.PlanId),
				("Tooling SHA", plan.ToolingSha),
				("Version", plan.Release.Version),
				("Release branch", plan.Release.Branch),
				("Source", $"{plan.Base.Ref} @ {plan.Base.Sha}"),
				("Skia source", plan.Skia.Sha),
				("Next action", plan.NextAction));
			output.Table(
				"Operations",
				["Operation", "Status", "Detail"],
				plan.Operations.Select(value => new object?[] { value.Id, value.Status, value.Detail }));
			output.Warnings(plan.Warnings);
			Write(path, output);
		}

		public static void Write(string? path, PrepareApplyResult result)
		{
			if (path is null)
				return;
			var output = new MarkdownBuilder();
			output.Title($"Prepare result {result.Release.Identity}");
			output.Overview(
				("Plan ID", result.PlanId),
				("Tooling SHA", result.ToolingSha),
				("Version", result.Release.Version),
				("Release branch", result.Release.Branch),
				("Next action", result.NextAction),
				("Stable bump pull request", result.StableBumpPullRequestUrl));
			output.Table(
				"Operations",
				["Operation", "Status", "Pull request"],
				result.Operations.Select(value => new object?[]
				{
					value.Id, value.Status, value.PullRequestUrl,
				}));
			output.Warnings(result.Warnings);
			Write(path, output);
		}

		public static void Write(string? path, FinishPlan plan)
		{
			if (path is null)
				return;
			var output = new MarkdownBuilder();
			output.Title($"Finish release {plan.Release.Version}");
			output.Overview(
				("Plan ID", plan.PlanId),
				("Tooling SHA", plan.ToolingSha),
				("Release", plan.Release.Identity),
				("Version", plan.Release.Version),
				("Tag", plan.Release.Tag),
				("Source", $"{plan.Receipt.SourceBranch} @ {plan.Receipt.SourceCommit}"),
				("Previous tag", plan.PreviousTag),
				("Draft URL", plan.Draft.Url),
				("Next action", plan.NextAction));
			output.Table(
				"Public package receipt",
				["Package", "Version", "Source branch", "Source commit"],
				plan.Receipt.Packages.Select(value => new object?[]
				{
					value.Id, value.Version, value.SourceBranch, value.SourceCommit,
				}));
			output.Table(
				"Operations",
				["Operation", "Status", "Detail"],
				plan.Operations.Select(value => new object?[] { value.Id, value.Status, value.Detail }));
			output.Warnings(plan.Warnings);
			Write(path, output);
		}

		public static void Write(string? path, FinishPendingReport report)
		{
			if (path is null)
				return;
			var output = new MarkdownBuilder();
			output.Title($"Release {report.RequestedVersion} is pending on NuGet.org");
			output.Overview(
				("Tooling SHA", report.ToolingSha),
				("Version", report.RequestedVersion),
				("Next action", report.NextAction),
				("Elapsed seconds", report.ElapsedSeconds),
				("Deadline seconds", report.DeadlineSeconds),
				("Message", report.Message));
			output.Table(
				"Missing packages",
				["Package", "Version"],
				report.MissingPackages.Select(value => new object?[] { value.Id, value.Version }));
			Write(path, output);
		}

		public static void Write(string? path, FinishCreateDraftResult result)
		{
			if (path is null)
				return;
			var output = FinishWriteOverview(
				$"Draft result {result.Release.Version}",
				result.PlanId,
				null,
				result.ToolingSha,
				result.Release,
				result.SourceCommit,
				result.ReleaseId,
				result.ReleaseUrl,
				result.BodyHash,
				result.NextAction);
			WriteOperations(output, result.Operations);
			Write(path, output);
		}

		public static void Write(string? path, FinishPublicationPlan plan)
		{
			if (path is null)
				return;
			var output = FinishWriteOverview(
				$"Publication plan {plan.Release.Version}",
				plan.PlanId,
				plan.PublicationPlanId,
				plan.ToolingSha,
				plan.Release,
				plan.SourceCommit,
				plan.ReleaseId,
				plan.ReleaseUrl,
				plan.BodyHash,
				plan.NextAction);
			output.Overview(
				"Publication state",
				("Draft", plan.IsDraft),
				("Published", plan.IsPublished),
				("Managed markers", plan.MarkerState),
				("Generated notes", plan.HasGeneratedNotes),
				("Ready to publish", plan.ReadyToPublish));
			Write(path, output);
		}

		public static void Write(string? path, FinishPublishResult result)
		{
			if (path is null)
				return;
			var output = FinishWriteOverview(
				$"Publication result {result.Release.Version}",
				result.PlanId,
				result.PublicationPlanId,
				result.ToolingSha,
				result.Release,
				result.SourceCommit,
				result.ReleaseId,
				result.ReleaseUrl,
				result.BodyHash,
				result.NextAction);
			WriteOperations(output, result.Operations);
			Write(path, output);
		}

		public static void Write(string? path, FinishCloseoutResult result)
		{
			if (path is null)
				return;
			var output = new MarkdownBuilder();
			output.Title($"Closeout result {result.Release.Version}");
			output.Overview(
				("Plan ID", result.PlanId),
				("Tooling SHA", result.ToolingSha),
				("Release", result.Release.Identity),
				("Version", result.Release.Version),
				("Tag", result.Tag),
				("Source", $"{result.SourceBranch} @ {result.SourceCommit}"),
				("Next action", result.NextAction));
			output.Table(
				"Schedule",
				["Milestone", "Number", "Action", "Status"],
				result.ScheduleResults.Select(value => new object?[]
				{
					value.Title, value.Number, value.Action, value.Status,
				}));
			output.Table(
				"Reconciliation",
				["Kind", "Number", "Via PR", "From", "To", "Status"],
				result.ReconcileResults.Select(value => new object?[]
				{
					value.Kind,
					value.Number,
					value.ViaPullRequest,
					value.FromMilestone,
					value.ToMilestone,
					value.Status,
				}));
			output.Table(
				"Closure",
				["Milestone", "Moved to", "Status", "Detail"],
				result.ClosureResults.Select(value => new object?[]
				{
					value.Milestone, value.MovedTo, value.Status, value.Detail,
				}));
			output.Table(
				"Workflow dispatch",
				["Workflow", "Ref", "Status"],
				result.Dispatches.Select(value => new object?[]
				{
					value.Workflow, value.Ref, value.Status,
				}));
			output.Warnings(result.Warnings);
			Write(path, output);
		}

		private static MarkdownBuilder FinishWriteOverview(
			string title,
			Guid planId,
			Guid? publicationPlanId,
			string toolingSha,
			FinishReleaseInfo release,
			string sourceCommit,
			long releaseId,
			Uri releaseUrl,
			string bodyHash,
			FinishNextAction nextAction)
		{
			var output = new MarkdownBuilder();
			output.Title(title);
			output.Overview(
				("Plan ID", planId),
				("Publication plan ID", publicationPlanId),
				("Tooling SHA", toolingSha),
				("Release", release.Identity),
				("Version", release.Version),
				("Tag", release.Tag),
				("Source commit", sourceCommit),
				("Release ID", releaseId),
				("Release URL", releaseUrl),
				("Body SHA256", bodyHash),
				("Next action", nextAction));
			return output;
		}

		private static void WriteOperations(
			MarkdownBuilder output,
			IReadOnlyList<FinishWriteOperationResult> operations) =>
			output.Table(
				"Operations",
				["Operation", "Status"],
				operations.Select(value => new object?[] { value.Id, value.Status }));

		private static void Write(string path, MarkdownBuilder summary)
		{
			var fullPath = Path.GetFullPath(path);
			try
			{
				Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
				File.WriteAllText(
					fullPath,
					summary.ToString(),
					new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
			{
				throw new ReleaseToolException($"could not write release summary '{path}'", ex);
			}
		}
	}

	internal sealed class MarkdownBuilder
	{
		private readonly StringBuilder value = new();

		public void Title(string title) => value.Append("# ").AppendLine(Escape(title)).AppendLine();

		public void Overview(params (string Label, object? Value)[] rows) =>
			Overview("Overview", rows);

		public void Overview(
			string title,
			params (string Label, object? Value)[] rows)
		{
			value.Append("## ").AppendLine(Escape(title)).AppendLine();
			value.AppendLine("| Field | Value |").AppendLine("| --- | --- |");
			foreach (var row in rows.Where(static row => row.Value is not null))
				value.Append("| ").Append(Escape(row.Label)).Append(" | ").Append(Escape(row.Value)).AppendLine(" |");
			value.AppendLine();
		}

		public void Table(
			string title,
			IReadOnlyList<string> columns,
			IEnumerable<IReadOnlyList<object?>> rows)
		{
			var materialized = rows.ToArray();
			if (materialized.Length == 0)
				return;
			value.Append("## ").AppendLine(Escape(title)).AppendLine();
			value.Append("| ").Append(string.Join(" | ", columns.Select(Escape))).AppendLine(" |");
			value.Append("| ").Append(string.Join(" | ", columns.Select(static _ => "---"))).AppendLine(" |");
			foreach (var row in materialized)
				value.Append("| ").Append(string.Join(" | ", row.Select(Escape))).AppendLine(" |");
			value.AppendLine();
		}

		public void Warnings(IReadOnlyList<string> warnings)
		{
			if (warnings.Count == 0)
				return;
			value.AppendLine("## Warnings").AppendLine();
			foreach (var warning in warnings)
				value.Append("- ").AppendLine(Escape(warning));
			value.AppendLine();
		}

		public override string ToString() => value.ToString();

		private static string Escape(object? item) =>
			(item?.ToString() ?? "—")
				.Replace("|", "\\|", StringComparison.Ordinal)
				.Replace("\r", " ", StringComparison.Ordinal)
				.Replace("\n", " ", StringComparison.Ordinal);
	}
}
