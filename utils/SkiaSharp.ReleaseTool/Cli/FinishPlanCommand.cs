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
			finish.Subcommands.Add(CreateDraftCommand(repositoryOption, environment));
			finish.Subcommands.Add(PlanPublicationCommand(repositoryOption, environment));
			finish.Subcommands.Add(PublishCommand(repositoryOption, environment));
			return finish;
		}

		private static Command CreateDraftCommand(
			Option<string?> repositoryOption,
			IReleaseCommandEnvironment environment)
		{
			var planOption = PlanOption();
			var expectedPlanIdOption = ExpectedPlanIdOption();
			var outputOption = OutputOption(
				"Finish create-draft result output path.",
				"finish-create-draft-result.json");
			var command = new Command(
				"create-draft",
				"Push the immutable release tag and create or reconcile its draft.");
			command.Options.Add(planOption);
			command.Options.Add(expectedPlanIdOption);
			command.Options.Add(outputOption);
			command.SetAction(async (parseResult, cancellationToken) =>
			{
				try
				{
					var repository = await environment.OpenRepositoryAsync(
						parseResult.GetValue(repositoryOption),
						cancellationToken).ConfigureAwait(false);
					var expectedPlanId = parseResult.GetRequiredValue(expectedPlanIdOption);
					var planPath = Path.Combine(
						repository.Root,
						parseResult.GetRequiredValue(planOption));
					var approvedPlan = ReadFinish(planPath, expectedPlanId);
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
					Write(outputPath, output, result);
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
			var outputOption = OutputOption(
				"Publication approval plan output path.",
				"finish-publication-plan.json");
			var command = new Command(
				"plan-publication",
				"Bind publication approval to the exact live draft body.");
			command.Options.Add(planOption);
			command.Options.Add(expectedPlanIdOption);
			command.Options.Add(outputOption);
			command.SetAction(async (parseResult, cancellationToken) =>
			{
				try
				{
					var repository = await environment.OpenRepositoryAsync(
						parseResult.GetValue(repositoryOption),
						cancellationToken).ConfigureAwait(false);
					var expectedPlanId = parseResult.GetRequiredValue(expectedPlanIdOption);
					var planPath = Path.Combine(
						repository.Root,
						parseResult.GetRequiredValue(planOption));
					var approvedPlan = ReadFinish(planPath, expectedPlanId);
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
					Write(outputPath, output, result);
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
			var outputOption = OutputOption(
				"Finish publish result output path.",
				"finish-publish-result.json");
			var command = new Command(
				"publish",
				"Publish the approved existing draft without changing its body.");
			command.Options.Add(planOption);
			command.Options.Add(expectedPlanIdOption);
			command.Options.Add(publicationOption);
			command.Options.Add(expectedPublicationPlanIdOption);
			command.Options.Add(outputOption);
			command.SetAction(async (parseResult, cancellationToken) =>
			{
				try
				{
					var repository = await environment.OpenRepositoryAsync(
						parseResult.GetValue(repositoryOption),
						cancellationToken).ConfigureAwait(false);
					var expectedPlanId = parseResult.GetRequiredValue(expectedPlanIdOption);
					var expectedPublicationPlanId =
						parseResult.GetRequiredValue(expectedPublicationPlanIdOption);
					var planPath = Path.Combine(
						repository.Root,
						parseResult.GetRequiredValue(planOption));
					var publicationPath = Path.Combine(
						repository.Root,
						parseResult.GetRequiredValue(publicationOption));
					var approvedPlan = ReadFinish(planPath, expectedPlanId);
					var publication = ReadPublication(
						publicationPath,
						expectedPlanId,
						expectedPublicationPlanId);
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
					Write(outputPath, output, result);
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

		private static FinishPlan ReadFinish(string path, Guid expectedPlanId)
		{
			try
			{
				return PlanStore.ReadFinish(path, expectedPlanId);
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
			{
				throw new ReleaseToolException($"could not read finish plan '{path}'", ex);
			}
		}

		private static FinishPublicationPlan ReadPublication(
			string path,
			Guid expectedPlanId,
			Guid expectedPublicationPlanId)
		{
			try
			{
				return PlanStore.ReadPublication(
					path,
					expectedPlanId,
					expectedPublicationPlanId);
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
			{
				throw new ReleaseToolException($"could not read publication plan '{path}'", ex);
			}
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

		private static void Write(
			string path,
			string displayPath,
			FinishCreateDraftResult result)
		{
			try
			{
				PlanStore.Write(path, result);
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
			{
				throw new ReleaseToolException($"could not write finish create-draft result '{displayPath}'", ex);
			}
		}

		private static void Write(
			string path,
			string displayPath,
			FinishPublicationPlan plan)
		{
			try
			{
				PlanStore.Write(path, plan);
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
			{
				throw new ReleaseToolException($"could not write finish publication plan '{displayPath}'", ex);
			}
		}

		private static void Write(
			string path,
			string displayPath,
			FinishPublishResult result)
		{
			try
			{
				PlanStore.Write(path, result);
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
			{
				throw new ReleaseToolException($"could not write finish publish result '{displayPath}'", ex);
			}
		}
	}
}
