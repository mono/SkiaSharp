namespace SkiaSharp.PackageValidation;

public enum ApplePayloadKind
{
	/// <summary>A bare fat <c>.dylib</c> (macOS).</summary>
	Dylib,

	/// <summary>A flat <c>.framework</c> shipped expanded in the customer package (iOS, tvOS).</summary>
	Framework,

	/// <summary>
	/// A versioned <c>.framework</c> shipped to customers only as <c>framework.zip</c>. The symbol
	/// package additionally carries the unpacked Mach-O so the binary can be indexed.
	/// </summary>
	CatalystFramework,
}

public sealed record ApplePackageSpec (
	string Product,
	string Platform,
	IReadOnlyList<string> RuntimeIdentifiers,
	ApplePayloadKind Kind)
{
	public string PackageId => $"{Product}.NativeAssets.{Platform}";

	public string NativeLibrary => PackageMatrix.NativeLibraryFor (Product);

	/// <summary>Path of the Mach-O module inside the customer package.</summary>
	public string? GetModulePathInNormalPackage (string rid) => Kind switch {
		ApplePayloadKind.Dylib => $"runtimes/{rid}/native/{NativeLibrary}.dylib",
		ApplePayloadKind.Framework => $"runtimes/{rid}/native/{NativeLibrary}.framework/{NativeLibrary}",
		// Catalyst ships only the zip to customers; the module lives in the symbol package.
		ApplePayloadKind.CatalystFramework => null,
		_ => null,
	};

	/// <summary>Path of the Mach-O module the symbol package must expose for indexing.</summary>
	public string GetModulePathInSymbolPackage (string rid) => Kind switch {
		ApplePayloadKind.Dylib => $"runtimes/{rid}/native/{NativeLibrary}.dylib",
		ApplePayloadKind.Framework => $"runtimes/{rid}/native/{NativeLibrary}.framework/{NativeLibrary}",
		ApplePayloadKind.CatalystFramework => $"runtimes/{rid}/native/{NativeLibrary}.framework/Versions/A/{NativeLibrary}",
		_ => throw new InvalidOperationException ($"Unknown payload kind '{Kind}'."),
	};

	public string? GetFrameworkZipPath (string rid) =>
		Kind == ApplePayloadKind.CatalystFramework ? $"runtimes/{rid}/native/{NativeLibrary}.framework.zip" : null;

	public string GetDsymRoot (string rid, string architecture) =>
		$"runtimes/{rid}/native/symbols/{architecture}/{DsymBundleName}";

	public string DsymBundleName => Kind switch {
		ApplePayloadKind.Dylib => $"{NativeLibrary}.dylib.dSYM",
		_ => $"{NativeLibrary}.framework.dSYM",
	};

	/// <summary>
	/// The official dSYM DWARF payload name. Framework dSYMs use the extensionless module name;
	/// dylib dSYMs keep the <c>.dylib</c> suffix.
	/// </summary>
	public string DwarfFileName => Kind switch {
		ApplePayloadKind.Dylib => $"{NativeLibrary}.dylib",
		_ => NativeLibrary,
	};
}

public sealed record AndroidPackageSpec (string Product)
{
	public string PackageId => $"{Product}.NativeAssets.Android";

	public string NativeLibrary => PackageMatrix.NativeLibraryFor (Product);

	public string GetStrippedLibraryPath (string rid) =>
		$"runtimes/{rid}/native/{NativeLibrary}.so";

	public string DebugFileName => $"{NativeLibrary}.so.dbg";
}

/// <summary>
/// The expected shape of every native symbol package, derived from the
/// <c>*.NativeAssets.*.csproj</c> packaging declarations.
/// </summary>
public static class PackageMatrix
{
	public static readonly IReadOnlyList<string> Products = new[] { "SkiaSharp", "HarfBuzzSharp" };

	/// <summary>
	/// Every Apple module is lipo'd to exactly these architectures for both device and simulator
	/// runtime identifiers, so a dSYM is expected for each.
	/// </summary>
	public static readonly IReadOnlyList<string> AppleArchitectures = new[] { "arm64", "x86_64" };

	public static readonly IReadOnlyList<string> AndroidRuntimeIdentifiers = new[] {
		"android-arm",
		"android-arm64",
		"android-x86",
		"android-x64",
	};

	public static string NativeLibraryFor (string product) => product switch {
		"SkiaSharp" => "libSkiaSharp",
		"HarfBuzzSharp" => "libHarfBuzzSharp",
		_ => throw new ArgumentOutOfRangeException (nameof (product), product, "Unknown product."),
	};

	public static IReadOnlyList<ApplePackageSpec> AppleSpecs { get; } = BuildAppleSpecs ();

	public static IReadOnlyList<AndroidPackageSpec> AndroidSpecs { get; } =
		Products.Select (p => new AndroidPackageSpec (p)).ToArray ();

	public static IEnumerable<string> AllPackageIds =>
		AppleSpecs.Select (s => s.PackageId).Concat (AndroidSpecs.Select (s => s.PackageId));

	/// <summary>
	/// Native assets packages that produce a symbol package this validator deliberately does not
	/// cover. These ship Windows PDBs, which Arcade indexes through a different key generator and
	/// which carry none of the dSYM/<c>.so.dbg</c> structure asserted here. Listing them explicitly
	/// keeps <see cref="Validator.ValidateNoUnexpectedSymbolPackages"/> able to fail on a genuinely
	/// new native symbol package instead of having to ignore every id it does not recognise.
	/// </summary>
	public static readonly IReadOnlySet<string> UnvalidatedNativeAssetsPackageIds =
		new HashSet<string> (StringComparer.OrdinalIgnoreCase) {
			"SkiaSharp.NativeAssets.Win32",
			"SkiaSharp.NativeAssets.WinUI",
			"SkiaSharp.NativeAssets.NanoServer",
			"HarfBuzzSharp.NativeAssets.Win32",
		};

	private static ApplePackageSpec[] BuildAppleSpecs ()
	{
		var platforms = new (string Platform, string[] Rids, ApplePayloadKind Kind)[] {
			("macOS", new[] { "osx" }, ApplePayloadKind.Dylib),
			("iOS", new[] { "ios", "iossimulator" }, ApplePayloadKind.Framework),
			("tvOS", new[] { "tvos", "tvossimulator" }, ApplePayloadKind.Framework),
			("MacCatalyst", new[] { "maccatalyst" }, ApplePayloadKind.CatalystFramework),
		};

		return Products
			.SelectMany (product => platforms.Select (p => new ApplePackageSpec (product, p.Platform, p.Rids, p.Kind)))
			.ToArray ();
	}
}
