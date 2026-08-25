using Xunit;

namespace SkiaSharp.PackageValidation.Tests;

public class ValidatorTests
{
	[Fact]
	public void MissingPackagesAreSkippedWhenNotRequiringAll ()
	{
		using var workspace = new TestWorkspace ();

		var result = Run (workspace, requireAll: false);

		Assert.True (result.Succeeded);
		Assert.Equal (0, result.Validated);
		Assert.Equal (PackageMatrix.AllPackageIds.Count (), result.Skipped);
	}

	[Fact]
	public void MissingPackagesFailWhenRequiringAll ()
	{
		using var workspace = new TestWorkspace ();

		var result = Run (workspace, requireAll: true);

		Assert.False (result.Succeeded);
		Assert.Equal (PackageMatrix.AllPackageIds.Count (), result.Errors.Count);
	}

	[Fact]
	public void PreviewSuffixDoublesTheExpectedMatrix ()
	{
		using var workspace = new TestWorkspace ();

		var result = Run (workspace, requireAll: false, previewSuffix: "preview.1.2");

		Assert.Equal (PackageMatrix.AllPackageIds.Count () * 2, result.Skipped);
	}

	[Fact]
	public void PackageWithoutItsSymbolPackageFails ()
	{
		using var workspace = new TestWorkspace ();
		new PackageBuilder ("SkiaSharp.NativeAssets.macOS", "3.999.0")
			.Add ("runtimes/osx/native/libSkiaSharp.dylib", BinaryFixtures.MachO (BinaryFixtures.CpuTypeArm64, BinaryFixtures.NewUuid ()))
			.Save (workspace.Packages);

		var result = Run (workspace, requireAll: false);

		Assert.False (result.Succeeded);
		Assert.Contains (result.Errors, e => e.Contains ("symbol package", StringComparison.Ordinal) && e.Contains ("was not produced", StringComparison.Ordinal));
	}

	[Fact]
	public void UnknownNativeAssetsSymbolPackageFails ()
	{
		using var workspace = new TestWorkspace ();
		new PackageBuilder ("SkiaSharp.NativeAssets.Fictional", "3.999.0")
			.Add ("runtimes/fictional/native/libSkiaSharp.so", BinaryFixtures.Elf (BinaryFixtures.NewBuildId ()))
			.Save (workspace.SymbolPackages, "SkiaSharp.NativeAssets.Fictional.3.999.0.symbols.nupkg");

		var versionsFile = WriteVersions (workspace, ("SkiaSharp.NativeAssets.Fictional", "3.999.0"));

		var result = new Validator (
			new ValidatorOptions {
				PackagesDirectory = workspace.Packages,
				SymbolPackagesDirectory = workspace.SymbolPackages,
				VersionsFile = versionsFile,
				RequireAll = false,
			},
			TextWriter.Null).Run ();

		Assert.Contains (result.Errors, e => e.Contains ("extend PackageMatrix", StringComparison.Ordinal));
	}

	[Fact]
	public void UnknownSymbolPackageIsDetectedEvenWhenAShorterProductIdSharesItsPrefix ()
	{
		using var workspace = new TestWorkspace ();
		new PackageBuilder ("SkiaSharp.NativeAssets.Fictional", "3.999.0")
			.Add ("runtimes/fictional/native/libSkiaSharp.so", BinaryFixtures.Elf (BinaryFixtures.NewBuildId ()))
			.Save (workspace.SymbolPackages, "SkiaSharp.NativeAssets.Fictional.3.999.0.symbols.nupkg");

		// scripts/VERSIONS.txt lists the bare 'SkiaSharp' product before every 'SkiaSharp.NativeAssets.*'
		// entry, so resolving the owning package by matching the file name against version keys picks
		// 'SkiaSharp' and the guard never fires. The identity must come from the nuspec instead.
		var versionsFile = workspace.WriteVersionsFile (
			new (string Id, string Version)[] { ("SkiaSharp", "3.999.0"), ("HarfBuzzSharp", "3.999.0") }
				.Concat (PackageMatrix.AllPackageIds.Select (id => (Id: id, Version: "3.999.0")))
				.Append ((Id: "SkiaSharp.NativeAssets.Fictional", Version: "3.999.0"))
				.ToArray ());

		var result = new Validator (
			new ValidatorOptions {
				PackagesDirectory = workspace.Packages,
				SymbolPackagesDirectory = workspace.SymbolPackages,
				VersionsFile = versionsFile,
				RequireAll = false,
			},
			TextWriter.Null).Run ();

		Assert.Contains (result.Errors, e =>
			e.Contains ("SkiaSharp.NativeAssets.Fictional", StringComparison.Ordinal) &&
			e.Contains ("extend PackageMatrix", StringComparison.Ordinal));
	}

	[Fact]
	public void WindowsNativeAssetsSymbolPackagesAreNotReportedAsUnknown ()
	{
		using var workspace = new TestWorkspace ();

		// These ship Windows PDBs from the same IncludeSymbols switch and are deliberately outside the
		// matrix, so they must not be mistaken for a packaging regression.
		foreach (var packageId in PackageMatrix.UnvalidatedNativeAssetsPackageIds)
			new PackageBuilder (packageId, "3.999.0")
				.Add ("runtimes/win-x64/native/libSkiaSharp.dll", "windows payload")
				.Save (workspace.SymbolPackages, $"{packageId}.3.999.0.symbols.nupkg");

		var result = Run (workspace, requireAll: false);

		Assert.Empty (result.Errors);
	}

	[Fact]
	public void MissingVersionForAnExpectedPackageThrows ()
	{
		using var workspace = new TestWorkspace ();
		var versionsFile = workspace.WriteVersionsFile (("SkiaSharp", "3.999.0"));

		var validator = new Validator (
			new ValidatorOptions {
				PackagesDirectory = workspace.Packages,
				SymbolPackagesDirectory = workspace.SymbolPackages,
				VersionsFile = versionsFile,
			},
			TextWriter.Null);

		Assert.Throws<InvalidOperationException> (() => validator.Run ());
	}

	[Fact]
	public void EveryExpectedPackageHasAVersionInTheRepositoryVersionsFile ()
	{
		var versions = Validator.ReadVersions (Path.Combine (RepositoryPaths.Root, "scripts", "VERSIONS.txt"));

		foreach (var packageId in PackageMatrix.AllPackageIds)
			Assert.True (versions.ContainsKey (packageId), $"'{packageId}' has no 'nuget' version in scripts/VERSIONS.txt.");
	}

	private static ValidationResult Run (TestWorkspace workspace, bool requireAll, string? previewSuffix = null) =>
		new Validator (
			new ValidatorOptions {
				PackagesDirectory = workspace.Packages,
				SymbolPackagesDirectory = workspace.SymbolPackages,
				VersionsFile = WriteVersions (workspace),
				PreviewSuffix = previewSuffix,
				RequireAll = requireAll,
			},
			TextWriter.Null).Run ();

	private static string WriteVersions (TestWorkspace workspace, params (string Id, string Version)[] extra) =>
		workspace.WriteVersionsFile (
			PackageMatrix.AllPackageIds
				.Select (id => (id, "3.999.0"))
				.Concat (extra)
				.ToArray ());
}
