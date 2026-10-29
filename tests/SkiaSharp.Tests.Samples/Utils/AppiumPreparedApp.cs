namespace SkiaSharp.Tests.Samples.Utils;

internal enum AppiumMode { All, Build, Run }

internal sealed record AppiumAppSpec(
	string Platform, string Architecture, string Folder, string Project, string Framework, string RuntimeIdentifier)
{
	internal string Key => $"{Platform}/{Architecture}/{Folder}";

	internal void Validate()
	{
		if (Platform != "ios" || Architecture is not ("x64" or "arm64") ||
			Framework != "net10.0-ios" || RuntimeIdentifier != $"iossimulator-{Architecture}")
			throw new InvalidOperationException($"Unsupported Appium target: {this}");
		ValidateRelativePath(Folder);
		ValidateRelativePath(Project);
	}

	internal string DirectoryFor(string root)
	{
		Validate();
		return Path.Combine(Path.GetFullPath(root), Key);
	}

	internal string FindApp(SampleWorkspace workspace)
	{
		var output = Path.Combine(workspace.Root, "samples", Folder, Path.GetDirectoryName(Project)!,
			"bin", "Debug", Framework, RuntimeIdentifier);
		var apps = Directory.Exists(output)
			? Directory.GetDirectories(output, "*.app")
			: [];
		if (apps.Length != 1 || !File.Exists(Path.Combine(apps[0], "Info.plist")))
			throw new FileNotFoundException($"Expected one built {Platform} app in {output}. Run build mode first.");
		return apps[0];
	}

	private static void ValidateRelativePath(string value)
	{
		if (string.IsNullOrWhiteSpace(value) || value.Contains('\\') || value.Contains(':') ||
			Path.IsPathRooted(value) ||
			value.Split('/').Any(part => part is "" or "." or ".." || part.Any(char.IsControl)))
			throw new ArgumentException($"Expected a safe sample-relative path: {value}");
	}
}

internal sealed record AppiumPreparedApp(
	SampleWorkspace Workspace, string Artifact, Func<SampleWorkspace, string, AppiumRunningApp> SessionFactory)
{
	internal AppiumRunningApp CreateRunningApp() => SessionFactory(Workspace, Artifact);

	internal static AppiumMode ParseMode(string? value) => value switch
	{
		null or "all" => AppiumMode.All,
		"build" => AppiumMode.Build,
		"run" => AppiumMode.Run,
		_ => throw new ArgumentException($"Invalid TEST_APPIUM_MODE '{value}'; expected all, build or run.")
	};
}
