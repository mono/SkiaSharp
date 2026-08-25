using System.Text.RegularExpressions;

namespace SkiaSharp.PackageValidation;

public sealed class ValidatorOptions
{
	public required string PackagesDirectory { get; init; }

	public required string SymbolPackagesDirectory { get; init; }

	public required string VersionsFile { get; init; }

	public string? PreviewSuffix { get; init; }

	/// <summary>
	/// When set, a package that is expected but not present is a failure. Platforms that cannot
	/// produce the full matrix locally run without it so the validator still checks what exists.
	/// </summary>
	public bool RequireAll { get; init; }

	public bool Verbose { get; init; }
}

public sealed record ValidationResult (int Validated, int Skipped, IReadOnlyList<string> Errors, IReadOnlyList<string> Warnings)
{
	public bool Succeeded => Errors.Count == 0;
}

/// <summary>
/// Validates the native symbol packages produced by <c>pack</c> against the shape declared by the
/// <c>*.NativeAssets.*</c> projects.
/// </summary>
public sealed class Validator
{
	private readonly ValidatorOptions options;
	private readonly TextWriter output;

	public Validator (ValidatorOptions options, TextWriter output)
	{
		this.options = options;
		this.output = output;
	}

	public ValidationResult Run ()
	{
		var log = new ValidationLog (output, options.Verbose);
		var versions = ReadVersions (options.VersionsFile);

		var workspace = Path.Combine (Path.GetTempPath (), "skiasharp-symbol-validation", Guid.NewGuid ().ToString ("n"));
		Directory.CreateDirectory (workspace);

		var validated = 0;
		var skipped = 0;

		try {
			foreach (var (packageId, version, channel) in EnumerateExpectedPackages (versions)) {
				using var _ = log.BeginScope ($"{packageId} {version}");

				var normalPath = Path.Combine (options.PackagesDirectory, $"{packageId}.{version}.nupkg");
				var symbolsPath = Path.Combine (options.SymbolPackagesDirectory, $"{packageId}.{version}.symbols.nupkg");

				var hasNormal = File.Exists (normalPath);
				var hasSymbols = File.Exists (symbolsPath);

				if (!hasNormal && !hasSymbols) {
					if (options.RequireAll) {
						log.Error ($"neither the package nor the symbol package was produced ({channel}).");
					} else {
						log.Info ($"skipping {packageId} {version}: not produced on this platform.");
						skipped++;
					}
					continue;
				}

				if (!log.Check (hasNormal, $"the package '{Path.GetFileName (normalPath)}' was not produced but its symbol package was."))
					continue;
				if (!log.Check (hasSymbols, $"the symbol package '{Path.GetFileName (symbolsPath)}' was not produced."))
					continue;

				log.Info ($"validating {packageId} {version} ({channel})");

				using var normal = TryOpen (normalPath, log);
				if (normal is null)
					continue;

				using var symbols = TryOpen (symbolsPath, log);
				if (symbols is null)
					continue;

				SymbolPackagePairValidator.Validate (normal, symbols, log);

				var apple = PackageMatrix.AppleSpecs.FirstOrDefault (s => IdEquals (s.PackageId, packageId));
				if (apple is not null)
					new AppleSymbolValidator (workspace).Validate (apple, normal, symbols, log);

				var android = PackageMatrix.AndroidSpecs.FirstOrDefault (s => IdEquals (s.PackageId, packageId));
				if (android is not null)
					new AndroidSymbolValidator (workspace).Validate (android, normal, symbols, log);

				validated++;
			}

			ValidateNoUnexpectedSymbolPackages (log);
		} finally {
			TryDelete (workspace);
		}

		return new ValidationResult (validated, skipped, log.Errors, log.Warnings);
	}

	private IEnumerable<(string PackageId, string Version, string Channel)> EnumerateExpectedPackages (IReadOnlyDictionary<string, string> versions)
	{
		foreach (var packageId in PackageMatrix.AllPackageIds) {
			if (!versions.TryGetValue (packageId, out var stable))
				throw new InvalidOperationException ($"'{packageId}' has no 'nuget' version in '{options.VersionsFile}'.");

			yield return (packageId, stable, "stable");

			if (!string.IsNullOrEmpty (options.PreviewSuffix))
				yield return (packageId, $"{stable}-{options.PreviewSuffix}", "preview");
		}
	}

	/// <summary>
	/// A symbol package for a native assets project that the matrix does not know about means a new
	/// package started producing symbols without validation being extended to cover it.
	/// </summary>
	/// <remarks>
	/// The package id is read from the nuspec rather than inferred from the file name. Inferring it
	/// by longest/first matching prefix is unreliable because <c>VERSIONS.txt</c> also lists the
	/// bare product ids (<c>SkiaSharp</c>, <c>HarfBuzzSharp</c>), which are prefixes of every native
	/// assets id and would shadow them.
	/// </remarks>
	private void ValidateNoUnexpectedSymbolPackages (ValidationLog log)
	{
		if (!Directory.Exists (options.SymbolPackagesDirectory))
			return;

		var known = PackageMatrix.AllPackageIds.ToHashSet (StringComparer.OrdinalIgnoreCase);

		foreach (var file in Directory.EnumerateFiles (options.SymbolPackagesDirectory, "*.symbols.nupkg")) {
			var name = Path.GetFileName (file);

			using var package = TryOpen (file, log);
			if (package is null)
				continue;

			var packageId = package.Id;

			if (!packageId.Contains (".NativeAssets.", StringComparison.OrdinalIgnoreCase))
				continue;
			if (known.Contains (packageId))
				continue;
			if (PackageMatrix.UnvalidatedNativeAssetsPackageIds.Contains (packageId))
				continue;

			log.Error ($"'{name}' is a native assets symbol package ('{packageId}') that this validator does not know how to validate; extend PackageMatrix.AppleSpecs/AndroidSpecs, or add it to PackageMatrix.UnvalidatedNativeAssetsPackageIds if it is deliberately out of scope.");
		}
	}

	private static NuGetPackage? TryOpen (string path, ValidationLog log)
	{
		try {
			return NuGetPackage.Open (path);
		} catch (InvalidDataException ex) {
			log.Error ($"'{Path.GetFileName (path)}' could not be read as a NuGet package: {ex.Message}");
			return null;
		}
	}

	public static IReadOnlyDictionary<string, string> ReadVersions (string versionsFile)
	{
		var versions = new Dictionary<string, string> (StringComparer.OrdinalIgnoreCase);
		var pattern = new Regex (@"^\s*(?<id>[^\s#][^\s]*)\s+nuget\s+(?<version>[^\s]+)\s*$", RegexOptions.IgnoreCase);

		foreach (var line in File.ReadLines (versionsFile)) {
			var match = pattern.Match (line);
			if (match.Success)
				versions[match.Groups["id"].Value] = match.Groups["version"].Value;
		}

		return versions;
	}

	private static bool IdEquals (string left, string right) =>
		string.Equals (left, right, StringComparison.OrdinalIgnoreCase);

	private static void TryDelete (string directory)
	{
		try {
			if (Directory.Exists (directory))
				Directory.Delete (directory, recursive: true);
		} catch (IOException) {
			// A leftover temp directory is not worth failing validation over.
		} catch (UnauthorizedAccessException) {
			// A leftover temp directory is not worth failing validation over.
		}
	}
}
