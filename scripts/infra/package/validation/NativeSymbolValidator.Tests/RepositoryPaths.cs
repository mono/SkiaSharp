namespace SkiaSharp.PackageValidation.Tests;

/// <summary>
/// Locates the repository so the wiring tests can assert against the real pipeline definitions
/// rather than a copy that would drift.
/// </summary>
public static class RepositoryPaths
{
	public static string Root { get; } = FindRoot ();

	public static string ReadFile (params string[] relativePath) =>
		File.ReadAllText (Path.Combine (new[] { Root }.Concat (relativePath).ToArray ()));

	private static string FindRoot ()
	{
		var directory = new DirectoryInfo (AppContext.BaseDirectory);

		while (directory is not null) {
			if (File.Exists (Path.Combine (directory.FullName, "build.cake")) &&
				Directory.Exists (Path.Combine (directory.FullName, "scripts")))
				return directory.FullName;

			directory = directory.Parent;
		}

		throw new InvalidOperationException ($"Could not locate the repository root from '{AppContext.BaseDirectory}'.");
	}
}
