using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SkiaSharp.Tests.Shared;

/// <summary>Resolves golden references from ordered disk roots and embedded-resource assemblies.</summary>
public sealed class GoldenStore
{
	public const string ResourcePrefix = "SkiaSharp.Goldens/";
	private readonly string[] diskRoots;
	private readonly Assembly[] resourceAssemblies;

	public GoldenStore(IEnumerable<string> diskRoots, IEnumerable<Assembly>? resourceAssemblies = null)
	{
		this.diskRoots = (diskRoots ?? throw new ArgumentNullException(nameof(diskRoots)))
			.Select(Path.GetFullPath).Distinct(StringComparer.Ordinal).ToArray();
		this.resourceAssemblies = (resourceAssemblies ?? Array.Empty<Assembly>()).Distinct().ToArray();
	}

	public GoldenResolution Resolve(GoldenRequest request)
	{
		if (request == null) throw new ArgumentNullException(nameof(request));
		var sources = new Dictionary<string, (string Location, Func<byte[]> Read)>(StringComparer.Ordinal);
		foreach (var root in diskRoots)
		{
			if (!Directory.Exists(root)) continue;
			var prefix = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
			foreach (var path in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories).OrderBy(p => p, StringComparer.Ordinal))
			{
				if (!path.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) continue;
				var key = path.Substring(prefix.Length).Replace(Path.DirectorySeparatorChar, '/');
				Add(key, path, () => File.ReadAllBytes(path));
			}
		}
		foreach (var assembly in resourceAssemblies)
		foreach (var resource in assembly.GetManifestResourceNames().OrderBy(n => n, StringComparer.Ordinal))
		{
			if (!resource.StartsWith(ResourcePrefix, StringComparison.Ordinal) ||
				!resource.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) continue;
			Add(resource.Substring(ResourcePrefix.Length), assembly.GetName().Name + "!" + resource, () =>
			{
				using var stream = assembly.GetManifestResourceStream(resource)
					?? throw new InvalidOperationException("Missing embedded golden: " + resource);
				using var memory = new MemoryStream();
				stream.CopyTo(memory);
				return memory.ToArray();
			});
		}

		var attempted = new List<string>();
		foreach (var platform in GoldenPlatform.FallbackChain(request.Platform))
		{
			if (request.Version != null)
			{
				var versions = sources.Keys
					.Where(k => k.StartsWith(platform + "/", StringComparison.Ordinal))
					.Select(GoldenPaths.ValidateKey)
					.Where(v => v != null && v >= request.Version)
					.Cast<Version>()
					.Distinct()
					.OrderBy(v => v);
				foreach (var version in versions)
				foreach (var file in request.FileNames())
				{
					var key = platform + "/" + version + "/" + file;
					attempted.Add(key);
					if (sources.TryGetValue(key, out var source))
						return new GoldenResolution(request, key, source.Location, attempted, source.Read);
				}
			}
			foreach (var file in request.FileNames())
			{
				var key = platform + "/" + file;
				attempted.Add(key);
				if (sources.TryGetValue(key, out var source))
					return new GoldenResolution(request, key, source.Location, attempted, source.Read);
			}
		}
		return new GoldenResolution(request, null, null, attempted, null);

		void Add(string key, string location, Func<byte[]> read)
		{
			GoldenPaths.ValidateKey(key);
			if (!sources.ContainsKey(key)) sources.Add(key, (location, read));
		}
	}
}
