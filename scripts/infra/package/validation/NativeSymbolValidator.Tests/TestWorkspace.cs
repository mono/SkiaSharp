namespace SkiaSharp.PackageValidation.Tests;

/// <summary>
/// A scratch directory for extracting package payloads, mirroring what the validator does in CI.
/// </summary>
public sealed class TestWorkspace : IDisposable
{
	public TestWorkspace ()
	{
		Path = System.IO.Path.Combine (System.IO.Path.GetTempPath (), "nsv-tests", Guid.NewGuid ().ToString ("N"));
		Directory.CreateDirectory (Path);
		Packages = System.IO.Path.Combine (Path, "nugets");
		SymbolPackages = System.IO.Path.Combine (Path, "nugets-symbols");
		Extract = System.IO.Path.Combine (Path, "extract");
		Directory.CreateDirectory (Packages);
		Directory.CreateDirectory (SymbolPackages);
		Directory.CreateDirectory (Extract);
	}

	public string Path { get; }

	public string Packages { get; }

	public string SymbolPackages { get; }

	public string Extract { get; }

	public string WriteVersionsFile (params (string Id, string Version)[] versions)
	{
		var file = System.IO.Path.Combine (Path, "VERSIONS.txt");
		var lines = new List<string> {
			"# Test versions file.",
			"",
		};
		lines.AddRange (versions.Select (v => $"{v.Id}                nuget    {v.Version}"));
		File.WriteAllLines (file, lines);
		return file;
	}

	public void Dispose ()
	{
		try {
			Directory.Delete (Path, recursive: true);
		} catch (IOException) {
			// A leaked scratch directory must never fail a test run.
		} catch (UnauthorizedAccessException) {
		}
	}
}
