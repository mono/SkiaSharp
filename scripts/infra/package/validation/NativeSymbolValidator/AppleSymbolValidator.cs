namespace SkiaSharp.PackageValidation;

/// <summary>
/// Validates the Apple dSYM symbol packages: complete bundles, the expected device and simulator
/// architecture coverage, UUIDs that actually match the shipped runtime binary, and the symbol
/// store keys Arcade will index them under.
/// </summary>
public sealed class AppleSymbolValidator
{
	private readonly string workspace;

	public AppleSymbolValidator (string workspace) => this.workspace = workspace;

	public void Validate (ApplePackageSpec spec, NuGetPackage normal, NuGetPackage symbols, ValidationLog log)
	{
		ValidateNoDebugPayloadForCustomers (spec, normal, log);

		foreach (var rid in spec.RuntimeIdentifiers) {
			using var _ = log.BeginScope (rid);
			ValidateRuntimeIdentifier (spec, rid, normal, symbols, log);
		}
	}

	/// <summary>
	/// A customer package must never carry debug information: it is the reason the symbols live in
	/// a separate package at all.
	/// </summary>
	private static void ValidateNoDebugPayloadForCustomers (ApplePackageSpec spec, NuGetPackage normal, ValidationLog log)
	{
		var leaked = normal.Entries
			.Where (e =>
				e.Contains (".dSYM/", StringComparison.OrdinalIgnoreCase) ||
				e.Contains ("/native/symbols/", StringComparison.OrdinalIgnoreCase) ||
				e.Contains ("/DWARF/", StringComparison.OrdinalIgnoreCase))
			.ToArray ();

		log.Check (
			leaked.Length == 0,
			$"package contains {leaked.Length} debug payload file(s) that must only ship in the symbol package: {SymbolPackagePairValidator.Describe (leaked)}.");

		// The Catalyst customer package intentionally ships only the zipped framework.
		if (spec.Kind == ApplePayloadKind.CatalystFramework) {
			var unpacked = normal.EntriesUnder ($"runtimes/{spec.RuntimeIdentifiers[0]}/native/{spec.NativeLibrary}.framework").ToArray ();
			log.Check (
				unpacked.Length == 0,
				$"package unexpectedly contains an unpacked framework: {SymbolPackagePairValidator.Describe (unpacked)}.");
		}
	}

	private void ValidateRuntimeIdentifier (ApplePackageSpec spec, string rid, NuGetPackage normal, NuGetPackage symbols, ValidationLog log)
	{
		if (spec.Kind == ApplePayloadKind.CatalystFramework) {
			var zip = spec.GetFrameworkZipPath (rid)!;
			log.Check (normal.Contains (zip), $"package is missing '{zip}'.");
			log.Check (symbols.Contains (zip), $"symbol package is missing '{zip}'.");
		} else {
			var normalModule = spec.GetModulePathInNormalPackage (rid)!;
			log.Check (normal.Contains (normalModule), $"package is missing the native module '{normalModule}'.");
		}

		var modulePath = spec.GetModulePathInSymbolPackage (rid);
		if (!log.Check (symbols.Contains (modulePath), $"symbol package is missing the native module '{modulePath}'."))
			return;

		var moduleFile = symbols.ExtractTo (modulePath, workspace);
		var moduleName = Path.GetFileName (modulePath);

		var module = NativeBinary.TryReadMachO (moduleFile);
		if (!log.Check (module is not null, $"'{modulePath}' is not a readable Mach-O image."))
			return;

		var missingArches = PackageMatrix.AppleArchitectures
			.Where (a => module!.GetUuid (a) is null)
			.ToArray ();
		log.Check (
			missingArches.Length == 0,
			$"'{modulePath}' is missing expected architecture(s) {string.Join (", ", missingArches)}; it has {string.Join (", ", module!.Architectures)}.");

		ValidateModuleKeys (modulePath, moduleFile, moduleName, module!, log);

		var dsymUuids = new HashSet<string> (StringComparer.OrdinalIgnoreCase);

		foreach (var architecture in PackageMatrix.AppleArchitectures) {
			using var _ = log.BeginScope (architecture);
			var uuid = ValidateDsym (spec, rid, architecture, symbols, module!, log);
			if (uuid is not null)
				dsymUuids.Add (uuid);
		}

		// Every slice the customer can load must be resolvable from the symbol package, and the
		// symbol package must not carry dSYMs for slices that are not shipped.
		var moduleUuids = module!.Uuids;
		var unmatchedRuntime = moduleUuids.Except (dsymUuids, StringComparer.OrdinalIgnoreCase).ToArray ();
		var unmatchedDsym = dsymUuids.Except (moduleUuids, StringComparer.OrdinalIgnoreCase).ToArray ();

		log.Check (
			unmatchedRuntime.Length == 0,
			$"runtime UUID(s) {string.Join (", ", unmatchedRuntime)} have no matching dSYM.");
		log.Check (
			unmatchedDsym.Length == 0,
			$"dSYM UUID(s) {string.Join (", ", unmatchedDsym)} do not correspond to any runtime slice.");
	}

	private string? ValidateDsym (ApplePackageSpec spec, string rid, string architecture, NuGetPackage symbols, MachOImage module, ValidationLog log)
	{
		var root = spec.GetDsymRoot (rid, architecture);
		var infoPlist = $"{root}/Contents/Info.plist";
		var dwarfPath = $"{root}/Contents/Resources/DWARF/{spec.DwarfFileName}";

		var bundleEntries = symbols.EntriesUnder (root).ToArray ();
		if (!log.Check (bundleEntries.Length > 0, $"symbol package is missing the dSYM bundle '{root}'."))
			return null;

		log.Check (symbols.Contains (infoPlist), $"dSYM bundle is missing '{infoPlist}'.");

		// dsymutil emits the DWARF payload under the official extensionless module name for
		// frameworks; anything else means the bundle was assembled by hand and will not resolve.
		if (!log.Check (symbols.Contains (dwarfPath), $"dSYM bundle is missing the DWARF payload '{dwarfPath}'.")) {
			var actual = bundleEntries.Where (e => e.Contains ("/DWARF/", StringComparison.OrdinalIgnoreCase)).ToArray ();
			if (actual.Length > 0)
				log.Info ($"dSYM bundle DWARF payload is named: {SymbolPackagePairValidator.Describe (actual)}");
			return null;
		}

		var dwarfFile = symbols.ExtractTo (dwarfPath, workspace);
		var dsym = NativeBinary.TryReadMachO (dwarfFile);
		if (!log.Check (dsym is not null, $"'{dwarfPath}' is not a readable Mach-O image."))
			return null;

		var slice = dsym!.Slices[0];

		log.Check (
			dsym.Slices.Count == 1,
			$"'{dwarfPath}' contains {dsym.Slices.Count} architecture(s); a per-architecture dSYM must be thin.");

		log.Check (
			string.Equals (slice.FileType, NativeBinary.MachODsymFileType, StringComparison.OrdinalIgnoreCase),
			$"'{dwarfPath}' has Mach-O file type '{slice.FileType}' but a dSYM payload must be '{NativeBinary.MachODsymFileType}'.");

		log.Check (
			string.Equals (slice.Architecture, architecture, StringComparison.OrdinalIgnoreCase),
			$"'{dwarfPath}' is built for '{slice.Architecture}' but is filed under '{architecture}'.");

		var expectedUuid = module.GetUuid (architecture);
		if (expectedUuid is not null) {
			log.Check (
				string.Equals (expectedUuid, slice.Uuid, StringComparison.OrdinalIgnoreCase),
				$"dSYM UUID '{slice.Uuid}' does not match the runtime '{architecture}' slice UUID '{expectedUuid}'.");
		}

		ValidateDsymKeys (dwarfPath, dwarfFile, spec.DwarfFileName, slice.Uuid, log);

		return slice.Uuid;
	}

	/// <summary>
	/// Confirms Arcade will index the runtime module under a <c>mach-uuid</c> key per slice, and
	/// that it will look for its symbols under the matching <c>mach-uuid-sym</c> key.
	/// </summary>
	private static void ValidateModuleKeys (string modulePath, string moduleFile, string moduleName, MachOImage module, ValidationLog log)
	{
		var identityKeys = NativeBinary.GetIdentityKeys (moduleFile, moduleName);
		var symbolKeys = NativeBinary.GetSymbolKeys (moduleFile, moduleName);

		log.Verbose ($"'{modulePath}' identity keys: {string.Join (", ", identityKeys)}");
		log.Verbose ($"'{modulePath}' symbol keys: {string.Join (", ", symbolKeys)}");

		foreach (var uuid in module.Uuids) {
			log.Check (
				identityKeys.Any (k => k.Contains ($"mach-uuid-{uuid}", StringComparison.OrdinalIgnoreCase)),
				$"Microsoft.SymbolStore did not produce a 'mach-uuid-{uuid}' module key for '{modulePath}'; it produced: {SymbolPackagePairValidator.Describe (identityKeys)}.");

			log.Check (
				symbolKeys.Any (k => k.Contains ($"mach-uuid-sym-{uuid}", StringComparison.OrdinalIgnoreCase)),
				$"Microsoft.SymbolStore did not produce a 'mach-uuid-sym-{uuid}' symbol key for '{modulePath}'; it produced: {SymbolPackagePairValidator.Describe (symbolKeys)}.");
		}
	}

	/// <summary>
	/// Confirms the dSYM itself indexes under the very key the runtime module asks for, which is
	/// what makes a debugger able to find it after publication.
	/// </summary>
	private static void ValidateDsymKeys (string dwarfPath, string dwarfFile, string dwarfFileName, string uuid, ValidationLog log)
	{
		var keys = NativeBinary.GetIdentityAndSymbolKeys (dwarfFile, dwarfFileName);

		log.Verbose ($"'{dwarfPath}' keys: {string.Join (", ", keys)}");

		log.Check (
			keys.Any (k => k.Contains ($"mach-uuid-sym-{uuid}", StringComparison.OrdinalIgnoreCase)),
			$"Microsoft.SymbolStore did not produce a 'mach-uuid-sym-{uuid}' key for '{dwarfPath}'; it produced: {SymbolPackagePairValidator.Describe (keys)}.");
	}
}
