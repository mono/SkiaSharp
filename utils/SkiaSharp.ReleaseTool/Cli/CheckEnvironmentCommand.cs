using System.CommandLine;
using System.Text.Json;
using SkiaSharp.ReleaseTool.Environments;
using SkiaSharp.ReleaseTool.Errors;

namespace SkiaSharp.ReleaseTool.Cli
{
	internal static class CheckEnvironmentCommand
	{
		public static Command Create(IReleaseCommandEnvironment environment)
		{
			var nameOption = new Option<string>("--name")
			{
				Description = "The GitHub Actions environment name.",
				Required = true,
			};
			var defaultBranchOption = new Option<string>("--default-branch")
			{
				Description = "The exact repository default branch allowed to deploy.",
				Required = true,
			};
			var outputOption = new Option<string?>("--output")
			{
				Description = "Optional environment report output path.",
			};
			var command = new Command(
				"check-environment",
				"Read-only validation of a mono/SkiaSharp GitHub Actions environment.");
			command.Options.Add(nameOption);
			command.Options.Add(defaultBranchOption);
			command.Options.Add(outputOption);
			command.SetAction(async (parseResult, cancellationToken) =>
			{
				try
				{
					var name = parseResult.GetRequiredValue(nameOption);
					var defaultBranch = parseResult.GetRequiredValue(defaultBranchOption);
					var snapshot = await environment.CreateEnvironmentGitHubClient()
						.GetEnvironmentAsync(name, cancellationToken).ConfigureAwait(false);
					var report = GitHubEnvironmentPolicy.Check(snapshot, name, defaultBranch);
					var json = JsonSerializer.Serialize(report, EnvironmentJsonContext.Strict.EnvironmentCheckReport);
					var output = parseResult.GetValue(outputOption);
					if (!string.IsNullOrWhiteSpace(output))
					{
						try
						{
							var fullPath = Path.GetFullPath(output);
							Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
							await File.WriteAllTextAsync(fullPath, json, cancellationToken).ConfigureAwait(false);
						}
						catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
						{
							throw new ReleaseToolException($"could not write environment report '{output}'", ex);
						}
					}
					await environment.StandardOutput.WriteLineAsync(json).ConfigureAwait(false);
					return report.Ok ? ExitCodes.Success : ExitCodes.GenericError;
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
}
