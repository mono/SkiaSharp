namespace SkiaSharp.PackageValidation;

/// <summary>
/// Validates the Android <c>.so</c>/<c>.so.dbg</c> symbol packages: customers get stripped
/// binaries, the symbol package carries the matching unstripped debug files, and the GNU build IDs
/// tie the two together.
/// </summary>
public sealed class AndroidSymbolValidator
{
	private readonly string workspace;

	public AndroidSymbolValidator (string workspace) => this.workspace = workspace;

	public void Validate (AndroidPackageSpec spec, NuGetPackage normal, NuGetPackage symbols, ValidationLog log)
	{
		var leaked = normal.Entries
			.Where (e => e.EndsWith (".dbg", StringComparison.OrdinalIgnoreCase))
			.ToArray ();
		log.Check (
			leaked.Length == 0,
			$"package contains {leaked.Length} debug file(s) that must only ship in the symbol package: {SymbolPackagePairValidator.Describe (leaked)}.");

		foreach (var rid in PackageMatrix.AndroidRuntimeIdentifiers) {
			using var _ = log.BeginScope (rid);
			ValidateRuntimeIdentifier (spec, rid, normal, symbols, log);
		}
	}

	private void ValidateRuntimeIdentifier (AndroidPackageSpec spec, string rid, NuGetPackage normal, NuGetPackage symbols, ValidationLog log)
	{
		var libraryPath = spec.GetStrippedLibraryPath (rid);

		if (!log.Check (normal.Contains (libraryPath), $"package is missing the native library '{libraryPath}'."))
			return;

		if (!log.Check (symbols.Contains (libraryPath), $"symbol package is missing the native library '{libraryPath}'."))
			return;

		using var libraryFile = normal.ExtractScoped (libraryPath, workspace);
		var libraryBuildId = NativeBinary.TryReadElfBuildId (libraryFile.Path);

		if (!log.Check (libraryBuildId is not null, $"'{libraryPath}' is not a readable ELF image with a GNU build ID."))
			return;

		// The debug file is placed by NuGet's legacy symbol packaging, which does not guarantee the
		// exact folder layout of the customer package, so it is located by name plus runtime
		// identifier rather than by an assumed path.
		var debugPath = FindDebugFile (symbols, spec, rid);
		if (!log.Check (debugPath is not null, $"symbol package is missing '{spec.DebugFileName}' for '{rid}'."))
			return;

		using var debugFile = symbols.ExtractScoped (debugPath!, workspace);
		var debugBuildId = NativeBinary.TryReadElfBuildId (debugFile.Path);

		if (!log.Check (debugBuildId is not null, $"'{debugPath}' is not a readable ELF image with a GNU build ID."))
			return;

		log.Check (
			string.Equals (libraryBuildId, debugBuildId, StringComparison.OrdinalIgnoreCase),
			$"debug file build ID '{debugBuildId}' does not match the shipped library build ID '{libraryBuildId}'.");

		// A stripped library is meaningfully smaller than its debug companion; if they are the same
		// size the strip step silently did nothing and customers are shipping debug information.
		var strippedLength = normal.GetLength (libraryPath);
		var debugLength = symbols.GetLength (debugPath!);
		log.Check (
			strippedLength < debugLength,
			$"'{libraryPath}' ({strippedLength} bytes) is not smaller than '{debugPath}' ({debugLength} bytes), so it does not look stripped.");

		ValidateKeys (libraryPath, libraryFile.Path, debugPath!, debugFile.Path, libraryBuildId!, log);
	}

	private static string? FindDebugFile (NuGetPackage symbols, AndroidPackageSpec spec, string rid) =>
		symbols.Entries.FirstOrDefault (e =>
			e.EndsWith ($"/{spec.DebugFileName}", StringComparison.OrdinalIgnoreCase) &&
			e.Contains ($"/{rid}/", StringComparison.OrdinalIgnoreCase));

	/// <summary>
	/// Confirms Arcade will index the shipped library under an <c>elf-buildid</c> key and look for
	/// its symbols under the matching <c>elf-buildid-sym</c> key, and that the debug file carries
	/// the same build ID so it lands where the lookup expects.
	/// </summary>
	private static void ValidateKeys (string libraryPath, string libraryFile, string debugPath, string debugFile, string buildId, ValidationLog log)
	{
		var identityKeys = NativeBinary.GetIdentityKeys (libraryFile, Path.GetFileName (libraryPath));
		var symbolKeys = NativeBinary.GetSymbolKeys (libraryFile, Path.GetFileName (libraryPath));
		var debugKeys = NativeBinary.GetIdentityKeys (debugFile, Path.GetFileName (debugPath));

		log.Verbose ($"'{libraryPath}' identity keys: {string.Join (", ", identityKeys)}");
		log.Verbose ($"'{libraryPath}' symbol keys: {string.Join (", ", symbolKeys)}");
		log.Verbose ($"'{debugPath}' identity keys: {string.Join (", ", debugKeys)}");

		log.Check (
			identityKeys.Any (k => k.Contains ($"elf-buildid-{buildId}", StringComparison.OrdinalIgnoreCase)),
			$"Microsoft.SymbolStore did not produce an 'elf-buildid-{buildId}' module key for '{libraryPath}'; it produced: {SymbolPackagePairValidator.Describe (identityKeys)}.");

		log.Check (
			symbolKeys.Any (k => k.Contains ($"elf-buildid-sym-{buildId}", StringComparison.OrdinalIgnoreCase)),
			$"Microsoft.SymbolStore did not produce an 'elf-buildid-sym-{buildId}' symbol key for '{libraryPath}'; it produced: {SymbolPackagePairValidator.Describe (symbolKeys)}.");

		// The debug file must at least index under the same build ID. Its exact key name depends on
		// how the file was produced, so only the build ID is asserted.
		log.Check (
			debugKeys.Any (k => k.Contains (buildId, StringComparison.OrdinalIgnoreCase)),
			$"Microsoft.SymbolStore did not produce a key containing build ID '{buildId}' for '{debugPath}'; it produced: {SymbolPackagePairValidator.Describe (debugKeys)}.");
	}
}
