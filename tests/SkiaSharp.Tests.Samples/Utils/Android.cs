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

		// Use the same command-line tools selected by SDK provisioning, not an older "latest" directory.
		var sdkManager = Environment.GetEnvironmentVariable("ANDROID_SDK_MANAGER_PATH")
			?? Path.Combine(SdkDirectory, "cmdline-tools", "latest", "bin", "sdkmanager");
		var manager = Path.Combine(
			Path.GetDirectoryName(sdkManager)!,
			OperatingSystem.IsWindows() ? "avdmanager.bat" : "avdmanager");
		return RunAvdManager(manager, ["create", "avd", "--name", name, "--package", package, "--device", model]);
	}

	private static Task<(int ExitCode, string Output, string Error)> RunAvdManager(string manager, string[] arguments)
	{
		var executable = OperatingSystem.IsWindows() ? "cmd.exe" : manager;
		string[] command = OperatingSystem.IsWindows() ? ["/d", "/c", manager, .. arguments] : arguments;
		return ProcessRunner.Run(executable, command, Repo.RootDir, TimeSpan.FromSeconds(30));
	}

	internal async Task CaptureAvdDiagnostics(string name, Func<string, string, Task> save)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		ArgumentNullException.ThrowIfNull(save);
		var adb = Path.Combine(SdkDirectory, "platform-tools", OperatingSystem.IsWindows() ? "adb.exe" : "adb");
		var status = new System.Text.StringBuilder($"Owned AVD: {name}\nThis is a diagnostic snapshot, not a readiness assertion.\n");

		Task<(int ExitCode, string Output, string Error)> Run(string[] arguments, bool publishOutput = true) =>
			ProcessRunner.Run(adb, arguments, Repo.RootDir, TimeSpan.FromSeconds(10), publishOutput: publishOutput);

		static string Describe(string[] arguments, (int ExitCode, string Output, string Error) result) =>
			$"adb {string.Join(' ', arguments)}\nExit: {result.ExitCode}\n{result.Output}\n{result.Error}\n";

		var devices = await Run(["devices", "-l"], publishOutput: false);
		status.AppendLine($"adb devices -l\nExit: {devices.ExitCode}\n{devices.Error}");
		if (devices.ExitCode != 0)
		{
			status.AppendLine("Owned AVD lookup failed. No guest diagnostics were attempted.");
			await save("android-avd.log", status.ToString());
			return;
		}

		List<(string Serial, string State)> matches = [];
		var candidates = 0;
		foreach (var line in devices.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
		{
			var parts = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length < 2 || !parts[0].StartsWith("emulator-", StringComparison.Ordinal) ||
				!int.TryParse(parts[0]["emulator-".Length..], out _))
				continue;
			candidates++;
			// Like the SDK's exact-name StopAvd lookup, probe only emulator identities.
			string[] arguments = ["-s", parts[0], "emu", "avd", "name"];
			var identity = await Run(arguments, publishOutput: false);
			status.AppendLine($"Emulator identity query {candidates}: exit {identity.ExitCode}.\n{identity.Error}");
			if (identity.ExitCode == 0 &&
				identity.Output.Split('\n')[0].Trim().Equals(name, StringComparison.Ordinal))
				matches.Add((parts[0], parts[1]));
		}
		status.AppendLine($"Emulator identities queried: {candidates}.");

		if (matches.Count != 1)
		{
			status.AppendLine($"Expected one exact owned AVD match; found {matches.Count}. No guest diagnostics were attempted.");
			await save("android-avd.log", status.ToString());
			return;
		}

		var (serial, state) = matches[0];
		status.AppendLine($"Exact owned AVD match: {serial}; adb state: {state}.");
		if (state != "device")
		{
			status.AppendLine("Owned AVD is not adb-connected. No guest diagnostics were attempted.");
			await save("android-avd.log", status.ToString());
			return;
		}
		await save("android-avd.log", status.ToString());

		foreach (var (file, command) in new (string, string[])[]
		{
			("android-boot-properties.log", ["shell", "getprop"]),
			("device.log", ["logcat", "-b", "all", "-d", "-v", "threadtime", "-t", "2000"]),
			("android-kernel.log", ["shell", "dmesg"])
		})
		{
			string[] arguments = ["-s", serial, .. command];
			var result = await Run(arguments);
			var text = Describe(arguments, result);
			if (file == "android-boot-properties.log")
			{
				foreach (var property in new[] { "sys.boot_completed", "dev.bootcomplete", "init.svc.bootanim" })
					if (!result.Output.Contains($"[{property}]:", StringComparison.Ordinal))
						text += $"{property}: not reported (unset or property capture failed).\n";
			}
			if (result.ExitCode != 0 || string.IsNullOrWhiteSpace(result.Output))
				text += "Guest diagnostic command failed or returned no output; see exit status and error above.\n";
			await save(file, text);
		}
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
