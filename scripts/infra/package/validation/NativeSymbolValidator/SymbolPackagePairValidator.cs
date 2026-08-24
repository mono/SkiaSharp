namespace SkiaSharp.PackageValidation;

/// <summary>
/// Checks that hold for every native symbol package regardless of platform: the symbol package
/// must be a faithful superset of the customer package it shadows.
/// </summary>
public static class SymbolPackagePairValidator
{
	/// <summary>
	/// Metadata that identifies the package to NuGet and to consumers. The symbol package is
	/// produced by a second pack of the same project, so all of these must survive unchanged.
	/// </summary>
	private static readonly string[] PreservedMetadata =
	{
		"id",
		"version",
		"authors",
		"description",
		"projectUrl",
		"copyright",
		"tags",
		"requireLicenseAcceptance",
		"repository",
	};

	public static void Validate (NuGetPackage normal, NuGetPackage symbols, ValidationLog log)
	{
		ValidateIdentity (normal, symbols, log);
		ValidateMetadata (normal, symbols, log);
		ValidatePayloadPreserved (normal, symbols, log);
	}

	private static void ValidateIdentity (NuGetPackage normal, NuGetPackage symbols, ValidationLog log)
	{
		log.Check (
			string.Equals (normal.Id, symbols.Id, StringComparison.OrdinalIgnoreCase),
			$"symbol package id '{symbols.Id}' does not match package id '{normal.Id}'.");

		log.Check (
			string.Equals (normal.Version, symbols.Version, StringComparison.OrdinalIgnoreCase),
			$"symbol package version '{symbols.Version}' does not match package version '{normal.Version}'.");
	}

	private static void ValidateMetadata (NuGetPackage normal, NuGetPackage symbols, ValidationLog log)
	{
		foreach (var field in PreservedMetadata) {
			var expected = normal.GetMetadata (field);
			var actual = symbols.GetMetadata (field);

			if (expected is null && actual is null)
				continue;

			log.Check (
				string.Equals (expected, actual, StringComparison.Ordinal),
				$"symbol package metadata <{field}> is '{actual ?? "<missing>"}' but the package has '{expected ?? "<missing>"}'.");
		}

		// Dependencies decide what a consumer restores alongside the package, so a symbol package
		// that dropped or altered them would not be a faithful shadow of the customer package.
		var expectedDependencies = DescribeDependencies (normal);
		var actualDependencies = DescribeDependencies (symbols);
		log.Check (
			expectedDependencies.SequenceEqual (actualDependencies, StringComparer.Ordinal),
			$"symbol package dependencies [{string.Join ("; ", actualDependencies)}] do not match package dependencies [{string.Join ("; ", expectedDependencies)}].");
	}

	private static void ValidatePayloadPreserved (NuGetPackage normal, NuGetPackage symbols, ValidationLog log)
	{
		var missing = new List<string> ();
		var different = new List<string> ();

		foreach (var entry in normal.PayloadEntries) {
			if (!symbols.Contains (entry)) {
				missing.Add (entry);
				continue;
			}

			if (!normal.ContentEquals (entry, symbols, entry))
				different.Add (entry);
		}

		log.Check (missing.Count == 0, $"symbol package is missing {missing.Count} package file(s): {Describe (missing)}.");
		log.Check (different.Count == 0, $"symbol package has {different.Count} file(s) whose content differs from the package: {Describe (different)}.");
	}

	private static string[] DescribeDependencies (NuGetPackage package)
	{
		var dependencies = package.Metadata.Elements ().FirstOrDefault (e => e.Name.LocalName == "dependencies");
		if (dependencies is null)
			return Array.Empty<string> ();

		var described = new List<string> ();

		foreach (var group in dependencies.Elements ().Where (e => e.Name.LocalName == "group")) {
			var targetFramework = group.Attribute ("targetFramework")?.Value ?? "";
			foreach (var dependency in group.Elements ().Where (e => e.Name.LocalName == "dependency"))
				described.Add ($"{targetFramework}:{dependency.Attribute ("id")?.Value}/{dependency.Attribute ("version")?.Value}");

			if (!group.Elements ().Any (e => e.Name.LocalName == "dependency"))
				described.Add ($"{targetFramework}:<none>");
		}

		foreach (var dependency in dependencies.Elements ().Where (e => e.Name.LocalName == "dependency"))
			described.Add ($":{dependency.Attribute ("id")?.Value}/{dependency.Attribute ("version")?.Value}");

		described.Sort (StringComparer.Ordinal);
		return described.ToArray ();
	}

	internal static string Describe (IReadOnlyList<string> items, int limit = 5) =>
		items.Count == 0
			? "<none>"
			: string.Join (", ", items.Take (limit)) + (items.Count > limit ? $", ... (+{items.Count - limit} more)" : "");
}
