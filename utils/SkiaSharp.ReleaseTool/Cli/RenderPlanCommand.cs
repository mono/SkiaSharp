using System.CommandLine;
using System.Text;
using SkiaSharp.ReleaseTool.Contracts;
using SkiaSharp.ReleaseTool.Environments;
using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Json;

namespace SkiaSharp.ReleaseTool.Cli
{
	internal static class RenderPlanCommand
	{
		public static Command Create(IReleaseCommandEnvironment environment)
		{
			var planOption = new Option<string>("--plan")
			{
				Description = "Strictly validated release artifact to render.",
				Required = true,
			};
			var formatOption = new Option<string>("--format")
			{
				Description = "Output format: json or markdown.",
				Required = true,
			};
			var outputOption = new Option<string>("--output")
			{
				Description = "Rendered output path.",
				Required = true,
			};
			var command = new Command(
				"render-plan",
				"Validate and render a release plan, result, pending report, or environment report.");
			command.Options.Add(planOption);
			command.Options.Add(formatOption);
			command.Options.Add(outputOption);
			command.SetAction(async (parseResult, cancellationToken) =>
			{
				try
				{
					var plan = Path.GetFullPath(parseResult.GetRequiredValue(planOption));
					var format = parseResult.GetRequiredValue(formatOption);
					var artifact = ReleaseArtifactReader.Read(plan);
					var rendered = format switch
					{
						"json" => artifact.ToJson() + Environment.NewLine,
						"markdown" => ReleaseArtifactMarkdown.Render(artifact),
						_ => throw new ValidationException(
							$"unsupported render format '{format}'; expected 'json' or 'markdown'"),
					};
					var output = Path.GetFullPath(parseResult.GetRequiredValue(outputOption));
					try
					{
						Directory.CreateDirectory(Path.GetDirectoryName(output)!);
						await File.WriteAllTextAsync(
							output,
							rendered,
							new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
							cancellationToken).ConfigureAwait(false);
					}
					catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
					{
						throw new ReleaseToolException($"could not write rendered artifact '{output}'", ex);
					}
					return ExitCodes.Success;
				}
				catch (OperationCanceledException)
				{
					await environment.StandardError.WriteLineAsync("error: operation canceled").ConfigureAwait(false);
					return ExitCodes.Canceled;
				}
				catch (ReleaseToolException ex)
				{
					await environment.StandardError.WriteLineAsync($"error: {ex.Message}").ConfigureAwait(false);
					return ExitCodes.GenericError;
				}
			});
			return command;
		}
	}

	internal static class ReleaseArtifactMarkdown
	{
		public static string Render(ReleaseArtifact artifact)
		{
			var markdown = new MarkdownBuilder();
			switch (artifact.Kind)
			{
				case ReleaseArtifactKind.PreparePlan:
					Render(markdown, (PreparePlan)artifact.Value);
					break;
				case ReleaseArtifactKind.PrepareApplyResult:
					Render(markdown, (PrepareApplyResult)artifact.Value);
					break;
				case ReleaseArtifactKind.FinishPlan:
					Render(markdown, (FinishPlan)artifact.Value);
					break;
				case ReleaseArtifactKind.FinishPendingReport:
					Render(markdown, (FinishPendingReport)artifact.Value);
					break;
				case ReleaseArtifactKind.FinishCreateDraftResult:
					Render(markdown, (FinishCreateDraftResult)artifact.Value);
					break;
				case ReleaseArtifactKind.FinishPublicationPlan:
					Render(markdown, (FinishPublicationPlan)artifact.Value);
					break;
				case ReleaseArtifactKind.FinishPublishResult:
					Render(markdown, (FinishPublishResult)artifact.Value);
					break;
				case ReleaseArtifactKind.FinishCloseoutPlan:
					Render(markdown, (FinishCloseoutPlan)artifact.Value);
					break;
				case ReleaseArtifactKind.FinishCloseoutResult:
					Render(markdown, (FinishCloseoutResult)artifact.Value);
					break;
				case ReleaseArtifactKind.EnvironmentCheckReport:
					Render(markdown, (EnvironmentCheckReport)artifact.Value);
					break;
				default:
					throw new InvalidOperationException($"Unsupported artifact kind {artifact.Kind}.");
			}
			return markdown.ToString();
		}

		private static void Render(MarkdownBuilder output, PreparePlan plan)
		{
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
				["Operation", "Kind", "Status", "Detail"],
				plan.Operations.Select(value => new object?[]
				{
					value.Id, value.Kind, value.Status, value.Detail,
				}));
			if (plan.StableBump is { } bump)
			{
				output.Overview(
					"Stable bump",
					("Branch", bump.BumpBranch),
					("SkiaSharp version", bump.SkiaSharpVersion),
					("HarfBuzzSharp version", bump.HarfBuzzSharpVersion),
					("Status", bump.Status),
					("Pull request", bump.PullRequestUrl));
			}
			output.Warnings(plan.Warnings);
		}

		private static void Render(MarkdownBuilder output, PrepareApplyResult result)
		{
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
		}

		private static void Render(MarkdownBuilder output, FinishPlan plan)
		{
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
				["Operation", "Kind", "Status", "Detail"],
				plan.Operations.Select(value => new object?[]
				{
					value.Id, value.Kind, value.Status, value.Detail,
				}));
			output.Warnings(plan.Warnings);
		}

		private static void Render(MarkdownBuilder output, FinishPendingReport report)
		{
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
		}

		private static void Render(MarkdownBuilder output, FinishCreateDraftResult result)
		{
			output.Title($"Draft result {result.Release.Version}");
			RenderFinishWriteOverview(
				output,
				result.PlanId,
				null,
				result.ToolingSha,
				result.Release,
				result.SourceCommit,
				result.ReleaseId,
				result.ReleaseUrl,
				result.BodyHashAlgorithm,
				result.BodyHash,
				result.NextAction);
			RenderFinishWriteOperations(output, result.Operations);
		}

		private static void Render(MarkdownBuilder output, FinishPublicationPlan plan)
		{
			output.Title($"Publication plan {plan.Release.Version}");
			RenderFinishWriteOverview(
				output,
				plan.PlanId,
				plan.PublicationPlanId,
				plan.ToolingSha,
				plan.Release,
				plan.SourceCommit,
				plan.ReleaseId,
				plan.ReleaseUrl,
				plan.BodyHashAlgorithm,
				plan.BodyHash,
				plan.NextAction);
			output.Overview(
				"Publication state",
				("Draft", plan.IsDraft),
				("Published", plan.IsPublished),
				("Managed markers", plan.MarkerState),
				("Generated notes", plan.HasGeneratedNotes),
				("Ready to publish", plan.ReadyToPublish));
		}

		private static void Render(MarkdownBuilder output, FinishPublishResult result)
		{
			output.Title($"Publication result {result.Release.Version}");
			RenderFinishWriteOverview(
				output,
				result.PlanId,
				result.PublicationPlanId,
				result.ToolingSha,
				result.Release,
				result.SourceCommit,
				result.ReleaseId,
				result.ReleaseUrl,
				result.BodyHashAlgorithm,
				result.BodyHash,
				result.NextAction);
			RenderFinishWriteOperations(output, result.Operations);
		}

		private static void Render(MarkdownBuilder output, FinishCloseoutPlan plan)
		{
			output.Title($"Closeout plan {plan.Release.Version}");
			RenderCloseoutOverview(
				output,
				plan.PlanId,
				plan.ToolingSha,
				plan.Release,
				plan.SourceBranch,
				plan.SourceCommit,
				plan.Tag,
				plan.NextAction);
			output.Table(
				"Schedule",
				["Milestone", "Number", "Action", "Status", "Due"],
				plan.ScheduleOperations.Select(value => new object?[]
				{
					value.Title, value.Number, value.Action, value.Status, value.DueOn,
				}));
			output.Table(
				"Reconciliation",
				["Kind", "Number", "Via PR", "From", "To", "Status"],
				plan.ReconcileOperations.Select(value => new object?[]
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
				["Milestone", "Tag", "Open items", "Move to", "Status", "Detail"],
				plan.ClosureOperations.Select(value => new object?[]
				{
					value.Milestone,
					value.Tag,
					value.OpenItemCount,
					value.MoveTo,
					value.Status,
					value.Detail,
				}));
			RenderDispatches(output, plan.Dispatches);
			output.Warnings(plan.Warnings);
		}

		private static void Render(MarkdownBuilder output, FinishCloseoutResult result)
		{
			output.Title($"Closeout result {result.Release.Version}");
			RenderCloseoutOverview(
				output,
				result.PlanId,
				result.ToolingSha,
				result.Release,
				result.SourceBranch,
				result.SourceCommit,
				result.Tag,
				result.NextAction);
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
			RenderDispatches(output, result.Dispatches);
			output.Warnings(result.Warnings);
		}

		private static void Render(MarkdownBuilder output, EnvironmentCheckReport report)
		{
			output.Title($"Release environment {report.Name}");
			output.Overview(
				("Exists", report.Exists),
				("Policy satisfied", report.Ok),
				("Default branch", report.DefaultBranch),
				("Allowed branches", string.Join(", ", report.AllowedBranches)),
				("Protection rules", string.Join(", ", report.ProtectionRuleTypes)),
				("Reviewer count", report.ReviewerCount),
				("Prevent self review", report.PreventSelfReview),
				("Custom branch policies", report.CustomBranchPolicies));
			if (report.Reasons.Count > 0)
			{
				output.Section("Policy findings");
				foreach (var reason in report.Reasons)
					output.Bullet(reason);
			}
		}

		private static void RenderFinishWriteOverview(
			MarkdownBuilder output,
			Guid planId,
			Guid? publicationPlanId,
			string toolingSha,
			FinishReleaseInfo release,
			string sourceCommit,
			long releaseId,
			Uri releaseUrl,
			BodyHashAlgorithm bodyHashAlgorithm,
			string bodyHash,
			FinishNextAction nextAction)
		{
			var rows = new List<(string, object?)>
			{
				("Plan ID", planId),
			};
			if (publicationPlanId is not null)
				rows.Add(("Publication plan ID", publicationPlanId));
			rows.AddRange(
			[
				("Tooling SHA", toolingSha),
				("Release", release.Identity),
				("Version", release.Version),
				("Tag", release.Tag),
				("Source commit", sourceCommit),
				("Release ID", releaseId),
				("Release URL", releaseUrl),
				("Body hash", $"{bodyHashAlgorithm}:{bodyHash}"),
				("Next action", nextAction),
			]);
			output.Overview(rows.ToArray());
		}

		private static void RenderFinishWriteOperations(
			MarkdownBuilder output,
			IReadOnlyList<FinishWriteOperationResult> operations) =>
			output.Table(
				"Operations",
				["Operation", "Status"],
				operations.Select(value => new object?[] { value.Id, value.Status }));

		private static void RenderCloseoutOverview(
			MarkdownBuilder output,
			Guid planId,
			string toolingSha,
			FinishReleaseInfo release,
			string sourceBranch,
			string sourceCommit,
			string tag,
			FinishCloseoutNextAction nextAction) =>
			output.Overview(
				("Plan ID", planId),
				("Tooling SHA", toolingSha),
				("Release", release.Identity),
				("Version", release.Version),
				("Tag", tag),
				("Source", $"{sourceBranch} @ {sourceCommit}"),
				("Next action", nextAction));

		private static void RenderDispatches(
			MarkdownBuilder output,
			IReadOnlyList<FinishWorkflowDispatch> dispatches) =>
			output.Table(
				"Workflow dispatch",
				["Workflow", "Ref", "Inputs", "Status"],
				dispatches.Select(value => new object?[]
				{
					value.Workflow,
					value.Ref,
					string.Join(
						", ",
						value.Inputs
							.OrderBy(item => item.Key, StringComparer.Ordinal)
							.Select(item => $"{item.Key}={item.Value}")),
					value.Status,
				}));
	}

	internal sealed class MarkdownBuilder
	{
		private readonly StringBuilder value = new();

		public void Title(string title) => value.Append("# ").AppendLine(Escape(title)).AppendLine();

		public void Section(string title) => value.Append("## ").AppendLine(Escape(title)).AppendLine();

		public void Bullet(object? item) => value.Append("- ").AppendLine(Escape(item));

		public void Overview(params (string Label, object? Value)[] rows) =>
			Overview("Overview", rows);

		public void Overview(
			string title,
			params (string Label, object? Value)[] rows)
		{
			Section(title);
			value.AppendLine("| Field | Value |");
			value.AppendLine("| --- | --- |");
			foreach (var row in rows)
			{
				value.Append("| ")
					.Append(Escape(row.Label))
					.Append(" | ")
					.Append(Escape(row.Value))
					.AppendLine(" |");
			}
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
			Section(title);
			value.Append("| ").Append(string.Join(" | ", columns.Select(Escape))).AppendLine(" |");
			value.Append("| ").Append(string.Join(" | ", columns.Select(static _ => "---"))).AppendLine(" |");
			foreach (var row in materialized)
			{
				value.Append("| ")
					.Append(string.Join(" | ", row.Select(Escape)))
					.AppendLine(" |");
			}
			value.AppendLine();
		}

		public void Warnings(IReadOnlyList<string> warnings)
		{
			if (warnings.Count == 0)
				return;
			Section("Warnings");
			foreach (var warning in warnings)
				Bullet(warning);
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
