using System.Runtime.InteropServices;

namespace SkiaSharp.Tests.Samples.Utils;

// The caller selects the installed SDK; repository tool resolution remains owned by DotNet.
internal sealed class Android
{
	private readonly string adb;
	internal string SdkDirectory { get; }

	internal Android(string sdkDirectory)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(sdkDirectory);
		SdkDirectory = Path.GetFullPath(sdkDirectory);

		adb = Path.Combine(SdkDirectory, "platform-tools", OperatingSystem.IsWindows() ? "adb.exe" : "adb");
		if (!File.Exists(adb))
			throw new FileNotFoundException(
				"Install Android platform-tools before selecting Android tests.", adb);
	}

	internal Task<ToolCommandResult> ListAvds() =>
		ExecuteSdk(["avd", "list", "--format", "json"]);

	internal Task<ToolCommandResult> ListDevices() =>
		ExecuteSdk(["device", "list", "--format", "json"]);

	internal Task<ToolCommandResult> CreateAvd(string name, string imagePrefix, string model)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		ArgumentException.ThrowIfNullOrWhiteSpace(imagePrefix);
		ArgumentException.ThrowIfNullOrWhiteSpace(model);

		var architecture = RuntimeInformation.ProcessArchitecture switch
		{
			Architecture.Arm64 => "arm64-v8a",
			Architecture.X64 => "x86_64",
			_ => throw new InvalidOperationException("The declared emulator profiles require an arm64 or x64 host.")
		};
		// UiAutomator2 can boot an AVD, but cannot create it or install missing images.
		var image = $"{imagePrefix};{architecture}";
		var imageDirectory = Path.Combine(SdkDirectory, image.Replace(';', Path.DirectorySeparatorChar));
		if (!File.Exists(Path.Combine(imageDirectory, "package.xml")) ||
			!File.Exists(Path.Combine(imageDirectory, "system.img")))
			throw new DirectoryNotFoundException(
				$"Required emulator image is not installed: {imageDirectory}. Tests never download images.");

		return ExecuteSdk(["avd", "create", "--name", name, "--sdk", image, "--device", model]);
	}

	internal Task<ToolCommandResult> DeleteAvd(string name)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		return ExecuteSdk(["avd", "delete", "--name", name, "--force"]);
	}

	internal Task<ToolCommandResult> AvdName(string serial) =>
		Adb(serial, ["emu", "avd", "name"]);

	internal Task<ToolCommandResult> PageSize(string serial) =>
		Adb(serial, ["shell", "getconf", "PAGE_SIZE"]);

	private Task<ToolCommandResult> Adb(string serial, string[] arguments)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(serial);
		return RunAdb(["-s", serial, .. arguments]);
	}

	private async Task<ToolCommandResult> RunAdb(string[] arguments)
	{
		var result = await ProcessRunner.Run(adb, arguments, Repo.RootDir, TimeSpan.FromSeconds(30));
		return new($"{adb} {string.Join(" ", arguments)}", result.ExitCode, result.Output, result.Error);
	}

	private async Task<ToolCommandResult> ExecuteSdk(string[] arguments)
	{
		string[] command = [.. arguments, "--home", SdkDirectory];
		var result = await DotNet.ExecuteTool(
			"android",
			command,
			new DotNetOptions { Timeout = TimeSpan.FromSeconds(30) });

		return new(
			$"dotnet tool run android -- {string.Join(" ", command)}",
			result.ExitCode, result.Output, result.Error);
	}
}
