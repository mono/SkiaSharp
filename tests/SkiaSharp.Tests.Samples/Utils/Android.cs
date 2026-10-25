namespace SkiaSharp.Tests.Samples.Utils;

internal static class Android
{
	internal static async Task<(int ExitCode, string Output, string Error)> DeviceProperty(string serial, string property)
	{
		var result = await Run(["device", "info", "--id", ExactSerial(serial),
			"--property", "^" + System.Text.RegularExpressions.Regex.Escape(property) + "$", "--format", "json"]);
		if (result.ExitCode != 0)
			return result;
		return (result.ExitCode, ParseDeviceProperty(result.Output, serial, property), result.Error);
	}

	internal static string ParseDeviceProperty(string output, string serial, string property)
	{
		using var json = System.Text.Json.JsonDocument.Parse(output);
		var device = json.RootElement.EnumerateArray().Single();
		if (device.GetProperty("device").GetProperty("serial").GetString() != serial)
			throw new InvalidOperationException($"Android CLI returned a different device than {serial}.");
		return device.GetProperty("properties").GetProperty(property).GetString()
			?? throw new InvalidOperationException($"Device property {property} is missing on {serial}.");
	}

	internal static Task<(int ExitCode, string Output, string Error)> ReadManifest(string apk) =>
		Run(["apk", "manifest", "info", "--pkg", apk, "--format", "xml"]);

	internal static Task<(int ExitCode, string Output, string Error)> Logcat(string serial) =>
		Run(["device", "logcat", "--id", ExactSerial(serial)], publishOutput: false);

	internal static Task<(int ExitCode, string Output, string Error)> DeviceInfo(string serial) =>
		Run(["device", "info", "--id", ExactSerial(serial), "--format", "json"], publishOutput: false);

	private static string ExactSerial(string serial) =>
		"^" + System.Text.RegularExpressions.Regex.Escape(serial) + "$";

	private static Task<(int ExitCode, string Output, string Error)> Run(string[] arguments, bool publishOutput = true)
	{
		var sdk = Environment.GetEnvironmentVariable("ANDROID_HOME");
		if (arguments[0] != "apk" && !string.IsNullOrWhiteSpace(sdk))
			arguments = [.. arguments, "--home", sdk];
		return ProcessRunner.Run("dotnet", ["android", .. arguments], Repo.RootDir, TimeSpan.FromSeconds(30),
			publishOutput: publishOutput);
	}
}
