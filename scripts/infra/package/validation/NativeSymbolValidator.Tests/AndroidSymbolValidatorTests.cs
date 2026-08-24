using Xunit;

namespace SkiaSharp.PackageValidation.Tests;

public class AndroidSymbolValidatorTests
{
	public static TheoryData<string> AllAndroidPackages ()
	{
		var data = new TheoryData<string> ();
		foreach (var spec in PackageMatrix.AndroidSpecs)
			data.Add (spec.Product);
		return data;
	}

	[Theory]
	[MemberData (nameof (AllAndroidPackages))]
	public void CorrectPackagePairPassesForEveryProduct (string product)
	{
		using var workspace = new TestWorkspace ();
		var fixture = new AndroidFixture (product);

		Assert.Empty (fixture.Validate (workspace));
	}

	[Fact]
	public void DebugFileLeakedIntoTheCustomerPackageFails ()
	{
		using var workspace = new TestWorkspace ();
		var fixture = new AndroidFixture ("SkiaSharp");
		var rid = PackageMatrix.AndroidRuntimeIdentifiers[0];

		fixture.Normal.Add ($"runtimes/{rid}/native/{fixture.Spec.DebugFileName}", BinaryFixtures.Elf (fixture.BuildId (rid), 8192));

		Assert.Contains (fixture.Validate (workspace), e => e.Contains ("must only ship in the symbol package", StringComparison.Ordinal));
	}

	[Fact]
	public void MissingDebugFileForAnAbiFails ()
	{
		using var workspace = new TestWorkspace ();
		var fixture = new AndroidFixture ("HarfBuzzSharp");
		var rid = PackageMatrix.AndroidRuntimeIdentifiers[2];

		fixture.Symbols.Remove ($"runtimes/{rid}/native/{fixture.Spec.DebugFileName}");

		Assert.Contains (fixture.Validate (workspace), e => e.Contains ($"missing '{fixture.Spec.DebugFileName}' for '{rid}'", StringComparison.Ordinal));
	}

	[Fact]
	public void BuildIdMismatchFails ()
	{
		using var workspace = new TestWorkspace ();
		var fixture = new AndroidFixture ("SkiaSharp");
		var rid = PackageMatrix.AndroidRuntimeIdentifiers[1];

		// A debug file from a different build of the same library: the debugger would silently
		// refuse it, so the package must not ship that way.
		fixture.Symbols.Add ($"runtimes/{rid}/native/{fixture.Spec.DebugFileName}", BinaryFixtures.Elf (BinaryFixtures.NewBuildId (), 8192));

		Assert.Contains (fixture.Validate (workspace), e => e.Contains ("does not match the shipped library build ID", StringComparison.Ordinal));
	}

	[Fact]
	public void UnstrippedLibraryFails ()
	{
		using var workspace = new TestWorkspace ();
		var fixture = new AndroidFixture ("SkiaSharp");
		var rid = PackageMatrix.AndroidRuntimeIdentifiers[3];
		var buildId = fixture.BuildId (rid);

		// Same size as the debug companion means the strip step did nothing.
		fixture.Normal.Add (fixture.Spec.GetStrippedLibraryPath (rid), BinaryFixtures.Elf (buildId, 8192));
		fixture.Symbols.Add (fixture.Spec.GetStrippedLibraryPath (rid), BinaryFixtures.Elf (buildId, 8192));

		Assert.Contains (fixture.Validate (workspace), e => e.Contains ("does not look stripped", StringComparison.Ordinal));
	}

	[Fact]
	public void MissingAbiFails ()
	{
		using var workspace = new TestWorkspace ();
		var fixture = new AndroidFixture ("SkiaSharp");
		var rid = PackageMatrix.AndroidRuntimeIdentifiers[2];

		fixture.Normal.Remove (fixture.Spec.GetStrippedLibraryPath (rid));

		Assert.Contains (fixture.Validate (workspace), e => e.Contains ("missing the native library", StringComparison.Ordinal));
	}

	[Fact]
	public void CorruptDebugFileFails ()
	{
		using var workspace = new TestWorkspace ();
		var fixture = new AndroidFixture ("SkiaSharp");
		var rid = PackageMatrix.AndroidRuntimeIdentifiers[0];

		fixture.Symbols.Add ($"runtimes/{rid}/native/{fixture.Spec.DebugFileName}", new byte[8192]);

		Assert.Contains (fixture.Validate (workspace), e => e.Contains ("not a readable ELF image", StringComparison.Ordinal));
	}

	[Fact]
	public void LibraryWithoutAGnuBuildIdFails ()
	{
		using var workspace = new TestWorkspace ();
		var fixture = new AndroidFixture ("HarfBuzzSharp");
		var rid = PackageMatrix.AndroidRuntimeIdentifiers[1];

		// Truncating the note leaves a valid ELF header with no build ID to match against.
		var stripped = BinaryFixtures.Elf (fixture.BuildId (rid));
		Array.Clear (stripped, 64, 56);
		fixture.Normal.Add (fixture.Spec.GetStrippedLibraryPath (rid), stripped);

		Assert.Contains (fixture.Validate (workspace), e => e.Contains ("GNU build ID", StringComparison.Ordinal));
	}
}

/// <summary>
/// Assembles a correct Android package pair so each test only has to describe what it breaks.
/// </summary>
public sealed class AndroidFixture
{
	private readonly Dictionary<string, string> buildIds = new ();

	public AndroidFixture (string product, string version = "3.999.0")
	{
		Spec = PackageMatrix.AndroidSpecs.Single (s => s.Product == product);
		Normal = new PackageBuilder (Spec.PackageId, version);
		Symbols = new PackageBuilder (Spec.PackageId, version);

		foreach (var rid in PackageMatrix.AndroidRuntimeIdentifiers) {
			var buildId = BinaryFixtures.NewBuildId ();
			buildIds[rid] = buildId;

			var stripped = BinaryFixtures.Elf (buildId, 1024);
			Normal.Add (Spec.GetStrippedLibraryPath (rid), stripped);
			Symbols.Add (Spec.GetStrippedLibraryPath (rid), stripped);
			Symbols.Add ($"runtimes/{rid}/native/{Spec.DebugFileName}", BinaryFixtures.Elf (buildId, 8192));
		}
	}

	public AndroidPackageSpec Spec { get; }

	public PackageBuilder Normal { get; }

	public PackageBuilder Symbols { get; }

	public string BuildId (string rid) => buildIds[rid];

	public IReadOnlyList<string> Validate (TestWorkspace workspace)
	{
		var normalPath = Normal.Save (workspace.Packages);
		var symbolsPath = Symbols.Save (workspace.SymbolPackages, $"{Symbols.Id}.{Symbols.Version}.symbols.nupkg");

		using var normal = NuGetPackage.Open (normalPath);
		using var symbols = NuGetPackage.Open (symbolsPath);

		var log = new ValidationLog (TextWriter.Null);
		new AndroidSymbolValidator (workspace.Extract).Validate (Spec, normal, symbols, log);
		return log.Errors;
	}
}
