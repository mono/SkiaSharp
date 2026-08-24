using System.IO.Compression;
using System.Xml.Linq;

namespace SkiaSharp.PackageValidation;

/// <summary>
/// Read-only view over a packed <c>.nupkg</c>. Everything the validator asserts is read from the
/// real archive produced by <c>pack</c>, never from the source tree.
/// </summary>
public sealed class NuGetPackage : IDisposable
{
	// OPC plumbing that NuGet regenerates per pack invocation. It is never part of the payload a
	// consumer restores, so it is excluded from payload preservation comparisons.
	private static readonly string[] OpcPrefixes =
	{
		"_rels/",
		"package/services/",
	};

	private readonly ZipArchive archive;
	private readonly Dictionary<string, ZipArchiveEntry> entries;

	private NuGetPackage (string path, ZipArchive archive)
	{
		FilePath = path;
		FileName = Path.GetFileName (path);
		this.archive = archive;

		entries = new Dictionary<string, ZipArchiveEntry> (StringComparer.OrdinalIgnoreCase);
		foreach (var entry in archive.Entries) {
			// Directory markers have no content and are not meaningful for validation.
			if (entry.FullName.EndsWith ('/'))
				continue;
			entries[Normalize (entry.FullName)] = entry;
		}

		Entries = entries.Keys.OrderBy (e => e, StringComparer.Ordinal).ToArray ();

		var nuspecName = Entries.FirstOrDefault (e =>
			e.EndsWith (".nuspec", StringComparison.OrdinalIgnoreCase) && !e.Contains ('/'));
		if (nuspecName is null)
			throw new InvalidDataException ($"'{FileName}' does not contain a .nuspec at the package root.");

		NuspecEntryName = nuspecName;

		using var nuspecStream = entries[nuspecName].Open ();
		var document = XDocument.Load (nuspecStream);
		Metadata = document.Root?.Elements ().FirstOrDefault (e => e.Name.LocalName == "metadata")
			?? throw new InvalidDataException ($"'{FileName}' has a .nuspec without a <metadata> element.");

		Id = GetMetadata ("id") ?? throw new InvalidDataException ($"'{FileName}' has a .nuspec without an <id>.");
		Version = GetMetadata ("version") ?? throw new InvalidDataException ($"'{FileName}' has a .nuspec without a <version>.");
	}

	public string FilePath { get; }

	public string FileName { get; }

	public string Id { get; }

	public string Version { get; }

	public XElement Metadata { get; }

	public string NuspecEntryName { get; }

	public IReadOnlyList<string> Entries { get; }

	public static NuGetPackage Open (string path) =>
		new (path, ZipFile.OpenRead (path));

	public static string Normalize (string entry) =>
		entry.Replace ('\\', '/').TrimStart ('/');

	/// <summary>
	/// Entries that make up the payload a consumer actually restores.
	/// </summary>
	public IEnumerable<string> PayloadEntries =>
		Entries.Where (e =>
			!string.Equals (e, "[Content_Types].xml", StringComparison.OrdinalIgnoreCase) &&
			!string.Equals (e, NuspecEntryName, StringComparison.OrdinalIgnoreCase) &&
			!OpcPrefixes.Any (p => e.StartsWith (p, StringComparison.OrdinalIgnoreCase)));

	public string? GetMetadata (string localName) =>
		Metadata.Elements ().FirstOrDefault (e => e.Name.LocalName == localName)?.Value?.Trim ();

	public bool Contains (string entry) =>
		entries.ContainsKey (Normalize (entry));

	public IEnumerable<string> EntriesUnder (string prefix)
	{
		prefix = Normalize (prefix).TrimEnd ('/') + "/";
		return Entries.Where (e => e.StartsWith (prefix, StringComparison.OrdinalIgnoreCase));
	}

	public long GetLength (string entry) =>
		entries[Normalize (entry)].Length;

	public byte[] ReadAllBytes (string entry)
	{
		var zipEntry = entries[Normalize (entry)];
		using var stream = zipEntry.Open ();
		using var buffer = new MemoryStream ();
		stream.CopyTo (buffer);
		return buffer.ToArray ();
	}

	/// <summary>
	/// Extracts an entry to disk. Native binaries and dSYM payloads can be very large and the
	/// binary readers need seekable streams, so they are staged as files rather than buffered.
	/// </summary>
	public string ExtractTo (string entry, string destinationDirectory)
	{
		var zipEntry = entries[Normalize (entry)];
		Directory.CreateDirectory (destinationDirectory);
		var destination = Path.Combine (destinationDirectory, Guid.NewGuid ().ToString ("n"));
		zipEntry.ExtractToFile (destination, overwrite: true);
		return destination;
	}

	/// <summary>
	/// Compares entry contents without materializing either side in memory.
	/// </summary>
	public bool ContentEquals (string entry, NuGetPackage other, string otherEntry)
	{
		var left = entries[Normalize (entry)];
		var right = other.entries[Normalize (otherEntry)];

		if (left.Length != right.Length)
			return false;

		using var leftStream = left.Open ();
		using var rightStream = right.Open ();

		var leftBuffer = new byte[81920];
		var rightBuffer = new byte[81920];

		while (true) {
			var leftRead = ReadBlock (leftStream, leftBuffer);
			var rightRead = ReadBlock (rightStream, rightBuffer);

			if (leftRead != rightRead)
				return false;
			if (leftRead == 0)
				return true;
			if (!leftBuffer.AsSpan (0, leftRead).SequenceEqual (rightBuffer.AsSpan (0, rightRead)))
				return false;
		}

		static int ReadBlock (Stream stream, byte[] buffer)
		{
			var total = 0;
			while (total < buffer.Length) {
				var read = stream.Read (buffer, total, buffer.Length - total);
				if (read == 0)
					break;
				total += read;
			}
			return total;
		}
	}

	public void Dispose () => archive.Dispose ();
}
