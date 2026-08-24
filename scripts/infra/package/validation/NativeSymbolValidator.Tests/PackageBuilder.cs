using System.IO.Compression;

namespace SkiaSharp.PackageValidation.Tests;

/// <summary>
/// Builds real <c>.nupkg</c> archives on disk so the validator is exercised through exactly the
/// code path it uses in CI: opening a zip and reading its nuspec and payload.
/// </summary>
public sealed class PackageBuilder
{
	private readonly Dictionary<string, byte[]> files = new (StringComparer.OrdinalIgnoreCase);

	public PackageBuilder (string id, string version)
	{
		Id = id;
		Version = version;
	}

	public string Id { get; }

	public string Version { get; }

	public PackageBuilder Add (string entry, byte[] content)
	{
		files[entry] = content;
		return this;
	}

	public PackageBuilder Add (string entry, string content) =>
		Add (entry, System.Text.Encoding.UTF8.GetBytes (content));

	public PackageBuilder Remove (string entry)
	{
		files.Remove (entry);
		return this;
	}

	public bool Contains (string entry) => files.ContainsKey (entry);

	public PackageBuilder Copy (string id, string version)
	{
		var copy = new PackageBuilder (id, version);
		foreach (var file in files)
			copy.files[file.Key] = file.Value;
		return copy;
	}

	public string Save (string directory, string? fileName = null)
	{
		Directory.CreateDirectory (directory);
		var path = Path.Combine (directory, fileName ?? $"{Id}.{Version}.nupkg");

		using var stream = File.Create (path);
		using var archive = new ZipArchive (stream, ZipArchiveMode.Create);

		Write (archive, "[Content_Types].xml",
			"""<?xml version="1.0" encoding="utf-8"?><Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types"><Default Extension="nuspec" ContentType="text/xml" /></Types>""");
		Write (archive, "_rels/.rels",
			"""<?xml version="1.0" encoding="utf-8"?><Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships" />""");
		Write (archive, $"{Id}.nuspec", Nuspec ());

		foreach (var file in files) {
			var entry = archive.CreateEntry (file.Key, CompressionLevel.NoCompression);
			using var entryStream = entry.Open ();
			entryStream.Write (file.Value);
		}

		return path;
	}

	private string Nuspec () =>
		$"""
		<?xml version="1.0" encoding="utf-8"?>
		<package xmlns="http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd">
		  <metadata>
		    <id>{Id}</id>
		    <version>{Version}</version>
		    <authors>Microsoft</authors>
		    <description>Test fixture.</description>
		  </metadata>
		</package>
		""";

	private static void Write (ZipArchive archive, string name, string content)
	{
		var entry = archive.CreateEntry (name, CompressionLevel.NoCompression);
		using var stream = entry.Open ();
		stream.Write (System.Text.Encoding.UTF8.GetBytes (content));
	}
}
