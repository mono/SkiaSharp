using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ApiDocsMigrator;

internal static class MigratorSelfTest
{
	public static async Task<int> RunAsync()
	{
		var root = Path.Combine(Directory.GetCurrentDirectory(), $".api-docs-migrator-self-test-{Guid.NewGuid():N}");
		Directory.CreateDirectory(root);
		try
		{
			var sourcePath = Path.Combine(root, "Widget.cs");
			var generatedPath = Path.Combine(root, "Generated.generated.cs");
			var packageXmlPath = Path.Combine(root, "SkiaSharp.xml");
			await File.WriteAllTextAsync(sourcePath, """
				namespace Fixtures;

				public sealed partial class Widget
				{
				    public Widget(string name) => Name = name;

				    public string Name { get; }

				    public static Widget Create(int value) => new(value.ToString());
				}
				""");
			await File.WriteAllTextAsync(Path.Combine(root, "Widget.Platform.cs"), """
				namespace Fixtures;

				/// <summary>Old widget docs.</summary>
				public sealed partial class Widget
				{
				}
				""");
			await File.WriteAllTextAsync(generatedPath, """
				namespace Fixtures;

				public enum Generated
				{
				    Value,
				}
				""");
			await File.WriteAllTextAsync(packageXmlPath, """
				<?xml version="1.0"?>
				<doc>
				  <assembly><name>Fixtures</name></assembly>
				  <members>
				    <member name="T:Fixtures.Widget">
				      <summary>Represents a widget.</summary>
				      <remarks><format type="text/markdown"><![CDATA[
				## Remarks

				Use a widget when an example requires one.
				]]></format></remarks>
				    </member>
				    <member name="C:Fixtures.Widget(System.String)">
				      <param name="name">The widget name.</param>
				      <summary>Initializes a widget.</summary>
				      <remarks>Stores <paramref name="name&amp;nbsp;" />.</remarks>
				    </member>
				    <member name="P:Fixtures.Widget.Name">
				      <summary>Gets the widget name.</summary>
				    </member>
				    <member name="M:Fixtures.Widget.Create(System.Int32)">
				      <summary>Creates a widget from a numeric value.</summary>
				      <param name="value">The numeric value.</param>
				      <returns>The new widget.</returns>
				    </member>
				    <member name="T:Fixtures.Generated">
				      <summary>Represents a generated enumeration.</summary>
				    </member>
				    <member name="F:Fixtures.Generated.Value">
				      <summary>Represents the generated value.</summary>
				    </member>
				    <member name="C:Fixtures.Widget()" />
				  </members>
				</doc>
				""");

			var options = new MigrationOptions(
				DocumentationRoot: string.Empty,
				SourceRoot: root,
				DryRun: false,
				ApplyResolved: false,
				PackageXmlPath: packageXmlPath);
			var first = await new PackageDocumentationMigrator(options).RunAsync();
			var firstSource = await File.ReadAllTextAsync(sourcePath);
			var firstPlatform = await File.ReadAllTextAsync(Path.Combine(root, "Widget.Platform.cs"));
			var firstGenerated = await File.ReadAllTextAsync(generatedPath);
			var second = await new PackageDocumentationMigrator(options).RunAsync();
			var secondSource = await File.ReadAllTextAsync(sourcePath);
			var secondGenerated = await File.ReadAllTextAsync(generatedPath);

			if (first.UpdatedDocumentCount != 6 ||
				first.UpdatedFileCount != 3 ||
				second.UpdatedFileCount != 0 ||
				firstSource != secondSource ||
				firstGenerated != secondGenerated ||
				!firstSource.Contains("/// <summary>Represents a widget.</summary>", StringComparison.Ordinal) ||
				firstPlatform.Contains("///", StringComparison.Ordinal) ||
				!firstSource.Contains("type=\"text/markdown\"", StringComparison.Ordinal) ||
				!firstSource.Contains("<![CDATA[", StringComparison.Ordinal) ||
				!firstSource.Contains("/// <returns>The new widget.</returns>", StringComparison.Ordinal) ||
				!firstSource.Contains("<paramref name=\"name\" />", StringComparison.Ordinal) ||
				firstSource.Contains("&amp;nbsp;", StringComparison.Ordinal) ||
				firstSource.IndexOf("/// <summary>Initializes a widget.</summary>", StringComparison.Ordinal) >
					firstSource.IndexOf("/// <param name=\"name\">The widget name.</param>", StringComparison.Ordinal) ||
				!firstGenerated.Contains("/// <summary>Represents a generated enumeration.</summary>", StringComparison.Ordinal) ||
				!firstGenerated.Contains("/// <summary>Represents the generated value.</summary>", StringComparison.Ordinal) ||
				HasSyntaxErrors(firstSource) ||
				HasSyntaxErrors(firstGenerated))
			{
				Console.Error.WriteLine(
					$"Self-test failed: package XML migration was not complete, valid, or idempotent. " +
					$"First: docs={first.UpdatedDocumentCount}, files={first.UpdatedFileCount}, unresolved={first.Unresolved.Count}; " +
					$"second: docs={second.UpdatedDocumentCount}, files={second.UpdatedFileCount}, unresolved={second.Unresolved.Count}; " +
					$"widget={firstSource.Contains("/// <summary>Represents a widget.</summary>", StringComparison.Ordinal)}, " +
					$"markdown={firstSource.Contains("type=\"text/markdown\"", StringComparison.Ordinal) && firstSource.Contains("<![CDATA[", StringComparison.Ordinal)}, " +
					$"returns={firstSource.Contains("/// <returns>The new widget.</returns>", StringComparison.Ordinal)}, " +
					$"generated-type={firstGenerated.Contains("/// <summary>Represents a generated enumeration.</summary>", StringComparison.Ordinal)}, " +
					$"generated-member={firstGenerated.Contains("/// <summary>Represents the generated value.</summary>", StringComparison.Ordinal)}, " +
					$"source-errors={HasSyntaxErrors(firstSource)}, generated-errors={HasSyntaxErrors(firstGenerated)}.");
				return 1;
			}

			Console.WriteLine("Self-test passed: package XML migration, Markdown CDATA, generated declarations, and idempotence.");
			return 0;
		}
		finally
		{
			Directory.Delete(root, recursive: true);
		}
	}

	private static bool HasSyntaxErrors(string source) =>
		CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(documentationMode: DocumentationMode.Diagnose))
			.GetDiagnostics()
			.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
}
