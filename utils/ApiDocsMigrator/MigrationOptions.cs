namespace ApiDocsMigrator;

internal sealed record MigrationOptions(
	string DocumentationRoot,
	string SourceRoot,
	bool DryRun,
	bool ApplyResolved,
	string? PackageXmlPath = null,
	string? ComparisonXmlPath = null,
	string? ProjectPath = null,
	string? Framework = null,
	bool OnlyUndocumented = false)
{
	public const string Usage =
		"Usage: dotnet run --project utils/ApiDocsMigrator -- (--docs <SkiaSharp-API-docs path> | --package-xml <NuGet XML path>) --source <source directory, e.g. binding/SkiaSharp> [--dry-run] [--apply-resolved] [--only-undocumented]\n" +
		"   or: dotnet run --project utils/ApiDocsMigrator -- --package-xml <NuGet XML path> --project <project file> --framework <target framework> [--dry-run] [--apply-resolved]\n" +
		"   or: dotnet run --project utils/ApiDocsMigrator -- --package-xml <NuGet XML path> --compare-xml <compiled XML path>\n" +
		"   or: dotnet run --project utils/ApiDocsMigrator -- --format-source <source directory>";

	public static MigrationOptions Parse(IReadOnlyList<string> args)
	{
		string? documentationRoot = null;
		string? packageXmlPath = null;
		string? comparisonXmlPath = null;
		string? projectPath = null;
		string? framework = null;
		string? sourceRoot = null;
		var dryRun = false;
		var applyResolved = false;
		var onlyUndocumented = false;

		for (var i = 0; i < args.Count; i++)
		{
			switch (args[i])
			{
				case "--docs" when i + 1 < args.Count:
					documentationRoot = Path.GetFullPath(args[++i]);
					break;
				case "--source" when i + 1 < args.Count:
					sourceRoot = Path.GetFullPath(args[++i]);
					break;
				case "--package-xml" when i + 1 < args.Count:
					packageXmlPath = Path.GetFullPath(args[++i]);
					break;
				case "--compare-xml" when i + 1 < args.Count:
					comparisonXmlPath = Path.GetFullPath(args[++i]);
					break;
				case "--project" when i + 1 < args.Count:
					projectPath = Path.GetFullPath(args[++i]);
					break;
				case "--framework" when i + 1 < args.Count:
					framework = args[++i];
					break;
				case "--dry-run":
					dryRun = true;
					break;
				case "--apply-resolved":
					applyResolved = true;
					break;
				case "--only-undocumented":
					onlyUndocumented = true;
					break;
				default:
					throw new ArgumentException($"Unknown or incomplete option: {args[i]}.");
			}
		}

		if (comparisonXmlPath is null && (sourceRoot is null && projectPath is null || (documentationRoot is null && packageXmlPath is null)))
			throw new ArgumentException("--source or --project and exactly one of --docs or --package-xml are required.");
		if (documentationRoot is not null && packageXmlPath is not null)
			throw new ArgumentException("--docs and --package-xml cannot be used together.");
		if (comparisonXmlPath is not null && (packageXmlPath is null || documentationRoot is not null))
			throw new ArgumentException("--compare-xml requires --package-xml and cannot be used with --docs.");
		if (projectPath is not null && packageXmlPath is null && documentationRoot is null)
			throw new ArgumentException("--project requires --package-xml or --docs.");
		if (framework is not null && projectPath is null)
			throw new ArgumentException("--framework requires --project.");
		if (documentationRoot is not null && !Directory.Exists(documentationRoot))
			throw new ArgumentException($"Documentation root does not exist: {documentationRoot}");
		if (packageXmlPath is not null && !File.Exists(packageXmlPath))
			throw new ArgumentException($"Package XML does not exist: {packageXmlPath}");
		if (comparisonXmlPath is not null && !File.Exists(comparisonXmlPath))
			throw new ArgumentException($"Compiled XML does not exist: {comparisonXmlPath}");
		if (projectPath is not null && !File.Exists(projectPath))
			throw new ArgumentException($"Project does not exist: {projectPath}");
		if (sourceRoot is not null && !Directory.Exists(sourceRoot))
			throw new ArgumentException($"Source root does not exist: {sourceRoot}");

		return new MigrationOptions(
			documentationRoot ?? string.Empty,
			sourceRoot ?? Path.GetDirectoryName(projectPath!)!,
			dryRun,
			applyResolved,
			packageXmlPath,
			comparisonXmlPath,
			projectPath,
			framework,
			onlyUndocumented);
	}
}
