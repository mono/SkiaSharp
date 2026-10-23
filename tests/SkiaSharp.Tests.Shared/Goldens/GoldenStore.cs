using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SkiaSharp.Tests.Shared;

/// <summary>Resolves golden cases from ordered disk roots and embedded-resource assemblies.</summary>
public sealed class GoldenStore
{
	public const string ResourcePrefix = "SkiaSharp.Goldens/";

	private readonly string[] diskRoots;
	private readonly Assembly[] resourceAssemblies;
	private readonly Lazy<Dictionary<string, Source>> sources;

	public GoldenStore(IEnumerable<string> diskRoots, IEnumerable<Assembly>? resourceAssemblies = null)
	{
		this.diskRoots = (diskRoots ?? throw new ArgumentNullException(nameof(diskRoots)))
			.Select(Path.GetFullPath).Distinct(StringComparer.Ordinal).ToArray();
		this.resourceAssemblies = (resourceAssemblies ?? Array.Empty<Assembly>()).Distinct().ToArray();
		sources = new Lazy<Dictionary<string, Source>>(EnumerateSources, isThreadSafe: true);
	}

	public Golden Resolve(GoldenCase goldenCase)
	{
		if (goldenCase == null)
			throw new ArgumentNullException(nameof(goldenCase));

		var fileNames = goldenCase.FileNames().ToArray();
		var sources = this.sources.Value;
		var attempted = new List<string>();
		foreach (var candidateProfile in goldenCase.FallbackChain())
		{
			if (goldenCase.Version != null)
			{
				var versions = sources
					.Where(source =>
						source.Key.StartsWith($"{candidateProfile}/", StringComparison.Ordinal) &&
						source.Value.Version != null &&
						source.Value.Version >= goldenCase.Version)
					.Select(source => source.Value.Version!)
					.Distinct()
					.OrderBy(value => value);
				foreach (var candidateVersion in versions)
				foreach (var fileName in fileNames)
				{
					var key = $"{candidateProfile}/{candidateVersion}/{fileName}";
					attempted.Add(key);
					if (sources.TryGetValue(key, out var source))
						return new Golden(goldenCase, key, source.Location, attempted, source.Read);
				}
			}

			foreach (var fileName in fileNames)
			{
				var key = $"{candidateProfile}/{fileName}";
				attempted.Add(key);
				if (sources.TryGetValue(key, out var source))
					return new Golden(goldenCase, key, source.Location, attempted, source.Read);
			}
		}

		return new Golden(goldenCase, null, null, attempted, null);
	}

	private Dictionary<string, Source> EnumerateSources()
	{
		var sources = new Dictionary<string, Source>(StringComparer.Ordinal);
		foreach (var root in diskRoots)
		{
			if (!Directory.Exists(root))
				continue;

			var prefix = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
			foreach (var path in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
				.OrderBy(path => path, StringComparer.Ordinal))
			{
				if (!path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
					continue;
				var key = path.Substring(prefix.Length).Replace(Path.DirectorySeparatorChar, '/');
				Add(key, path, () => File.ReadAllBytes(path));
			}
		}

		foreach (var assembly in resourceAssemblies)
		foreach (var resource in assembly.GetManifestResourceNames().OrderBy(name => name, StringComparer.Ordinal))
		{
			if (!resource.StartsWith(ResourcePrefix, StringComparison.Ordinal) || !resource.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
				continue;

			Add(resource.Substring(ResourcePrefix.Length), $"{assembly.GetName().Name}!{resource}", () =>
			{
				using var stream = assembly.GetManifestResourceStream(resource)
					?? throw new InvalidOperationException($"Missing embedded golden: {resource}");
				using var memory = new MemoryStream();
				stream.CopyTo(memory);
				return memory.ToArray();
			});
		}

		return sources;

		void Add(string key, string location, Func<byte[]> read)
		{
			var version = ValidateKey(key);
			if (!sources.ContainsKey(key))
				sources.Add(key, new Source(location, read, version));
		}
	}

	private static Version? ValidateKey(string key)
	{
		var parts = key.Split('/');
		if (parts.Length < 2 || !key.EndsWith(".png", StringComparison.Ordinal))
			throw new ArgumentException($"Expected profile/[version/]case[.variant][.architecture].png: {key}");

		GoldenCase.ValidateSegment(parts[0]);
		var caseStart = 1;
		Version? parsed = null;
		if (parts.Length >= 3 && Regex.IsMatch(parts[1], @"\A(0|[1-9][0-9]*)(\.(0|[1-9][0-9]*)){2,3}\z"))
		{
			if (!Version.TryParse(parts[1], out parsed))
				throw new ArgumentException($"Invalid System.Version: {key}");

			caseStart = 2;
		}
		else if (parts.Length >= 3 && parts[1].Contains(".") && char.IsDigit(parts[1][0]))
		{
			throw new ArgumentException($"Expected a normalized System.Version with three or four components: {key}");
		}

		foreach (var segment in parts.Skip(caseStart).Take(parts.Length - caseStart - 1))
			GoldenCase.ValidateSegment(segment, caseSegment: true);

		var names = parts[parts.Length - 1]
			.Substring(0, parts[parts.Length - 1].Length - 4)
			.Split('.');
		if (names.Length > 3)
			throw new ArgumentException($"Too many golden filename suffixes: {key}");

		GoldenCase.ValidateSegment(names[0], caseSegment: true);
		foreach (var suffix in names.Skip(1))
			GoldenCase.ValidateSegment(suffix);
		if (names.Length == 3 && (GoldenCase.IsArchitecture(names[1]) || !GoldenCase.IsArchitecture(names[2])))
			throw new ArgumentException($"Variant must precede a supported .NET architecture: {key}");
		return parsed;
	}

	private sealed record Source(string Location, Func<byte[]> Read, Version? Version);
}
