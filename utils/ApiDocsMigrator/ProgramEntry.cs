namespace ApiDocsMigrator;

internal static class ProgramEntry
{
	public static async Task<int> RunAsync(string[] args)
	{
		if (args.Length == 1 && args[0] == "--self-test")
			return await MigratorSelfTest.RunAsync();

		try
		{
			var options = MigrationOptions.Parse(args);
			if (options.ComparisonXmlPath is not null)
			{
				var comparison = XmlParityComparer.Compare(options.PackageXmlPath!, options.ComparisonXmlPath);
				Console.WriteLine(comparison.Format());
				return comparison.HasDifferences ? 1 : 0;
			}

			var result = options.PackageXmlPath is null
				? await new DocumentationMigrator(options).RunAsync()
				: await new PackageDocumentationMigrator(options).RunAsync();
			var inputKind = options.PackageXmlPath is null ? "ECMA" : "package XML";
			Console.WriteLine($"Migrated {result.UpdatedDocumentCount} documentation comments in {result.UpdatedFileCount} files. " +
				$"Parsed {result.ParsedDocumentationCount} {inputKind} entries for {result.DeclarationCount} declarations in {result.Duration.TotalSeconds:F2}s.");
			if (result.Unresolved.Count == 0)
				return 0;

			Console.Error.WriteLine("Documentation migration completed with unresolved DocIds:");
			foreach (var unresolved in result.Unresolved.OrderBy(error => error, StringComparer.Ordinal))
				Console.Error.WriteLine($"  {unresolved}");
			return 1;
		}
		catch (MigrationException ex)
		{
			Console.Error.WriteLine(ex.Message);
			return 1;
		}
		catch (ArgumentException ex)
		{
			Console.Error.WriteLine(ex.Message);
			Console.Error.WriteLine(MigrationOptions.Usage);
			return 2;
		}
	}
}
