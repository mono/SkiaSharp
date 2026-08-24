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

				using var normal = NuGetPackage.Open (normalPath);
				using var symbols = NuGetPackage.Open (symbolsPath);

				SymbolPackagePairValidator.Validate (normal, symbols, log);

				var apple = PackageMatrix.AppleSpecs.FirstOrDefault (s => IdEquals (s.PackageId, packageId));
				if (apple is not null)
					new AppleSymbolValidator (workspace).Validate (apple, normal, symbols, log);

				var android = PackageMatrix.AndroidSpecs.FirstOrDefault (s => IdEquals (s.PackageId, packageId));
				if (android is not null)
					new AndroidSymbolValidator (workspace).Validate (android, normal, symbols, log);

				validated++;
			}

			ValidateNoUnexpectedSymbolPackages (versions, log);
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
	private void ValidateNoUnexpectedSymbolPackages (IReadOnlyDictionary<string, string> versions, ValidationLog log)
	{
		if (!Directory.Exists (options.SymbolPackagesDirectory))
			return;

		var known = PackageMatrix.AllPackageIds.ToHashSet (StringComparer.OrdinalIgnoreCase);

		foreach (var file in Directory.EnumerateFiles (options.SymbolPackagesDirectory, "*.symbols.nupkg")) {
			var name = Path.GetFileName (file);
			var packageId = versions.Keys.FirstOrDefault (id =>
				name.StartsWith (id + ".", StringComparison.OrdinalIgnoreCase) && !known.Contains (id));

			if (packageId is not null && packageId.Contains (".NativeAssets.", StringComparison.OrdinalIgnoreCase))
				log.Error ($"'{name}' is a native assets symbol package that this validator does not know how to validate; extend PackageMatrix.");
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
