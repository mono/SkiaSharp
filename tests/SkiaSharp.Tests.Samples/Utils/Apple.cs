namespace SkiaSharp.Tests.Samples.Utils;

internal static class Apple
{
	internal static Task<(int ExitCode, string Output, string Error)> CreateSimulator(
		string name, string model, string runtime)
	{
		return Execute(["simulator", "create", name, "--device-type", model, "--runtime", runtime, "--format", "json"]);
	}

	internal static Task<(int ExitCode, string Output, string Error)> BootSimulator(string udid)
	{
		return Execute(["simulator", "boot", udid, "--wait"]);
	}

	internal static Task<(int ExitCode, string Output, string Error)> ShutdownSimulator(string udid)
	{
		return Execute(["simulator", "shutdown", udid]);
	}

	internal static Task<(int ExitCode, string Output, string Error)> DeleteSimulator(string udid)
	{
		return Execute(["simulator", "delete", udid]);
	}

	private static Task<(int ExitCode, string Output, string Error)> Execute(string[] arguments)
	{
		// Resolve the Apple tool from the repository manifest, not a sample workspace.
		return ProcessRunner.Run("dotnet", ["apple", .. arguments], Repo.RootDir, TimeSpan.FromMinutes(3));
	}
}
