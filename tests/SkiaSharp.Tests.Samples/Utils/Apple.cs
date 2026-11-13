namespace SkiaSharp.Tests.Samples.Utils;

internal static class Apple
{
	internal static Task<(int ExitCode, string Output, string Error)> ListSimulators() =>
		ProcessRunner.Run(
			"dotnet", ["apple", "simulator", "list", "--format", "json"],
			Repo.RootDir, TimeSpan.FromSeconds(30));
}
