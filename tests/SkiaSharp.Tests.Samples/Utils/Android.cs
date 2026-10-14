using System.Runtime.InteropServices;

namespace SkiaSharp.Tests.Samples.Utils;

// The caller selects the installed SDK; repository tool resolution remains owned by DotNet.
internal sealed class Android
{
	internal string SdkDirectory { get; }

	internal Android(string sdkDirectory)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(sdkDirectory);
		SdkDirectory = Path.GetFullPath(sdkDirectory);
	}

	internal Task<(int ExitCode, string Output, string Error)> CreateAvd(string name, string image, string model)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		ArgumentException.ThrowIfNullOrWhiteSpace(image);
		ArgumentException.ThrowIfNullOrWhiteSpace(model);

		// UiAutomator2 can boot an AVD, but cannot create it or install missing images.
		var architecture = RuntimeInformation.ProcessArchitecture switch
		{
			Architecture.Arm64 => "arm64-v8a",
			Architecture.X64 => "x86_64",
			_ => throw new InvalidOperationException("The declared emulator profiles require an arm64 or x64 host.")
		};
		var package = $"system-images;{image};{architecture}";
		var imageDirectory = Path.Combine(SdkDirectory, package.Replace(';', Path.DirectorySeparatorChar));
		if (!File.Exists(Path.Combine(imageDirectory, "package.xml")) || !File.Exists(Path.Combine(imageDirectory, "system.img")))
			throw new DirectoryNotFoundException($"Required emulator image is not installed: {imageDirectory}. Tests never download images.");

		// The installed SDK manager understands minor API targets; the tool wrapper writes android-0.
		var manager = Path.Combine(
			SdkDirectory, "cmdline-tools", "latest", "bin",
			OperatingSystem.IsWindows() ? "avdmanager.bat" : "avdmanager");
		return RunAvdManager(manager, ["create", "avd", "--name", name, "--package", package, "--device", model]);
	}

	private static Task<(int ExitCode, string Output, string Error)> RunAvdManager(string manager, string[] arguments)
	{
		var executable = OperatingSystem.IsWindows() ? "cmd.exe" : manager;
		string[] command = OperatingSystem.IsWindows() ? ["/d", "/c", manager, .. arguments] : arguments;
		return ProcessRunner.Run(executable, command, Repo.RootDir, TimeSpan.FromSeconds(30));
	}

	internal Task<(int ExitCode, string Output, string Error)> DeleteAvd(string name)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		return ExecuteSdk(["avd", "delete", "--name", name, "--force"]);
	}

	private Task<(int ExitCode, string Output, string Error)> ExecuteSdk(string[] arguments)
	{
		return DotNet.ExecuteTool(
			"android",
			[.. arguments, "--home", SdkDirectory],
			new DotNetOptions { Timeout = TimeSpan.FromSeconds(30) });
	}
}
