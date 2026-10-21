namespace SkiaSharp.Tests.Samples.Utils;

internal static class Android
{
	internal static Task<(int ExitCode, string Output, string Error)> ReadManifest(string apk) =>
		Run(["apk", "manifest", "info", "--pkg", apk, "--format", "xml"]);

	internal static Task<(int ExitCode, string Output, string Error)> Logcat(string serial) =>
		Run(["device", "logcat", "--id", serial], publishOutput: false);

	internal static Task<(int ExitCode, string Output, string Error)> LastAnr(string serial)
	{
		// The pinned Android CLI has no arbitrary shell/dumpsys command.
		var sdk = Environment.GetEnvironmentVariable("ANDROID_HOME");
		if (string.IsNullOrWhiteSpace(sdk))
			throw new InvalidOperationException("ANDROID_HOME must select the SDK for exact-device diagnostics.");
		var adb = Path.Combine(sdk, "platform-tools", OperatingSystem.IsWindows() ? "adb.exe" : "adb");
		return ProcessRunner.Run(adb, ["-s", serial, "shell", "dumpsys", "activity", "lastanr"],
			Repo.RootDir, TimeSpan.FromSeconds(30));
	}

	private static Task<(int ExitCode, string Output, string Error)> Run(string[] arguments, bool publishOutput = true) =>
		ProcessRunner.Run("dotnet", ["android", .. arguments], Repo.RootDir, TimeSpan.FromSeconds(30),
			publishOutput: publishOutput);
}
