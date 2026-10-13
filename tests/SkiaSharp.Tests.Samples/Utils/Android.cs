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
