using System.CommandLine;
using System.Text.Json;
using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Git;
using SkiaSharp.ReleaseTool.Json;
using SkiaSharp.ReleaseTool.Model;
using SkiaSharp.ReleaseTool.Planning;

namespace SkiaSharp.ReleaseTool
{
	public static class Program
	{
		public static int Main(string[] args) =>
			InvokeAsync(args, new ReleaseCommandEnvironment()).GetAwaiter().GetResult();

		internal static async Task<int> InvokeAsync(
			string[] args,
			IReleaseCommandEnvironment environment)
		{
			var repoOption = new Option<string?>("--repo")
			{
				Description = "Path inside the SkiaSharp repository (defaults to the current directory).",
				Recursive = true,
			};
			var integrationTargetOption = new Option<string>("--integration-target")
			{
				Description = "The integration branch to release from.",
				Required = true,
			};
			var versionOption = new Option<string?>("--version")
			{
				Description = "Explicit release version; omitted to detect the next preview.",
			};
			var approvedBaseOption = new Option<string?>("--approved-base")
			{
				Description = "Fully-qualified audited recovery ref.",
			};
			var toolingShaOption = new Option<string?>("--tooling-sha")
			{
				Description = "Tooling commit recorded in the plan; defaults to HEAD.",
			};
			var outputOption = new Option<string>("--output")
			{
				Description = "Prepare plan output path.",
				DefaultValueFactory = _ => "prepare-plan.json",
			};

			var planCommand = new Command("plan", "Discover release state and write a read-only prepare plan.");
			planCommand.Options.Add(integrationTargetOption);
			planCommand.Options.Add(versionOption);
			planCommand.Options.Add(approvedBaseOption);
			planCommand.Options.Add(toolingShaOption);
			planCommand.Options.Add(outputOption);
			planCommand.SetAction(async (parseResult, cancellationToken) =>
			{
				try
				{
					var repository = await environment.OpenRepositoryAsync(
						parseResult.GetValue(repoOption),
						cancellationToken).ConfigureAwait(false);
					await repository.FetchAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
					var toolingSha = parseResult.GetValue(toolingShaOption)
						?? await repository.ResolveAsync("HEAD", cancellationToken).ConfigureAwait(false);
					var integrationTarget = ReleaseVersionPolicy.NormalizeIntegrationBranch(
						parseResult.GetRequiredValue(integrationTargetOption));
					var builder = new PreparePlanBuilder(
						repository,
						environment.CreateGitHubClient(),
						environment.TimeProvider,
						environment.NewPlanId);
					var plan = await builder.BuildAsync(
						new PreparePlanRequest(
							integrationTarget,
							parseResult.GetValue(versionOption),
							parseResult.GetValue(approvedBaseOption),
							toolingSha),
						cancellationToken).ConfigureAwait(false);
					var output = parseResult.GetRequiredValue(outputOption);
					try
					{
						PlanStore.Write(Path.Combine(repository.Root, output), plan);
					}
					catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
					{
						throw new ReleaseToolException($"could not write prepare plan '{output}'", ex);
					}
					await environment.StandardOutput.WriteLineAsync(
						JsonSerializer.Serialize(plan, Contracts.ReleaseJsonContext.Strict.PreparePlan)).ConfigureAwait(false);
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

			var prepareCommand = new Command("prepare", "Prepare a release.");
			prepareCommand.Subcommands.Add(planCommand);
			var root = new RootCommand("SkiaSharp release automation CLI");
			root.Options.Add(repoOption);
			root.Subcommands.Add(prepareCommand);
			return await root.Parse(args).InvokeAsync().ConfigureAwait(false);
		}
	}

	internal interface IReleaseCommandEnvironment
	{
		TextWriter StandardOutput { get; }
		TextWriter StandardError { get; }
		TimeProvider TimeProvider { get; }
		Func<Guid> NewPlanId { get; }
		Task<IReleaseRepository> OpenRepositoryAsync(
			string? path,
			CancellationToken cancellationToken);
		IPrepareGitHubClient CreateGitHubClient();
	}

	internal sealed class ReleaseCommandEnvironment : IReleaseCommandEnvironment
	{
		public TextWriter StandardOutput => Console.Out;
		public TextWriter StandardError => Console.Error;
		public TimeProvider TimeProvider => TimeProvider.System;
		public Func<Guid> NewPlanId => Guid.NewGuid;

		public async Task<IReleaseRepository> OpenRepositoryAsync(
			string? path,
			CancellationToken cancellationToken) =>
			await GitRepository.DiscoverAsync(
				path ?? Environment.CurrentDirectory,
				cancellationToken: cancellationToken).ConfigureAwait(false);

		public IPrepareGitHubClient CreateGitHubClient() => new OctokitPrepareGitHubClient();
	}
}
