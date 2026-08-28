using System.CommandLine;
using System.Text.Json;
using SkiaSharp.ReleaseTool.Contracts;
using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Finishing;
using SkiaSharp.ReleaseTool.Json;
using SkiaSharp.ReleaseTool.Model;
using SkiaSharp.ReleaseTool.NuGet;

namespace SkiaSharp.ReleaseTool.Cli
{
	internal static class FinishPlanCommand
	{
		public static Command Create(
			Option<string?> repositoryOption,
			IReleaseCommandEnvironment environment)
		{
			var versionOption = new Option<string>("--version")
			{
				Description = "Exact public NuGet.org SkiaSharp version.",
				Required = true,
			};
			var toolingShaOption = new Option<string?>("--tooling-sha")
			{
				Description = "Tooling commit recorded in the plan; defaults to HEAD.",
			};
			var outputOption = new Option<string>("--output")
			{
				Description = "Finish plan or pending report output path.",
				DefaultValueFactory = _ => "finish-plan.json",
			};

			var plan = new Command("plan", "Verify the public receipt and plan tag/release state.");
			plan.Options.Add(versionOption);
			plan.Options.Add(toolingShaOption);
			plan.Options.Add(outputOption);
			plan.SetAction(async (parseResult, cancellationToken) =>
			{
				try
				{
					var repository = await environment.OpenRepositoryAsync(
						parseResult.GetValue(repositoryOption),
						cancellationToken).ConfigureAwait(false);
					await repository.FetchAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
					var toolingSha = parseResult.GetValue(toolingShaOption)
						?? await repository.ResolveAsync("HEAD", cancellationToken).ConfigureAwait(false);
					var requestedVersion = PublicReleaseVersion.Parse(
						parseResult.GetRequiredValue(versionOption)).Text;
					var output = parseResult.GetRequiredValue(outputOption);
					var outputPath = Path.Combine(repository.Root, output);
					var policies = ReleasePolicies.Load(repository.Root);

					try
					{
						var builder = new FinishPlanBuilder(
							repository,
							environment.CreatePublicReceiptVerifier(),
							environment.CreateFinishGitHubClient(),
							policies,
							environment.TimeProvider,
							environment.NewPlanId);
						var result = await builder.BuildAsync(
							new FinishPlanRequest(requestedVersion, toolingSha),
							cancellationToken).ConfigureAwait(false);
						Write(outputPath, output, result);
						await environment.StandardOutput.WriteLineAsync(
							JsonSerializer.Serialize(
								result,
								ReleaseJsonContext.Strict.FinishPlan)).ConfigureAwait(false);
						return ExitCodes.Success;
					}
					catch (PackagesPendingException ex)
					{
						var report = new FinishPendingReport(
							SchemaVersion: 1,
							Operation: FinishPendingOperation.FinishPlanPending,
							GeneratedAt: environment.TimeProvider.GetUtcNow(),
							ToolingSha: toolingSha,
							NextAction: PendingNextAction.Pending,
							RequestedVersion: requestedVersion,
							MissingPackages: ex.MissingPackages,
							ElapsedSeconds: ex.Elapsed.TotalSeconds,
							DeadlineSeconds: ex.Deadline.TotalSeconds,
							Message: ex.Message);
						Write(outputPath, output, report);
						await environment.StandardOutput.WriteLineAsync(
							JsonSerializer.Serialize(
								report,
								ReleaseJsonContext.Strict.FinishPendingReport)).ConfigureAwait(false);
						return ExitCodes.Pending;
					}
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

			var finish = new Command("finish", "Finish a published SkiaSharp package release.");
			finish.Subcommands.Add(plan);
			return finish;
		}

		private static void Write(string path, string displayPath, FinishPlan plan)
		{
			try
			{
				PlanStore.Write(path, plan);
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
			{
				throw new ReleaseToolException($"could not write finish plan '{displayPath}'", ex);
			}
		}

		private static void Write(string path, string displayPath, FinishPendingReport report)
		{
			try
			{
				PlanStore.Write(path, report);
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
			{
				throw new ReleaseToolException($"could not write finish pending report '{displayPath}'", ex);
			}
		}
	}
}
