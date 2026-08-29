using System.CommandLine;
using System.Text.Json;
using SkiaSharp.ReleaseTool.Contracts;
using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Finishing;
using SkiaSharp.ReleaseTool.Json;
using SkiaSharp.ReleaseTool.Milestones;
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
			var summaryOption = SummaryOption("Optional finish plan or pending Markdown summary path.");

			var plan = new Command("plan", "Verify the public receipt and plan tag/release state.");
			plan.Options.Add(versionOption);
			plan.Options.Add(toolingShaOption);
			plan.Options.Add(outputOption);
			plan.Options.Add(summaryOption);
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
						Write(output, "finish plan", () => PlanStore.Write(outputPath, result));
						ReleaseSummary.Write(
							SummaryPath(repository.Root, parseResult.GetValue(summaryOption)),
							result);
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
						Write(output, "finish pending report", () => PlanStore.Write(outputPath, report));
						ReleaseSummary.Write(
							SummaryPath(repository.Root, parseResult.GetValue(summaryOption)),
							report);
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
			finish.Subcommands.Add(CreateDraftCommand(repositoryOption, environment));
			finish.Subcommands.Add(PlanPublicationCommand(repositoryOption, environment));
			finish.Subcommands.Add(PublishCommand(repositoryOption, environment));
			finish.Subcommands.Add(CloseoutCommand(repositoryOption, environment));
			return finish;
		}

		private static Command CloseoutCommand(
			Option<string?> repositoryOption,
			IReleaseCommandEnvironment environment)
		{
			var planOption = PlanOption();
			var expectedPlanIdOption = ExpectedPlanIdOption();
			var expectedPlanSha256Option = ExpectedPlanSha256Option();
			var outputOption = OutputOption(
				"Finish closeout result output path.",
				"finish-closeout.json");
			var summaryOption = SummaryOption("Optional closeout result Markdown summary path.");
			var command = new Command(
				"closeout",
				"Apply schedule, milestone reconciliation, closure, and workflow dispatch.");
			command.Options.Add(planOption);
			command.Options.Add(expectedPlanIdOption);
			command.Options.Add(expectedPlanSha256Option);
			command.Options.Add(outputOption);
			command.Options.Add(summaryOption);
			command.SetAction(async (parseResult, cancellationToken) =>
			{
				try
				{
					var repository = await environment.OpenRepositoryAsync(
						parseResult.GetValue(repositoryOption),
						cancellationToken).ConfigureAwait(false);
					var expectedPlanId = parseResult.GetRequiredValue(expectedPlanIdOption);
					var expectedPlanSha256 =
						parseResult.GetRequiredValue(expectedPlanSha256Option);
					var planPath = Path.Combine(
						repository.Root,
						parseResult.GetRequiredValue(planOption));
					var approvedPlan = ReadFinish(
						planPath,
						expectedPlanId,
						expectedPlanSha256);
					var output = parseResult.GetRequiredValue(outputOption);
					var outputPath = Path.Combine(repository.Root, output);
					var service = new FinishCloseoutService(
						repository,
						environment.CreateCloseoutGitHubClient(),
						environment.CreateChromiumScheduleClient(),
						environment.TimeProvider);
					var result = await service.ApplyAsync(
						approvedPlan,
						expectedPlanId,
						cancellationToken).ConfigureAwait(false);
					Write(output, "finish closeout result", () => PlanStore.Write(outputPath, result));
					ReleaseSummary.Write(
						SummaryPath(repository.Root, parseResult.GetValue(summaryOption)),
						result);
					await environment.StandardOutput.WriteLineAsync(
						JsonSerializer.Serialize(
							result,
							ReleaseJsonContext.Strict.FinishCloseoutResult)).ConfigureAwait(false);
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

		private static Command CreateDraftCommand(
			Option<string?> repositoryOption,
			IReleaseCommandEnvironment environment)
		{
			var planOption = PlanOption();
			var expectedPlanIdOption = ExpectedPlanIdOption();
			var expectedPlanSha256Option = ExpectedPlanSha256Option();
			var outputOption = OutputOption(
				"Finish create-draft result output path.",
				"finish-create-draft-result.json");
			var summaryOption = SummaryOption("Optional create-draft result Markdown summary path.");
			var command = new Command(
				"create-draft",
				"Push the immutable release tag and create or reconcile its draft.");
			command.Options.Add(planOption);
			command.Options.Add(expectedPlanIdOption);
			command.Options.Add(expectedPlanSha256Option);
			command.Options.Add(outputOption);
			command.Options.Add(summaryOption);
			command.SetAction(async (parseResult, cancellationToken) =>
			{
				try
				{
					var repository = await environment.OpenRepositoryAsync(
						parseResult.GetValue(repositoryOption),
						cancellationToken).ConfigureAwait(false);
					var expectedPlanId = parseResult.GetRequiredValue(expectedPlanIdOption);
					var expectedPlanSha256 =
						parseResult.GetRequiredValue(expectedPlanSha256Option);
					var planPath = Path.Combine(
						repository.Root,
						parseResult.GetRequiredValue(planOption));
					var approvedPlan = ReadFinish(
						planPath,
						expectedPlanId,
						expectedPlanSha256);
					var output = parseResult.GetRequiredValue(outputOption);
					var outputPath = Path.Combine(repository.Root, output);
					var result = await new FinishService(
						repository,
						environment.CreateFinishGitHubWriteClient(),
						environment.TimeProvider,
						environment.NewPlanId).CreateDraftAsync(
							approvedPlan,
							expectedPlanId,
							cancellationToken,
							AllowedArtifacts(
								repository.Root,
								planPath,
								outputPath)).ConfigureAwait(false);
					Write(output, "finish create-draft result", () => PlanStore.Write(outputPath, result));
					ReleaseSummary.Write(
						SummaryPath(repository.Root, parseResult.GetValue(summaryOption)),
						result);
					await environment.StandardOutput.WriteLineAsync(
						JsonSerializer.Serialize(
							result,
							ReleaseJsonContext.Strict.FinishCreateDraftResult)).ConfigureAwait(false);
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

		private static Command PlanPublicationCommand(
			Option<string?> repositoryOption,
			IReleaseCommandEnvironment environment)
		{
			var planOption = PlanOption();
			var expectedPlanIdOption = ExpectedPlanIdOption();
			var expectedPlanSha256Option = ExpectedPlanSha256Option();
			var outputOption = OutputOption(
				"Publication approval plan output path.",
				"finish-publication-plan.json");
			var summaryOption = SummaryOption("Optional publication plan Markdown summary path.");
			var command = new Command(
				"plan-publication",
				"Bind publication approval to the exact live draft body.");
			command.Options.Add(planOption);
			command.Options.Add(expectedPlanIdOption);
			command.Options.Add(expectedPlanSha256Option);
			command.Options.Add(outputOption);
			command.Options.Add(summaryOption);
			command.SetAction(async (parseResult, cancellationToken) =>
			{
				try
				{
					var repository = await environment.OpenRepositoryAsync(
						parseResult.GetValue(repositoryOption),
						cancellationToken).ConfigureAwait(false);
					var expectedPlanId = parseResult.GetRequiredValue(expectedPlanIdOption);
					var expectedPlanSha256 =
						parseResult.GetRequiredValue(expectedPlanSha256Option);
					var planPath = Path.Combine(
						repository.Root,
						parseResult.GetRequiredValue(planOption));
					var approvedPlan = ReadFinish(
						planPath,
						expectedPlanId,
						expectedPlanSha256);
					var output = parseResult.GetRequiredValue(outputOption);
					var outputPath = Path.Combine(repository.Root, output);
					var result = await new FinishService(
						repository,
						environment.CreateFinishGitHubClient(),
						environment.TimeProvider,
						environment.NewPlanId).PlanPublicationAsync(
							approvedPlan,
							expectedPlanId,
							cancellationToken).ConfigureAwait(false);
					Write(output, "finish publication plan", () => PlanStore.Write(outputPath, result));
					ReleaseSummary.Write(
						SummaryPath(repository.Root, parseResult.GetValue(summaryOption)),
						result);
					await environment.StandardOutput.WriteLineAsync(
						JsonSerializer.Serialize(
							result,
							ReleaseJsonContext.Strict.FinishPublicationPlan)).ConfigureAwait(false);
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

		private static Command PublishCommand(
			Option<string?> repositoryOption,
			IReleaseCommandEnvironment environment)
		{
			var planOption = PlanOption();
			var expectedPlanIdOption = ExpectedPlanIdOption();
			var expectedPlanSha256Option = ExpectedPlanSha256Option();
			var publicationOption = new Option<string>("--publication")
			{
				Description = "Approved finish plan-publication artifact.",
				Required = true,
			};
			var expectedPublicationPlanIdOption = new Option<Guid>("--expected-publication-plan-id")
			{
				Description = "Publication correlation identifier emitted by finish plan-publication.",
				Required = true,
			};
			var expectedPublicationSha256Option = new Option<string>("--expected-publication-sha256")
			{
				Description = "Exact lowercase SHA256 digest of the approved publication plan bytes.",
				Required = true,
			};
			var outputOption = OutputOption(
				"Finish publish result output path.",
				"finish-publish-result.json");
			var summaryOption = SummaryOption("Optional publication result Markdown summary path.");
			var command = new Command(
				"publish",
				"Publish the approved existing draft without changing its body.");
			command.Options.Add(planOption);
			command.Options.Add(expectedPlanIdOption);
			command.Options.Add(expectedPlanSha256Option);
			command.Options.Add(publicationOption);
			command.Options.Add(expectedPublicationPlanIdOption);
			command.Options.Add(expectedPublicationSha256Option);
			command.Options.Add(outputOption);
			command.Options.Add(summaryOption);
			command.SetAction(async (parseResult, cancellationToken) =>
			{
				try
				{
					var repository = await environment.OpenRepositoryAsync(
						parseResult.GetValue(repositoryOption),
						cancellationToken).ConfigureAwait(false);
					var expectedPlanId = parseResult.GetRequiredValue(expectedPlanIdOption);
					var expectedPlanSha256 =
						parseResult.GetRequiredValue(expectedPlanSha256Option);
					var expectedPublicationPlanId =
						parseResult.GetRequiredValue(expectedPublicationPlanIdOption);
					var expectedPublicationSha256 =
						parseResult.GetRequiredValue(expectedPublicationSha256Option);
					var planPath = Path.Combine(
						repository.Root,
						parseResult.GetRequiredValue(planOption));
					var publicationPath = Path.Combine(
						repository.Root,
						parseResult.GetRequiredValue(publicationOption));
					var approvedPlan = ReadFinish(
						planPath,
						expectedPlanId,
						expectedPlanSha256);
					var publication = ReadPublication(
						publicationPath,
						expectedPlanId,
						expectedPublicationPlanId,
						expectedPublicationSha256);
					var output = parseResult.GetRequiredValue(outputOption);
					var outputPath = Path.Combine(repository.Root, output);
					var result = await new FinishService(
						repository,
						environment.CreateFinishGitHubWriteClient(),
						environment.TimeProvider,
						environment.NewPlanId).PublishAsync(
							approvedPlan,
							expectedPlanId,
							publication,
							expectedPublicationPlanId,
							cancellationToken,
							AllowedArtifacts(
								repository.Root,
								planPath,
								publicationPath,
								outputPath)).ConfigureAwait(false);
					Write(output, "finish publish result", () => PlanStore.Write(outputPath, result));
					ReleaseSummary.Write(
						SummaryPath(repository.Root, parseResult.GetValue(summaryOption)),
						result);
					await environment.StandardOutput.WriteLineAsync(
						JsonSerializer.Serialize(
							result,
							ReleaseJsonContext.Strict.FinishPublishResult)).ConfigureAwait(false);
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

		private static Option<string> PlanOption() =>
			new("--plan")
			{
				Description = "Approved FinishPlan artifact.",
				Required = true,
			};

		private static Option<Guid> ExpectedPlanIdOption() =>
			new("--expected-plan-id")
			{
				Description = "Plan correlation identifier emitted by finish plan.",
				Required = true,
			};

		private static Option<string> ExpectedPlanSha256Option() =>
			new("--expected-plan-sha256")
			{
				Description = "Exact lowercase SHA256 digest of the approved finish plan bytes.",
				Required = true,
			};

		private static Option<string?> SummaryOption(string description) =>
			new("--summary")
			{
				Description = description,
			};

		private static Option<string> OutputOption(
			string description,
			string defaultPath) =>
			new("--output")
			{
				Description = description,
				DefaultValueFactory = _ => defaultPath,
			};

		private static IReadOnlyList<string> AllowedArtifacts(
			string repositoryRoot,
			params string[] paths) =>
			paths
				.Concat(
				[
					Path.Combine(repositoryRoot, "finish-plan.json"),
					Path.Combine(repositoryRoot, "finish-create-draft-result.json"),
					Path.Combine(repositoryRoot, "finish-publication-plan.json"),
					Path.Combine(repositoryRoot, "finish-publish-result.json"),
				])
				.Distinct(StringComparer.Ordinal)
				.ToArray();

		private static FinishPlan ReadFinish(
			string path,
			Guid expectedPlanId,
			string expectedPlanSha256)
		{
			try
			{
				return PlanStore.ReadFinish(path, expectedPlanId, expectedPlanSha256);
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
			{
				throw new ReleaseToolException($"could not read finish plan '{path}'", ex);
			}
		}

		private static FinishPublicationPlan ReadPublication(
			string path,
			Guid expectedPlanId,
			Guid expectedPublicationPlanId,
			string expectedPublicationSha256)
		{
			try
			{
				return PlanStore.ReadPublication(
					path,
					expectedPlanId,
					expectedPublicationPlanId,
					expectedPublicationSha256);
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
			{
				throw new ReleaseToolException($"could not read publication plan '{path}'", ex);
			}
		}

		private static void Write(
			string displayPath,
			string description,
			Action write)
		{
			try
			{
				write();
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
			{
				throw new ReleaseToolException($"could not write {description} '{displayPath}'", ex);
			}
		}

		private static string? SummaryPath(string repositoryRoot, string? path) =>
			string.IsNullOrWhiteSpace(path) ? null : Path.Combine(repositoryRoot, path);
	}
}
